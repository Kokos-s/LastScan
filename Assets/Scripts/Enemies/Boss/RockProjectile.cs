using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RockProjectile : MonoBehaviour
{
    public static readonly List<RockProjectile> All = new List<RockProjectile>();

    [SerializeField] private float damage = 15f;

    private Rigidbody rb;
    private Collider col;
    private bool armed;
    private bool held;

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

    public void Release(Vector3 velocity)
    {
        held = false;
        transform.SetParent(null);
        col.enabled = true;
        rb.isKinematic = false;
        rb.linearVelocity = velocity;
        rb.angularVelocity = Random.insideUnitSphere * 3f;
        armed = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!armed) return;
        if (collision.collider.GetComponentInParent<BossController>() != null) return;

        var target = collision.collider.GetComponentInParent<IDamageable>();
        if (target != null) target.TakeDamage(damage);

        armed = false;
    }
}