using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DrillHitbox : MonoBehaviour
{
    // Поле названо по-новому, чтобы старое значение из Inspector не подтянулось
    [SerializeField] private float hitDamage = 5f;

    private readonly HashSet<IDamageable> alreadyHit = new HashSet<IDamageable>();

    private void OnEnable() => alreadyHit.Clear();

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<BossController>() != null) return;

        // колонны буром босса не разрушаются
        if (other.GetComponentInParent<DestructibleColumn>() != null) return;

        var target = other.GetComponentInParent<IDamageable>();
        if (target == null) return;

        if (alreadyHit.Add(target))
            target.TakeImpactDamage(hitDamage);
    }
}