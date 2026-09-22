using UnityEngine;
using System.Collections;

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
    [SerializeField] private RobotController robotController;

    [SerializeField] private float motorForce = 5000f;
    [SerializeField] private float parkingBrakeForce = 5000f;
    [SerializeField] private float parkingBrakeSpeed = 0.5f;
    [SerializeField] private float brakeForce = 3000f;
    [SerializeField] private float maxSpeed = 17f;
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

    [SerializeField] private float startBoostMultiplier = 2f;
    [SerializeField] private float startBoostEndSpeed = 4f;
    

    private float stormResistance = 0f;
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
        if (robotController != null && robotController.IsBusy)
        {
            ApplyWheelForces(wheelFrontLeft, 0f, brakeForce);
            ApplyWheelForces(wheelMiddleLeft, 0f, brakeForce);
            ApplyWheelForces(wheelRearLeft, 0f, brakeForce);
            ApplyWheelForces(wheelFrontRight, 0f, brakeForce);
            ApplyWheelForces(wheelMiddleRight, 0f, brakeForce);
            ApplyWheelForces(wheelRearRight, 0f, brakeForce);
            return;
        }

        if (isInDanger)
        {
            ZeroWheelTorques();
            rb.AddForce(dangerDirection * dangerForce, ForceMode.Acceleration);
            return;
        }

        Vector2 moveInput = controls.Rover.Move.ReadValue<Vector2>();

        float throttle = 0f;

        if (Mathf.Abs(moveInput.y) > 0.1f)
            throttle = moveInput.y;

        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        float currentSpeed = rb.linearVelocity.magnitude;
        Debug.Log(currentSpeed);
        bool isBraking = false;

        if ((forwardSpeed > directionChangeSpeed && throttle < 0f) || (forwardSpeed < -directionChangeSpeed && throttle > 0f))
            isBraking = true;
        float motorTorque = 0f;
        float brakeTorque = 0f;

        if (throttle == 0f)
        {
            if (currentSpeed <= parkingBrakeSpeed)
                brakeTorque = parkingBrakeForce;
        }
        else if (isBraking)
            brakeTorque = Mathf.Abs(throttle) * brakeForce;
        else if (energy.CurrentEnergy > 0f)
        {
            float motorPowerReductionFactor = 1f - Mathf.InverseLerp(maxSpeed * 0.6f, maxSpeed, currentSpeed);
            float startBoost = Mathf.Lerp(startBoostMultiplier, 1f, Mathf.InverseLerp(0f, startBoostEndSpeed, currentSpeed));
            motorTorque = throttle * motorForce * startBoost * motorPowerReductionFactor * (1f - stormResistance);
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



    private bool isInDanger = false;
    private Vector3 dangerDirection;
    private float dangerForce;
    private WheelFrictionCurve[] originalForward;
    private WheelFrictionCurve[] originalSideways;

    public void SetInDanger(Vector3 direction, float force)
    {
        if (!isInDanger)
            ReduceFriction();

        isInDanger = true;
        dangerDirection = direction.normalized;
        dangerForce = force;
    }

    public void ExitDanger()
    {
        if (!isInDanger) return;

        isInDanger = false;
        RestoreFriction();
    }

    private void ReduceFriction()
    {
        WheelCollider[] wheels = AllWheels();
        originalForward = new WheelFrictionCurve[wheels.Length];
        originalSideways = new WheelFrictionCurve[wheels.Length];

        for (int i = 0; i < wheels.Length; i++)
        {
            originalForward[i] = wheels[i].forwardFriction;
            originalSideways[i] = wheels[i].sidewaysFriction;

            WheelFrictionCurve reducedForward = wheels[i].forwardFriction;
            reducedForward.stiffness = 0.05f;
            wheels[i].forwardFriction = reducedForward;

            WheelFrictionCurve reducedSideways = wheels[i].sidewaysFriction;
            reducedSideways.stiffness = 0.05f;
            wheels[i].sidewaysFriction = reducedSideways;
        }
    }

    private void RestoreFriction()
    {
        WheelCollider[] wheels = AllWheels();
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].forwardFriction = originalForward[i];
            wheels[i].sidewaysFriction = originalSideways[i];
        }
    }

    private WheelCollider[] AllWheels()
    {
        return new WheelCollider[] { wheelFrontLeft, wheelMiddleLeft, wheelRearLeft, wheelFrontRight, wheelMiddleRight, wheelRearRight };
    }

    private void ZeroWheelTorques()
    {
        foreach (WheelCollider wheel in AllWheels())
        {
            wheel.motorTorque = 0f;
            wheel.brakeTorque = 0f;
        }
    }

    public void SetStormResistance(float resistance)
    {
        stormResistance = Mathf.Clamp01(resistance);
    }
}