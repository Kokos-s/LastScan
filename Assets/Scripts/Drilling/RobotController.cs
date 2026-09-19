using UnityEngine;

public class RobotController : MonoBehaviour
{
    public Transform gripPoint;
    public Transform containerSlot;

    public DrillZone currentDrillZone;

    public void EnterDrillZone(DrillZone zone)
    {
        currentDrillZone = zone;
    }

    public void ExitDrillZone(DrillZone zone)
    {
        if (currentDrillZone == zone)
            currentDrillZone = null;
    }

    public MineralPickup GetCurrentMineral()
    {
        return currentDrillZone != null ? currentDrillZone.mineral : null;
    }
}