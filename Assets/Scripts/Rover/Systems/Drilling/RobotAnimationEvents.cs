using System.Collections.Generic;
using UnityEngine;

public class RobotAnimationEvents : MonoBehaviour
{
    [SerializeField] private RoverInventory roverInventory;
    [SerializeField] private GameMissions gameMissions;

    [Header("Attack (атака на F)")]
    [SerializeField] private Transform attackPoint;      // точка перед щипцами
    [SerializeField] private float attackRadius = 3f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private LayerMask attackMask;       // слой Boss

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

        Collider[] hits = Physics.OverlapSphere(p.position, attackRadius, attackMask,
                                                QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            var target = h.GetComponentInParent<IDamageable>();
            if (target == null || ReferenceEquals(target, GetComponent<RoverHealth>())) continue;

            if (hitThisSwing.Add(target))
                target.TakeDamage(attackDamage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Transform p = attackPoint != null ? attackPoint : transform;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(p.position, attackRadius);
    }
}