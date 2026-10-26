using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RockProjectile : MonoBehaviour
{
    public static readonly List<RockProjectile> All = new List<RockProjectile>();

    [SerializeField] private float impactDamage = 2f;        // урон ровера от камня босса
    [SerializeField] private float playerThrowDamage = 3f;   // урон боссу от камня ровера
    [SerializeField] private float ignoreOwnerTime = 0.5f;   // сколько после вылета камень не задевает бросающего

    private Rigidbody rb;
    private Collider col;
    private bool armed;
    private bool held;
    private bool thrownByPlayer;

    public bool IsAvailable => !held && !armed && !rb.isKinematic;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    private void OnEnable() => All.Add(this);
    private void OnDisable() => All.Remove(this);

    public static RockProjectile FindNearest(Vector3 from)
    {
        RockProjectile best = null;
        float bestDist = float.MaxValue;
        foreach (var r in All)
        {
            if (!r.IsAvailable) continue;
            float d = (r.transform.position - from).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = r; }
        }
        return best;
    }

    public void Grab()
    {
        held = true;
        armed = false;
        rb.isKinematic = true;
        col.enabled = false;
    }

    // Бросок босса
    public void Release(Vector3 velocity) => ReleaseInternal(velocity, false);

    // Бросок ровера
    public void ReleaseByPlayer(Vector3 velocity) => ReleaseInternal(velocity, true);

    private void ReleaseInternal(Vector3 velocity, bool byPlayer)
    {
        thrownByPlayer = byPlayer;
        held = false;
        transform.SetParent(null);

        col.enabled = true;

        var ignored = CollectOwnerColliders(byPlayer);
        SetIgnore(ignored, true);

        rb.isKinematic = false;
        rb.linearVelocity = velocity;
        rb.angularVelocity = Random.insideUnitSphere * 3f;
        armed = true;

        if (ignored.Count > 0) StartCoroutine(RestoreCollision(ignored));
    }

    private List<Collider> CollectOwnerColliders(bool byPlayer)
    {
        var result = new List<Collider>();

        Transform root = null;
        if (byPlayer)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) root = p.transform;
        }
        else
        {
            var boss = FindFirstObjectByType<BossController>();
            if (boss != null) root = boss.transform;
        }
        if (root == null) return result;

        foreach (var c in root.GetComponentsInChildren<Collider>(false))
        {
            if (c == null || !c.enabled || c.isTrigger) continue;
            if (c.GetComponentInParent<BossDisc>() != null) continue;
            result.Add(c);
        }
        return result;
    }

    private void SetIgnore(List<Collider> list, bool ignore)
    {
        foreach (var c in list)
            if (c != null && c.enabled && c.gameObject.activeInHierarchy)
                Physics.IgnoreCollision(col, c, ignore);
    }

    private IEnumerator RestoreCollision(List<Collider> list)
    {
        yield return new WaitForSeconds(ignoreOwnerTime);
        if (col != null) SetIgnore(list, false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!armed) return;

        bool hitBoss = collision.collider.GetComponentInParent<BossController>() != null;

        // колонны камнем не разрушаются
        if (collision.collider.GetComponentInParent<DestructibleColumn>() != null)
        {
            armed = false;
            return;
        }

        if (thrownByPlayer)
        {
            if (hitBoss)
            {
                var boss = collision.collider.GetComponentInParent<IDamageable>();
                if (boss != null) boss.TakeDamage(playerThrowDamage);

                var fb = collision.collider.GetComponentInParent<BossHitFeedback>();
                if (fb != null)
                {
                    ContactPoint c = collision.GetContact(0);
                    fb.Play(c.point, c.normal);
                }
            }
            armed = false;
            thrownByPlayer = false;
            return;
        }

        if (hitBoss) return;

        var target = collision.collider.GetComponentInParent<IDamageable>();
        if (target != null) target.TakeImpactDamage(impactDamage);

        armed = false;
    }
}