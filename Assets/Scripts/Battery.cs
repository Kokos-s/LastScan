using UnityEngine;

public class Battery : MonoBehaviour
{
    [SerializeField] private float bobHeight = 0.4f;
    [SerializeField] private float bobSpeed = 3f;
    private Vector3 startPosition;

    [SerializeField] private float energyAmount = 30f;
    [SerializeField] private bool respawns = true;
    [SerializeField] private float respawnTime = 30f;

    private bool isActive = true;
    private float respawnTimer = 0f;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        float verticalOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = startPosition + new Vector3(0f, verticalOffset, 0f);

        if (!isActive && respawns)
        {
            respawnTimer -= Time.deltaTime;

            if (respawnTimer <= 0f)
            {
                Reactivate();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isActive && other.CompareTag("Player"))
        {
            RoverEnergy roverEnergy = other.GetComponent<RoverEnergy>();
            roverEnergy.AddEnergy(energyAmount);
            Collect();
        }
    }

    void Collect()
    {
        isActive = false;
        respawnTimer = respawnTime;
        gameObject.GetComponent<Renderer>().enabled = false;
    }

    void Reactivate()
    {
        isActive = true;
        gameObject.GetComponent<Renderer>().enabled = true;
    }
}
