using UnityEngine;

public class RobotAnimationEvents : MonoBehaviour
{
    [SerializeField] private RoverInventory roverInventory;

    private RobotController controller;

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

        controller.ClearLockedMineral(); // цикл завершён — сбрасываем
    }

    public void DealDamage()
    {
        Debug.Log("Attack hit frame");
    }
}