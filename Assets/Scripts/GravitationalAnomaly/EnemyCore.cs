using UnityEngine;

public class EnemyCore : MonoBehaviour
{
    [Header("Ataque")]
    public float damage = 2f;
    public float attackCooldown = 1f;
    private float lastAttackTime;

    [Header("Patrullaje")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 2f;
    private int currentPointIndex = 0;

    [Header("Terreno")]
    public Terrain terrain;          
    public float hoverHeight = 2f;   
    public float bobHeight = 0.5f;   
    public float bobSpeed = 1f;

    private void Update()
    {
        Patrol();
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPointIndex];

        
        Vector3 direction = (targetPoint.position - transform.position).normalized;
        transform.position += new Vector3(direction.x, 0f, direction.z) * patrolSpeed * Time.deltaTime;

        
        float groundHeight = terrain.SampleHeight(transform.position);
        float verticalOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight; 
        float targetHeight = groundHeight + hoverHeight + verticalOffset;

        transform.position = new Vector3(transform.position.x, targetHeight, transform.position.z);

        
        if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                             new Vector3(targetPoint.position.x, 0, targetPoint.position.z)) < 1f)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            RoverHealth playerHealth = collision.gameObject.GetComponent<RoverHealth>();
            if (playerHealth != null && Time.time > lastAttackTime + attackCooldown)
            {
                playerHealth.TakeDamage(damage);
                lastAttackTime = Time.time;
            }
        }
    }
}
