using System.Collections.Generic;
using UnityEngine;

public class RobotAnimationEvents : MonoBehaviour
{
    [SerializeField] private RoverInventory roverInventory;
    [SerializeField] private GameMissions gameMissions;

    [Header("Attack (атака на ЛКМ)")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 3f;
    [SerializeField] private float attackDamage = 4f;
    [SerializeField] private LayerMask attackMask;

    [Header("Hit Feedback (удар по боссу)")]
    [SerializeField] private float hitStopTime = 0.06f;
    [SerializeField] private float shakeAmplitude = 0.3f;
    [SerializeField] private float shakeDuration = 0.15f;

    private RobotController controller;
    private readonly HashSet<IDamageable> hitThisSwing = new HashSet<IDamageable>();

    private void Awake()
    {
        controller = GetComponent<RobotController>();
    }

    public void SpawnMineral()
    {
        var mineral = controller.GetLockedMineral();
        if (mineral != null)
            mineral.PlaySpawnEffect();
    }

    public void AttachMineralToGrip()
    {
        var mineral = controller.GetLockedMineral();
        if (mineral != null)
            mineral.AttachTo(controller.gripPoint);
    }

    public void PlaceMineralInContainer()
    {
        var mineral = controller.GetLockedMineral();
        if (mineral != null)
        {
            mineral.PlaceInContainerAndDespawn(controller.containerSlot);
            roverInventory.AddOre();
        }

        controller.ClearLockedMineral();
    }

    public void DealDamage()
    {
        Transform p = attackPoint != null ? attackPoint : transform;
        hitThisSwing.Clear();
        bool hitSomething = false;

        Collider[] hits = Physics.OverlapSphere(p.position, attackRadius, attackMask,
                                                QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            var target = h.GetComponentInParent<IDamageable>();
            if (target == null || ReferenceEquals(target, GetComponent<RoverHealth>())) continue;

            if (hitThisSwing.Add(target))
            {
                target.TakeDamage(attackDamage);
                hitSomething = true;

                var fb = h.GetComponentInParent<BossHitFeedback>();
                if (fb != null)
                {
                    Vector3 point = h.bounds.ClosestPoint(p.position);
                    Vector3 normal = (p.position - point);
                    fb.Play(point, normal);
                }
            }
        }

        if (hitSomething && HitFeedbackFX.Instance != null)
        {
            HitFeedbackFX.Instance.Shake(shakeAmplitude, shakeDuration);
            HitFeedbackFX.Instance.HitStop(hitStopTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Transform p = attackPoint != null ? attackPoint : transform;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(p.position, attackRadius);
    }
}