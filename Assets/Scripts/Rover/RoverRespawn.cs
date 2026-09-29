using UnityEngine;

public class RoverRespawn : MonoBehaviour
{
    [SerializeField] private RoverHealth roverHealth;
    [SerializeField] private RoverInventory roverInventory;
    [SerializeField] private RoverMovement roverMovement;
    [SerializeField] private RoverEnergy roverEnergy;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float respawnDelay = 5f;
    [SerializeField] private GameObject wreckPrefab;

    private Rigidbody roverBody;
    private float respawnTimer;

    public bool IsRespawning
    {
        get { return roverHealth.IsDead; }
    }

    public float RespawnTimeLeft
    {
        get { return Mathf.Max(0f, respawnDelay - respawnTimer); }
    }

    private void Awake()
    {
        roverBody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!roverHealth.IsDead)
        {
            respawnTimer = 0f;
            return;
        }

        respawnTimer += Time.deltaTime;

        if (respawnTimer >= respawnDelay)
            Respawn();
    }

    private void Respawn()
    {
        int lostOre = roverInventory.CollectedOre;
        Vector3 wreckPosition = roverBody.position;
        Quaternion wreckRotation = roverBody.rotation;
        Vector3 wreckVelocity = roverBody.linearVelocity;
        Vector3 wreckAngularVelocity = roverBody.angularVelocity;
        Vector3 roverCenter = roverBody.worldCenterOfMass;
        roverBody.linearVelocity = Vector3.zero;
        roverBody.angularVelocity = Vector3.zero;
        roverMovement.ResetAfterRespawn();

        roverBody.position = spawnPoint.position;
        roverBody.rotation = spawnPoint.rotation;

        respawnTimer = 0f;
        roverInventory.RemoveOre(roverInventory.CollectedOre);
        roverEnergy.AddEnergy(roverEnergy.MaxEnergy);

        GameObject wreck = Instantiate(wreckPrefab, wreckPosition, wreckRotation);
        WreckOre wreckOre = wreck.GetComponentInChildren<WreckOre>();
        wreckOre.SetOre(lostOre);
        Rigidbody[] pieces = wreck.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody piece in pieces)
        {
            Vector3 offset = piece.worldCenterOfMass - roverCenter;
            piece.linearVelocity = wreckVelocity + Vector3.Cross(wreckAngularVelocity, offset);
            piece.angularVelocity = wreckAngularVelocity;
        }

        roverHealth.Revive();
    }
}