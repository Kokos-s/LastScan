using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DrillHitbox : MonoBehaviour
{
    [SerializeField] private float damage = 2f;

    private readonly HashSet<IDamageable> alreadyHit = new HashSet<IDamageable>();

    private void OnEnable() => alreadyHit.Clear();

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<BossController>() != null) return;

        var target = other.GetComponentInParent<IDamageable>();
        if (target == null) return;

        if (alreadyHit.Add(target))
            target.TakeDamage(damage);
    }
}