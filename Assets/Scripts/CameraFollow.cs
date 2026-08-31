using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 15f;
    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 30f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float horizontalSpeed = 20f;
    [SerializeField] private float verticalSpeed = 20f;
    [SerializeField] private float minPitch = 10f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private float pitch = 30f;
    [SerializeField] private float inputIgnoreTime = 0.3f;

    private float yaw = 0f;
    private float ignoreTimer = 0f;
    private RoverControls controls;

    private void Awake()
    {
        controls = new RoverControls();
    }

    private void OnEnable()
    {
        controls.Camera.Enable();
    }

    private void OnDisable()
    {
        controls.Camera.Disable();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        ignoreTimer = inputIgnoreTime;
    }

    void LateUpdate()
    {
        if (ignoreTimer > 0f)
        {
            ignoreTimer -= Time.deltaTime;
            return;
        }

        Vector2 lookInput = controls.Camera.Look.ReadValue<Vector2>();
        float zoomInput = controls.Camera.Zoom.ReadValue<float>();

        yaw += lookInput.x * horizontalSpeed * Time.deltaTime;
        pitch -= lookInput.y * verticalSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        distance -= zoomInput * zoomSpeed * Time.deltaTime;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, 0f, -distance);

        transform.position = target.position + offset;
        transform.LookAt(target.position);
    }
}