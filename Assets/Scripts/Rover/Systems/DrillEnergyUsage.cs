using UnityEngine;

public class DrillEnergyUsage : MonoBehaviour
{
    private RoverControls controls;

    [SerializeField] private RoverEnergy roverEnergy;
    [SerializeField] private float energyDrainRate = 2f;
    [SerializeField] private RobotController robotController;

    private bool canDrill = false;

    public bool CanDrill
    {
        get { return canDrill; }
    }

    private void Awake()
    {
        controls = new RoverControls();
    }

    private void OnEnable()
    {
        controls.Rover.Enable();
    }

    private void OnDisable()
    {
        controls.Rover.Disable();
        canDrill = false;
    }

    private void Update()
    {
        canDrill = false;

        if (robotController.IsBusy || robotController.GetCurrentMineral() != null)
            return;

        if (controls.Rover.Drill.IsPressed())
        {
            float energyToUse = energyDrainRate * Time.deltaTime;
            canDrill = roverEnergy.TryConsume(energyToUse);
        }
    }
}