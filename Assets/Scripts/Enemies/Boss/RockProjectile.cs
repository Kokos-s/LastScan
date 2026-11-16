using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RockProjectile : MonoBehaviour
{
    public static readonly List<RockProjectile> All = new List<RockProjectile>();

    [SerializeField] private float impactDamage = 2f;        // урон ровера от камня босса
    [SerializeField] private float playerThrowDamage = 3f;   // урон боссу от предмета, брошенного ровером
    [SerializeField] private float ignoreOwnerTime = 0.5f;
    [SerializeField] private bool bossCanTake = true;        // false = только игрок (части остова ровера)

    [Header("Одноразовый камень")]
    [SerializeField] private float breakDelay = 0.05f;
    [SerializeField] private ParticleSystem breakFx;
    [SerializeField] private float breakFxScale = 4f;
    [SerializeField] private float maxFlightTime = 8f;

    private Rigidbody rb;
    private Collider[] cols;
    private bool armed;
    private bool held;
    private bool thrownByPlayer;
    private bool consumed;
    private float launchTime;

    // части, скреплённые шарнирами с этой (остальной остов)
    private readonly List<Collider> attachedCols = new List<Collider>();

    public bool IsAvailableToPlayer => !held && !armed && !consumed && !rb.isKinematic;
    public bool IsAvailable => IsAvailableToPlayer && bossCanTake;
    public bool IsThrown => armed && !consumed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cols = GetComponentsInChildren<Collider>(true);
    }

    // Вызывается для частей остова: босс их не берёт, игрок берёт
    public void MarkPlayerOnly()
    {
        bossCanTake = false;
    }

    private void OnEnable() => All.Add(this);
    private void OnDisable() => All.Remove(this);

    private void Update()
    {
        if (armed && !consumed && Time.time - launchTime > maxFlightTime)
            Consume();
    }

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

    private void SetCollidersEnabled(bool value)
    {
        foreach (var c in cols)
            if (c != null) c.enabled = value;
    }

    // Все тела, связанные с этим цепочкой шарниров (остальной остов)
    private void CollectAttachedColliders()
    {
        attachedCols.Clear();

        var group = new HashSet<Rigidbody> { rb };
        var joints = FindObjectsByType<Joint>(FindObjectsSortMode.None);

        bool grew = true;
        while (grew)
        {
            grew = false;
            foreach (var j in joints)
            {
                if (j == null) continue;
                Rigidbody owner = j.GetComponent<Rigidbody>();
                Rigidbody other = j.connectedBody;
                if (owner == null || other == null) continue;

                if (group.Contains(owner) && group.Add(other)) grew = true;
                else if (group.Contains(other) && group.Add(owner)) grew = true;
            }
        }

        foreach (var body in group)
        {
            if (body == rb || body == null) continue;
            foreach (var c in body.GetComponentsInChildren<Collider>(false))
                if (c != null && !c.isTrigger) attachedCols.Add(c);
        }
    }

    // Убираем шарниры, которые ссылаются на это тело: иначе после его исчезновения
    // Unity привяжет соседей к точке в мире и они зависнут в воздухе
    private void ReleaseJointsToMe()
    {
        foreach (var j in FindObjectsByType<Joint>(FindObjectsSortMode.None))
            if (j != null && j.connectedBody == rb)
                Destroy(j);
    }

    private static Transform FindPlayerRoot()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        return p != null ? p.transform : null;
    }

    private void IgnoreAttachedWithPlayer(bool ignore)
    {
        Transform root = FindPlayerRoot();
        if (root == null) return;

        var playerCols = root.GetComponentsInChildren<Collider>(false);
        foreach (var a in attachedCols)
        {
            if (a == null || !a.enabled) continue;
            foreach (var pc in playerCols)
                if (pc != null && pc.enabled && !pc.isTrigger)
                    Physics.IgnoreCollision(a, pc, ignore);
        }
    }

    public void Grab()
    {
        held = true;
        armed = false;

        // шарниры остаются: остальной остов висит на подобранной части
        CollectAttachedColliders();
        IgnoreAttachedWithPlayer(true);

        rb.isKinematic = true;
        SetCollidersEnabled(false);
    }

    public void Release(Vector3 velocity) => ReleaseInternal(velocity, false);

    public void ReleaseByPlayer(Vector3 velocity) => ReleaseInternal(velocity, true);

    private void ReleaseInternal(Vector3 velocity, bool byPlayer)
    {
        thrownByPlayer = byPlayer;
        held = false;
        transform.SetParent(null);

        SetCollidersEnabled(true);

        var ignored = CollectOwnerColliders(byPlayer);
        SetIgnore(ignored, true);

        rb.isKinematic = false;
        rb.linearVelocity = velocity;
        rb.angularVelocity = Random.insideUnitSphere * 3f;
        armed = true;
        launchTime = Time.time;

        StartCoroutine(RestoreCollision(ignored));
    }

    private List<Collider> CollectOwnerColliders(bool byPlayer)
    {
        var result = new List<Collider>();

        Transform root = null;
        if (byPlayer) root = FindPlayerRoot();
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
        foreach (var mine in cols)
        {
            if (mine == null || !mine.enabled) continue;
            foreach (var c in list)
                if (c != null && c.enabled && c.gameObject.activeInHierarchy)
                    Physics.IgnoreCollision(mine, c, ignore);
        }
    }

    private IEnumerator RestoreCollision(List<Collider> list)
    {
        yield return new WaitForSeconds(ignoreOwnerTime);
        SetIgnore(list, false);
        IgnoreAttachedWithPlayer(false);
        attachedCols.Clear();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!armed || consumed) return;

        bool hitBoss = collision.collider.GetComponentInParent<BossController>() != null;

        if (!thrownByPlayer && hitBoss) return;

        if (collision.collider.GetComponentInParent<DestructibleColumn>() != null)
        {
            Consume();
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
        }
        else
        {
            var target = collision.collider.GetComponentInParent<IDamageable>();
            if (target != null) target.TakeImpactDamage(impactDamage);
        }

        Consume();
    }

    private void Consume()
    {
        if (consumed) return;
        consumed = true;
        armed = false;

        ReleaseJointsToMe();

        if (breakFx != null)
        {
            var fx = Instantiate(breakFx, transform.position, Quaternion.identity);
            fx.transform.localScale *= breakFxScale;
            fx.Play();
            Destroy(fx.gameObject, 3f);
        }

        Destroy(gameObject, breakDelay);
    }
}