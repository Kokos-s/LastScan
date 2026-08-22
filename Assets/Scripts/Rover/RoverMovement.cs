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

    [SerializeField] private float motorForce = 1500f;
    [SerializeField] private float maxSteerAngle = 30f;

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
        float motorTorque = moveInput.y * motorForce;
        wheelFrontLeft.motorTorque = motorTorque;
        wheelFrontRight.motorTorque = motorTorque;
        wheelMiddleLeft.motorTorque = motorTorque;
        wheelMiddleRight.motorTorque = motorTorque;
        wheelRearLeft.motorTorque = motorTorque;
        wheelRearRight.motorTorque = motorTorque;

        float steerAngle = moveInput.x * maxSteerAngle;
        wheelFrontLeft.steerAngle = steerAngle;
        wheelFrontRight.steerAngle = steerAngle;
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
}
   