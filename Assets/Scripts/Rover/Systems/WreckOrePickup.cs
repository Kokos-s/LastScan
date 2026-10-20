using UnityEngine;

[DefaultExecutionOrder(-100)]
public class WreckOrePickup : MonoBehaviour
{
    [SerializeField] private float recoveryRadius = 10f;
    [SerializeField] private RoverHealth roverHealth;
    [SerializeField] private RobotController robotController;
    [SerializeField] private WreckOre currentWreck;
    [SerializeField] private RoverInventory roverInventory;
    [SerializeField] private float pickupDuration = 2f;

    [Header("Эффект забора")]
    [SerializeField] private Material sparkMaterial;   // необязательно: свой материал для шариков
    [SerializeField] private int maxPieces = 8;
    [SerializeField] private float sparkScale = 0.5f;
    [SerializeField] private float flightTime = 0.9f;
    [SerializeField] private float arcHeight = 4f;
    [SerializeField] private float spreadRadius = 1.5f;

    private RoverControls controls;
    private float pickupTimer;
    private WreckOre pickupTarget;

    private int lastCollectedAmount;
    private float collectedMessageTimer;

    public bool CanRecover
    {
        get { return currentWreck != null; }
    }

    public bool IsPickingUp
    {
        get { return CanRecover && pickupTimer > 0f; }
    }

    public float PickupProgress
    {
        get { return Mathf.Clamp01(pickupTimer / pickupDuration); }
    }

    public bool ShowCollectedMessage
    {
        get { return collectedMessageTimer > 0f; }
    }

    public int LastCollectedAmount
    {
        get { return lastCollectedAmount; }
    }

    private void Awake()
    {
        controls = new RoverControls();
    }

    private void OnEnable()
    {
        controls.Rover.Drill.Enable();
    }

    private void OnDisable()
    {
        controls.Rover.Drill.Disable();
        ResetPickup();
        currentWreck = null;
        collectedMessageTimer = 0f;
    }

    private void Update()
    {
        collectedMessageTimer = Mathf.Max(0f, collectedMessageTimer - Time.deltaTime);

        currentWreck = null;

        if (roverHealth.IsDead || robotController.IsBusy)
            return;

        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, recoveryRadius, Physics.AllLayers, QueryTriggerInteraction.Collide);

        float nearestDistance = Mathf.Infinity;

        foreach (Collider nearbyCollider in nearbyColliders)
        {
            WreckOre wreck = nearbyCollider.GetComponentInParent<WreckOre>();

            if (wreck == null || wreck.StoredOre <= 0)
                continue;

            float distance = Vector3.Distance(transform.position, wreck.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                currentWreck = wreck;
            }
        }
    }

    private void LateUpdate()
    {
        if (!CanRecover || !controls.Rover.Drill.IsPressed())
        {
            ResetPickup();
            return;
        }

        if (pickupTarget != currentWreck)
        {
            ResetPickup();
            pickupTarget = currentWreck;
        }

        pickupTimer += Time.deltaTime;
        pickupTarget.SetCharge(PickupProgress);

        if (pickupTimer >= pickupDuration)
        {
            Vector3 from = currentWreck.transform.position + Vector3.up * 1.2f;
            int amount = currentWreck.TakeOre();
            roverInventory.AddOre(amount);
            lastCollectedAmount = amount;
            collectedMessageTimer = 2f;

            SpawnSparks(from, amount);

            currentWreck = null;
            pickupTarget = null;
            pickupTimer = 0f;
        }
    }

    private void ResetPickup()
    {
        if (pickupTarget != null)
            pickupTarget.SetCharge(0f);

        pickupTarget = null;
        pickupTimer = 0f;
    }

    private void SpawnSparks(Vector3 from, int amount)
    {
        int count = Mathf.Clamp(amount, 1, maxPieces);

        for (int i = 0; i < count; i++)
        {
            // Небольшой разброс стартовых точек по остову
            Vector3 offset = Random.insideUnitSphere * spreadRadius;
            offset.y = Mathf.Abs(offset.y);

            GameObject spark = OreFx.CreateSpark(from + offset, sparkScale, sparkMaterial);
            OreFlyer flyer = spark.AddComponent<OreFlyer>();
            flyer.Init(transform, i * 0.07f, flightTime * Random.Range(0.85f, 1.15f), arcHeight);
        }
    }
}