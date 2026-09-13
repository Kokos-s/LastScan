using UnityEngine;

public class DrillEnergyUsage : MonoBehaviour
{
    private RoverControls controls;

    [SerializeField] private RoverEnergy roverEnergy;
    [SerializeField] private float energyDrainRate = 2f;
    private bool canDrill = false;
    public bool CanDrill
    {
        get { return canDrill; }
    }

    void Awake()
    {
        controls = new RoverControls();
    }

    void OnEnable()
    {
        controls.Rover.Enable();
    }

    void OnDisable()
    {
        controls.Rover.Disable();
    }

    void Update()
    {
        if (controls.Rover.Drill.IsPressed())
        {
            float energyToUse = energyDrainRate * Time.deltaTime;
            canDrill = roverEnergy.TryConsume(energyToUse);
        }
        else
        {
            canDrill = false;
        }
    }
}