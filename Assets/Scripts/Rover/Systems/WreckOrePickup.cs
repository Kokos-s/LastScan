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
        currentWreck = null;
        pickupTarget = null;
        pickupTimer = 0f;
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
            pickupTimer = 0f;
            pickupTarget = null;
            return;
        }

        if (pickupTarget != currentWreck)
        {
            pickupTarget = currentWreck;
            pickupTimer = 0f;
        }

        pickupTimer += Time.deltaTime;

        if (pickupTimer >= pickupDuration)
        {
            int amount = currentWreck.TakeOre();
            roverInventory.AddOre(amount);
            lastCollectedAmount = amount;
            collectedMessageTimer = 2f;
            currentWreck = null;
            pickupTarget = null;
            pickupTimer = 0f;
        }
    }
}