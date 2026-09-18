using UnityEngine;

public class CloudMovement : MonoBehaviour
{
    public GameObject surfaceObject;
    public float height = 10f;
    public float volatility = 0.5f;
    public float moveSpeed = 2f;

    private Bounds surfaceBounds;
    private float offsetX;
    private float offsetZ;

    void Start()
    {
        if (surfaceObject == null)
        {
            enabled = false;
            return;
        }

        Renderer rend = surfaceObject.GetComponent<Renderer>();
        Collider col = surfaceObject.GetComponent<Collider>();

        if (rend != null)
        {
            surfaceBounds = rend.bounds;
        }
        else if (col != null)
        {
            surfaceBounds = col.bounds;
        }
        else
        {
            enabled = false;
            return;
        }

        offsetX = Random.Range(0f, 1000f);
        offsetZ = Random.Range(0f, 1000f);
    }

    void Update()
    {
        float noiseX = Mathf.PerlinNoise(Time.time * volatility + offsetX, 0f);
        float noiseZ = Mathf.PerlinNoise(0f, Time.time * volatility + offsetZ);

        float newX = Mathf.Lerp(surfaceBounds.min.x, surfaceBounds.max.x, noiseX);
        float newZ = Mathf.Lerp(surfaceBounds.min.z, surfaceBounds.max.z, noiseZ);

        Vector3 targetPosition = new Vector3(newX, height, newZ);
        transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        if (surfaceObject != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 center = new Vector3(surfaceBounds.center.x, height, surfaceBounds.center.z);
            Vector3 size = new Vector3(surfaceBounds.size.x, 0.1f, surfaceBounds.size.z);
            Gizmos.DrawWireCube(center, size);
        }
    }
}