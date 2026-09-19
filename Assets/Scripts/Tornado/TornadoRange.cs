using UnityEngine;

public class TornadoRange : MonoBehaviour
{
   
    [Header("Zona de atracción (radio exterior)")]
    [SerializeField] private float pullRadius = 35f;
    [SerializeField] private float pullForce = 4f;

    [Header("Zona de peligro (radio interior)")]
    [SerializeField] private float dangerRadius = 14f;
    [SerializeField] private float knockbackForceMin = 30f;
    [SerializeField] private float knockbackForceMax = 60f;
    [SerializeField] private float damage = 3f;
    [SerializeField] private float damageCooldown = 1f;

    private float lastDamageTime = -999f;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        RoverMovement movement = rb.GetComponent<RoverMovement>();
        RoverHealth health = rb.GetComponent<RoverHealth>();

        Vector3 diff = rb.position - transform.position;
        diff.y = 0f;
        float distance = diff.magnitude;
        Vector3 dirFromTornado = diff.normalized;

        if (distance <= dangerRadius)
        {
            float proximity = 1f - Mathf.Clamp01(distance / dangerRadius);
            float scaledForce = Mathf.Lerp(knockbackForceMin, knockbackForceMax, proximity);

            if (movement != null)
                movement.SetInDanger(dirFromTornado, scaledForce); // mantiene el knockback activo mientras esté acá

            if (health != null && Time.time >= lastDamageTime + damageCooldown)
            {
                health.TakeDamage(damage);
                lastDamageTime = Time.time;
            }
        }
        else if (distance <= pullRadius)
{
    float pullProximity = 1f - Mathf.Clamp01((distance - dangerRadius) / (pullRadius - dangerRadius));
    float scaledPull = Mathf.Lerp(pullForce, pullForce * 4f, pullProximity); // crece hasta 4x más cerca del borde de peligro
    rb.AddForce(-dirFromTornado * scaledPull, ForceMode.Acceleration);
}
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        RoverMovement movement = other.attachedRigidbody?.GetComponent<RoverMovement>();
        if (movement != null)
            movement.ExitDanger(); 
    }
}