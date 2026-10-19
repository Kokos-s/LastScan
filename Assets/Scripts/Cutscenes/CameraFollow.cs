using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 40f;
    [SerializeField] private float minDistance = 15f;
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float horizontalSpeed = 15f;
    [SerializeField] private float verticalSpeed = 15f;
    [SerializeField] private float minPitch = 10f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private float pitch = 30f;
    [SerializeField] private float inputIgnoreTime = 0.3f;
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField] private float collisionRadius = 1f;
    [SerializeField] private float collisionPadding = 0.2f;
    [SerializeField] private float returnSpeed = 20f;

    private float currentDistance;
    private float yaw = 0f;
    private float ignoreTimer = 0f;
    private RoverControls controls;

    private void Awake()
    {
        controls = new RoverControls();
        currentDistance = distance;
    }

    private void OnEnable()
    {
        controls.Camera.Enable();
    }

    private void OnDisable()
    {
        controls.Camera.Disable();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        ignoreTimer = inputIgnoreTime;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        if (ignoreTimer > 0f)
            ignoreTimer -= Time.deltaTime;
        else
        {
            Vector2 lookInput = controls.Camera.Look.ReadValue<Vector2>();
            float zoomInput = controls.Camera.Zoom.ReadValue<float>();
            yaw += lookInput.x * horizontalSpeed * Time.deltaTime;
            pitch -= lookInput.y * verticalSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            distance -= zoomInput * zoomSpeed * Time.deltaTime;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 direction = rotation * Vector3.back;

        float allowedDistance = distance;

        RaycastHit hit;

        if (Physics.SphereCast(target.position, collisionRadius, direction, out hit, distance, obstacleLayers, QueryTriggerInteraction.Ignore))
            allowedDistance = Mathf.Max(0f, hit.distance - collisionPadding);

        if (currentDistance > allowedDistance)
            currentDistance = allowedDistance;
        else
            currentDistance = Mathf.MoveTowards(currentDistance, allowedDistance, returnSpeed * Time.deltaTime);

        transform.position = target.position + direction * currentDistance;
        transform.rotation = rotation;
    }
}



/*using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 40f;
    [SerializeField] private float minDistance = 15f;
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float horizontalSpeed = 15f;
    [SerializeField] private float verticalSpeed = 15f;
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
}*/