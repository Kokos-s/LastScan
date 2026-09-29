using UnityEngine;

public class RoverRespawn : MonoBehaviour
{
    [SerializeField] private RoverHealth roverHealth;
    [SerializeField] private RoverInventory roverInventory;
    [SerializeField] private RoverMovement roverMovement;
    [SerializeField] private RoverEnergy roverEnergy;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float respawnDelay = 5f;

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
        roverBody.linearVelocity = Vector3.zero;
        roverBody.angularVelocity = Vector3.zero;
        roverMovement.ResetAfterRespawn();

        roverBody.position = spawnPoint.position;
        roverBody.rotation = spawnPoint.rotation;

        respawnTimer = 0f;
        roverInventory.RemoveOre(roverInventory.CollectedOre);
        roverEnergy.AddEnergy(roverEnergy.MaxEnergy);
        roverHealth.Revive();
    }
}