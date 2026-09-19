using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DrillZone : MonoBehaviour
{
    public MineralPickup mineral;
    public DrillTipDisplay drillTipDisplay;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("RoverCabin")) return;

        var controller = other.GetComponentInParent<RobotController>();
        if (controller != null)
            controller.EnterDrillZone(this);

        if (drillTipDisplay != null)
            drillTipDisplay.ShowTooltip();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("RoverCabin")) return;

        var controller = other.GetComponentInParent<RobotController>();
        if (controller != null)
            controller.ExitDrillZone(this);
    }
}