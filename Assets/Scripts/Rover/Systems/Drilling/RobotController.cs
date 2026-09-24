using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("Marker points")]
    public Transform gripPoint;
    public Transform containerSlot;

    [Header("State")]
    public DrillZone currentDrillZone;
    private MineralPickup lockedMineral;

    public bool IsBusy { get; private set; }

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

    public void LockMineral()
    {
        lockedMineral = currentDrillZone != null ? currentDrillZone.mineral : null;
    }

    public MineralPickup GetLockedMineral()
    {
        return lockedMineral;
    }

    public void ClearLockedMineral()
    {
        lockedMineral = null;
        IsBusy = false;
    }

    public void SetBusy(bool busy)
    {
        IsBusy = busy;
    }
}