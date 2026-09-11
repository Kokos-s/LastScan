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
    [SerializeField] private RoverEnergy energy;

    [SerializeField] private float motorForce = 5000f;
    [SerializeField] private float maxSpeed = 17f;
    [SerializeField] private float brakeForce = 3000f;
    [SerializeField] private float directionChangeSpeed = 0.2f;

    [SerializeField] private float steerSmoothSpeed = 1.5f;
    private float currentSteerAngle = 0f;
    [SerializeField] private float maxSteerAngle = 21f;

    [SerializeField] private Transform meshFrontLeft;
    [SerializeField] private Transform meshMiddleLeft;
    [SerializeField] private Transform meshRearLeft;
    [SerializeField] private Transform meshFrontRight;
    [SerializeField] private Transform meshMiddleRight;
    [SerializeField] private Transform meshRearRight;

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

        float throttle = 0f;

        if (Mathf.Abs(moveInput.y) > 0.1f)
            throttle = moveInput.y;

        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        float currentSpeed = rb.linearVelocity.magnitude;
        bool isBraking = false;

        if ((forwardSpeed > directionChangeSpeed && throttle < 0f)  ||  (forwardSpeed < -directionChangeSpeed && throttle > 0f))
            isBraking = true;
        float motorTorque = 0f;
        float brakeTorque = 0f;

        if (isBraking)
            brakeTorque = Mathf.Abs(throttle) * brakeForce;
        else if (throttle != 0f && energy.CurrentEnergy > 0f)
        {
            float motorPowerReductionFactor = 1f - Mathf.InverseLerp(maxSpeed * 0.6f, maxSpeed, currentSpeed);
            motorTorque = throttle * motorForce * motorPowerReductionFactor;
        }

        ApplyWheelForces(wheelFrontLeft, motorTorque, brakeTorque);
        ApplyWheelForces(wheelMiddleLeft, motorTorque, brakeTorque);
        ApplyWheelForces(wheelRearLeft, motorTorque, brakeTorque);
        ApplyWheelForces(wheelFrontRight, motorTorque, brakeTorque);
        ApplyWheelForces(wheelMiddleRight, motorTorque, brakeTorque);
        ApplyWheelForces(wheelRearRight, motorTorque, brakeTorque);

        float targetSteerAngle = moveInput.x * maxSteerAngle;

        currentSteerAngle = Mathf.MoveTowards(currentSteerAngle, targetSteerAngle, steerSmoothSpeed);

        ApplySteerAngle(wheelFrontLeft, currentSteerAngle);
        ApplySteerAngle(wheelFrontRight, currentSteerAngle);

        if (Mathf.Abs(moveInput.y) > 0.1f)
            energy.ReportPlayerMovement();
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
        mesh.rotation = rotation;
    }

    void ApplyWheelForces(WheelCollider wheel, float motorTorque, float brakeTorque)
    {
        if (wheel.isGrounded)
            wheel.motorTorque = motorTorque;
        else
            wheel.motorTorque = 0f;

        wheel.brakeTorque = brakeTorque;
    }

    void ApplySteerAngle(WheelCollider wheel, float angle)
    {
        wheel.steerAngle = angle;
    }
}
