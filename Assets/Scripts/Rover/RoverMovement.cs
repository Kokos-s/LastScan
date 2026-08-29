using UnityEngine;

public class RoverMovement : MonoBehaviour
{
    [SerializeField] private WheelCollider wheelFrontLeft;
    [SerializeField] private WheelCollider wheelMiddleLeft;
    [SerializeField] private WheelCollider wheelRearLeft;
    [SerializeField] private WheelCollider wheelFrontRight;
    [SerializeField] private WheelCollider wheelMiddleRight;
    [SerializeField] private WheelCollider wheelRearRight;

    private RoverControls controls;
    private Rigidbody rb;

    [SerializeField] private float motorForce = 2000f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float steerSmoothSpeed = 0.3f;
    private float currentSteerAngle = 0f;
    [SerializeField] private float maxSteerAngle = 25f;


    [SerializeField] private Transform meshFrontLeft;
    [SerializeField] private Transform meshMiddleLeft;
    [SerializeField] private Transform meshRearLeft;
    [SerializeField] private Transform meshFrontRight;
    [SerializeField] private Transform meshMiddleRight;
    [SerializeField] private Transform meshRearRight;

    private Vector3 meshRotationOffset = new Vector3(90f, 90f, 0f);

    private void Awake()
    {
        controls = new RoverControls();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        controls.Rover.Enable();
    }

    private void OnDisable()
    {
        controls.Rover.Disable();
    }

    void FixedUpdate()
    {
        Vector2 moveInput = controls.Rover.Move.ReadValue<Vector2>();
        float currentSpeed = rb.linearVelocity.magnitude;
        //Debug.Log(currentSpeed);
        float motorPowerReductionFactor = Mathf.Max(0f, 1f - (currentSpeed / maxSpeed));
        float motorTorque = moveInput.y * motorForce * motorPowerReductionFactor;

        ApplyMotorTorque(wheelFrontLeft, motorTorque);
        ApplyMotorTorque(wheelMiddleLeft, motorTorque);
        ApplyMotorTorque(wheelRearLeft, motorTorque);
        ApplyMotorTorque(wheelFrontRight, motorTorque);
        ApplyMotorTorque(wheelMiddleRight, motorTorque);
        ApplyMotorTorque(wheelRearRight, motorTorque);

        float targetSteerAngle = moveInput.x * maxSteerAngle;
        currentSteerAngle = Mathf.MoveTowards(currentSteerAngle, targetSteerAngle, steerSmoothSpeed);
        ApplySteerAngle(wheelFrontLeft, currentSteerAngle);
        ApplySteerAngle(wheelFrontRight, currentSteerAngle);
        //Debug.Log("Target: " + targetSteerAngle + " Current: " + currentSteerAngle);
    }

    void Update()
    {
        UpdateWheelVisual(wheelFrontLeft, meshFrontLeft);
        UpdateWheelVisual(wheelMiddleLeft, meshMiddleLeft);
        UpdateWheelVisual(wheelRearLeft, meshRearLeft);
        UpdateWheelVisual(wheelFrontRight, meshFrontRight);
        UpdateWheelVisual(wheelMiddleRight, meshMiddleRight);
        UpdateWheelVisual(wheelRearRight, meshRearRight);
    }

    void UpdateWheelVisual(WheelCollider collider, Transform mesh)
    {
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        mesh.position = position;
        mesh.rotation = rotation * Quaternion.Euler(meshRotationOffset);
    }

        void ApplyMotorTorque(WheelCollider wheel, float torque)
    {
        if (wheel.isGrounded)
        {
            wheel.motorTorque = torque;
        }
        else
        {
            wheel.motorTorque = 0f;
        }
    }
        void ApplySteerAngle(WheelCollider wheel, float angle)
    {
        if (wheel.isGrounded)
        {
            wheel.steerAngle = angle;
        }
    }
}
   