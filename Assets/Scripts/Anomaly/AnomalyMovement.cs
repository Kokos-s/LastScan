using UnityEngine;

public class AnomalyMovement : MonoBehaviour
{
    private enum AnomalyState
    {
        Wandering,
        Alerted
    }

    [SerializeField] private AnomalyState currentState = AnomalyState.Wandering;

    [SerializeField] private Terrain terrain;
    private float mapMinX;
    private float mapMaxX;
    private float mapMinZ;
    private float mapMaxZ;
    private Vector3 wanderTarget;

    [SerializeField] private float wanderSpeed = 10f;
    [SerializeField] private float wanderRadius = 60f;
    [SerializeField] private float turnSpeed = 0.1f;

    [SerializeField] private float hoverHeight = 6f;
    [SerializeField] private float bobHeight = 2f;
    [SerializeField] private float bobSpeed = 1f;

    void Start()
    {
        Vector3 terrainSize = terrain.terrainData.size;
        Vector3 terrainPosition = terrain.transform.position;
        mapMinX = terrainPosition.x;
        mapMaxX = terrainPosition.x + terrainSize.x;
        mapMinZ = terrainPosition.z;
        mapMaxZ = terrainPosition.z + terrainSize.z;

        wanderTarget = GetRandomWanderPoint();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 directionToTarget = (wanderTarget - transform.position).normalized;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, directionToTarget, turnSpeed * Time.deltaTime, 0f);
        float verticalOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;

        transform.rotation = Quaternion.LookRotation(newDirection);
        transform.position += transform.forward * wanderSpeed * Time.deltaTime;
        float groundHeight = terrain.SampleHeight(transform.position);
        float targetHeight = groundHeight + hoverHeight + verticalOffset;
        transform.position = new Vector3(transform.position.x, targetHeight, transform.position.z);

        if (Vector3.Distance(transform.position, wanderTarget) < 1f)
        {
            wanderTarget = GetRandomWanderPoint();
        }
    }

    Vector3 GetRandomWanderPoint()
    {
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        float pointX = transform.position.x + randomCircle.x;
        float pointZ = transform.position.z + randomCircle.y;

        pointX = Mathf.Clamp(pointX, mapMinX, mapMaxX);
        pointZ = Mathf.Clamp(pointZ, mapMinZ, mapMaxZ);

        return new Vector3(pointX, transform.position.y, pointZ);
    }
}