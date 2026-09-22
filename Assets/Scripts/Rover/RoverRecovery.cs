using UnityEngine;
using UnityEngine.InputSystem;

public class RoverRecovery : MonoBehaviour
{
    [SerializeField] private Vector3 recoveryCenter = new Vector3(-0.2f, 0f, 0.3f);
    [SerializeField] private Vector3 recoverySize = new Vector3(3.2f, 1.5f, 3f);

    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float searchDistance = 10f;
    [SerializeField] private float rayHeight = 30f;
    [SerializeField] private float maxGroundAngle = 20f;
    [SerializeField] private float groundClearance = 4f;

    [SerializeField] private RoverHealth roverHealth;
    [SerializeField] private float holdDuration = 3f;
    [SerializeField] private float damageBlockDuration = 1f;

    private Rigidbody roverBody;

    private float holdTimer;
    private float damageBlockTimer;
    private float previousHealth;
    private bool waitingForRelease;
    private RoverControls controls;

    private bool hasFoundPoint;
    private Vector3 recoveryBoxCenter;
    private Quaternion recoveryRotation;

    public float RecoveryProgress
    {
        get { return Mathf.Clamp01(holdTimer / holdDuration); }
    }

    public bool IsDamageBlocked
    {
        get { return damageBlockTimer > 0f; }
    }

    public bool IsDead
    {
        get { return roverHealth.IsDead; }
    }

    private void OnEnable()
    {
        controls.Rover.Enable();
    }

    private void OnDisable()
    {
        controls.Rover.Disable();
        holdTimer = 0f;
        waitingForRelease = false;
    }

    private void OnDestroy()
    {
        controls.Dispose();
    }

    private void Awake()
    {
        roverBody = GetComponent<Rigidbody>();
        controls = new RoverControls();
    }

    private void Start()
    {
        previousHealth = roverHealth.currentHealth;
    }

    private void LateUpdate()
    {
        bool isHolding = controls.Rover.Recover.IsPressed();

        if (damageBlockTimer > 0f)
            damageBlockTimer -= Time.deltaTime;

        if (roverHealth.currentHealth < previousHealth)
        {
            holdTimer = 0f;
            damageBlockTimer = damageBlockDuration;
        }

        previousHealth = roverHealth.currentHealth;

        if (!isHolding)
            waitingForRelease = false;

        if (roverHealth.IsDead || damageBlockTimer > 0f)
        {
            holdTimer = 0f;
            return;
        }

        if (!isHolding)
        {
            holdTimer = 0f;
            return;
        }

        if (waitingForRelease)
            return;

        holdTimer += Time.deltaTime;

        if (holdTimer >= holdDuration)
        {
            Recover();
            holdTimer = 0f;
            waitingForRelease = true;
        }
    }

    private void Recover()
    {
        FindRecoveryPlace();

        if (!hasFoundPoint)
            return;

        Vector3 centerOffset = Vector3.Scale(recoveryCenter, transform.lossyScale);

        roverBody.position = recoveryBoxCenter - recoveryRotation * centerOffset;

        roverBody.rotation = recoveryRotation;
        roverBody.linearVelocity = Vector3.zero;
        roverBody.angularVelocity = Vector3.zero;
    }

    private void FindRecoveryPlace()
    {
        hasFoundPoint = false;

        for (int ring = 1; ring <= 1000; ring++)
        {
            float distance = searchDistance * ring;

            for (int i = 0; i < 72; i++)
            {
                float angle = i * 5f * Mathf.Deg2Rad;

                Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

                Vector3 rayStart = transform.position + direction * distance + Vector3.up * rayHeight;

                bool foundGround = Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayHeight * 2f, groundMask, QueryTriggerInteraction.Ignore);

                if (!foundGround)
                    continue;

                float groundAngle = Vector3.Angle(hit.normal, Vector3.up);

                if (groundAngle > maxGroundAngle)
                    continue;

                if (!IsSpaceFree(hit.point))
                    continue;

                hasFoundPoint = true;
                return;
            }
        }
    }

    private Vector3 GetWorldSize()
    {
        Vector3 scale = transform.lossyScale;

        scale = new Vector3(  Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)
        );

        return Vector3.Scale(recoverySize, scale);
    }

    private bool IsSpaceFree(Vector3 groundPoint)
    {
        recoveryRotation = Quaternion.Euler( 0f, transform.eulerAngles.y, 0f);

        Vector3 worldSize = GetWorldSize();

        recoveryBoxCenter = groundPoint + Vector3.up * (worldSize.y / 2f + groundClearance);

        Collider[] obstacles = Physics.OverlapBox( recoveryBoxCenter, worldSize / 2f, recoveryRotation, Physics.AllLayers, QueryTriggerInteraction.Ignore);

        foreach (Collider obstacle in obstacles)
        {
            if (obstacle.transform.IsChildOf(transform))
                continue;

            return false;
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Matrix4x4 previousMatrix = Gizmos.matrix;

        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(recoveryCenter, recoverySize);

        Gizmos.matrix = previousMatrix;
    }
}