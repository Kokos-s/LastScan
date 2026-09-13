using UnityEngine;

public class DrillRotation : MonoBehaviour
{
    private RoverControls controls;
    [SerializeField] private float accelerationTime = 1f;
    [SerializeField] private float decelerationTime = 1.5f;

    [SerializeField] private Transform row1;
    [SerializeField] private Transform row2;
    [SerializeField] private Transform row3;
    [SerializeField] private Transform row4;

    private float currentRotationSpeed = 0f;
    [SerializeField] private float maxRotationSpeed = 600f;
    [SerializeField] private DrillEnergyUsage drillEnergyUsage;

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

        if (controls.Rover.Drill.IsPressed() && drillEnergyUsage.CanDrill)
        {
            currentRotationSpeed += maxRotationSpeed / accelerationTime * Time.deltaTime;
            if (currentRotationSpeed > maxRotationSpeed)
                currentRotationSpeed = maxRotationSpeed;
        }
        else
        {
            currentRotationSpeed -= maxRotationSpeed / decelerationTime * Time.deltaTime;
            if (currentRotationSpeed < 0f)
                currentRotationSpeed = 0f;
        }

        if (currentRotationSpeed > 0f)
        {
            float speedOdd = currentRotationSpeed * Time.deltaTime;
            float speedEven = -currentRotationSpeed * Time.deltaTime;

            if (row1 != null)
                row1.Rotate(Vector3.forward * speedOdd);

            if (row3 != null)
                row3.Rotate(Vector3.forward * speedOdd);

            if (row2 != null)
                row2.Rotate(Vector3.forward * speedEven);

            if (row4 != null)
                row4.Rotate(Vector3.forward * speedEven);
        }
    }
}

