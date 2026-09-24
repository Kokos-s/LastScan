using UnityEngine;

public class BaseUnloading : MonoBehaviour
{
    [SerializeField] private float unloadInterval = 0.5f;
    private float unloadTimer;

    [SerializeField] private RoverHealth roverHealth;
    [SerializeField] private float repairPerOre = 2f;

    [SerializeField] private GameMissions missions;
    private RoverInventory inventory;
    private bool roverInZone;
    private RoverControls controls;

    public bool RoverInZone
    {
        get { return roverInZone; }
    }

    public bool HasOre
    {
        get { return inventory != null && inventory.CollectedOre > 0; }
    }

    public bool IsUnloading
    {
        get
        {
            return roverInZone && HasOre
                && controls.Rover.Unload.IsPressed();
        }
    }


    private void Awake()
    {
        controls = new RoverControls();
    }

    private void OnEnable()
    {
        controls.Rover.Unload.Enable();
    }

    private void OnDisable()
    {
        controls.Rover.Unload.Disable();
    }

    private void FixedUpdate()
    {
        roverInZone = false;
    }

    private void LateUpdate()
    {
        if (!roverInZone || !controls.Rover.Unload.IsPressed() || inventory.CollectedOre <= 0)
        {
            unloadTimer = unloadInterval;
            return;
        }
        
        unloadTimer -= Time.deltaTime;

        if (unloadTimer <= 0)
        {
            unloadTimer = unloadInterval;
            int portionSize = 1;
            int amount = Mathf.Min(portionSize, inventory.CollectedOre);
            inventory.RemoveOre(amount);
            missions.AddDeliveryOre(amount);
            roverHealth.Repair(amount * repairPerOre);
        }
    }

    public void OnTriggerStay(Collider other)
    {
        RoverInventory foundInventory = other.GetComponentInParent<RoverInventory>();

        if (foundInventory != null)
        {
            inventory = foundInventory;
            roverInZone = true;
        }
    }
}
