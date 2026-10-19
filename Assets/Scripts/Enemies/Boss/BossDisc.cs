using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BossDisc : MonoBehaviour
{
    public enum DiscState { Stored, Held, Flying, Lying }

    [SerializeField] private float damage = 1.5f;
    [SerializeField] private float speed = 18f;
    [SerializeField] private int maxBounces = 6;
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private float spinSpeed = 1200f;
    [SerializeField] private float spinDirection = 1f;
    [SerializeField] private Vector3 spinAxis = Vector3.right;
    [SerializeField] private Transform visual;
    [SerializeField] private Vector3 heldEulerAngles = new Vector3(0f, 90f, 0f);

    private Rigidbody rb;
    private Collider col;
    private Transform homeParent;
    private Transform worldRoot;   // корень мира пещеры (CaveWorld), null если его нет
    private Vector3 homeLocalPos;
    private Quaternion homeLocalRot;
    private int bounces;
    private float dieTime;
    private float nextHitTime;
    private Vector3 lastDir = Vector3.forward;

    public DiscState State { get; private set; } = DiscState.Stored;
    public bool IsStored => State == DiscState.Stored;
    public bool IsLying => State == DiscState.Lying;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        homeParent = transform.parent;
        homeLocalPos = transform.localPosition;
        homeLocalRot = transform.localRotation;

        // Диск вылетает не в корень сцены, а в корень мира пещеры,
        // чтобы выключение CaveWorld останавливало и его
        Transform r = transform.root;
        worldRoot = (r != transform && r.GetComponent<BossController>() == null) ? r : null;

        SetInert();
        State = DiscState.Stored;
    }

    private void Start()
    {
        if (State == DiscState.Stored) SetInert();
    }

    private void SetInert()
    {
        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;
        rb.detectCollisions = false;
        col.enabled = false;
    }

    public void Hold()
    {
        SetInert();
        State = DiscState.Held;
    }

    public void ReturnHome()
    {
        SetInert();
        transform.SetParent(homeParent, false);
        transform.localPosition = homeLocalPos;
        transform.localRotation = homeLocalRot;
        State = DiscState.Stored;
    }

    public void Launch(Vector3 dir)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) dir = Vector3.forward;
        lastDir = dir.normalized;

        transform.SetParent(worldRoot);

        // высота полёта = центр коллайдера ровера
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            var pc = playerObj.GetComponent<Collider>();
            if (pc != null)
            {
                Vector3 p = transform.position;
                p.y = pc.bounds.center.y;
                transform.position = p;
            }
        }

        col.enabled = true;
        rb.detectCollisions = true;
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        rb.linearVelocity = lastDir * speed;

        bounces = 0;
        dieTime = Time.time + lifetime;
        State = DiscState.Flying;
    }

    public void Lie()
    {
        transform.SetParent(worldRoot);
        SetInert();

        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit,
                            100f, LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore))
            transform.position = hit.point + Vector3.up * 0.1f;

        State = DiscState.Lying;
    }

    private void Update()
    {
        if (visual != null && State == DiscState.Flying)
            visual.Rotate(spinAxis.normalized, spinSpeed * spinDirection * Time.deltaTime, Space.Self);

        if (State == DiscState.Flying && Time.time >= dieTime)
            Lie();
    }

    private void LateUpdate()
    {
        if (State == DiscState.Stored && transform.parent == homeParent)
        {
            transform.localPosition = homeLocalPos;
            transform.localRotation = homeLocalRot;
        }
        else if (State == DiscState.Held && transform.parent != null && transform.parent != homeParent)
        {
            transform.localRotation = Quaternion.Euler(heldEulerAngles);
        }
    }

    private void FixedUpdate()
    {
        if (State != DiscState.Flying) return;

        Vector3 v = rb.linearVelocity;
        v.y = 0f;
        if (v.sqrMagnitude > 0.01f) lastDir = v.normalized;
        rb.linearVelocity = lastDir * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (State != DiscState.Flying) return;
        if (collision.collider.GetComponentInParent<BossController>() != null) return;

        var target = collision.collider.GetComponentInParent<IDamageable>();
        if (target != null)
        {
            if (Time.time >= nextHitTime)
            {
                target.TakeDamage(damage);
                nextHitTime = Time.time + 0.5f;
            }
            return;
        }

        bounces++;
        if (bounces >= maxBounces) Lie();
    }
}