using UnityEngine;

public class TornadoRange : MonoBehaviour
{
    [SerializeField] private float pullRadius = 30f;
    [SerializeField] private float pullForce = 10f;

    [SerializeField] private float dangerRadius = 10f;
    [SerializeField] private float knockbackForceMin = 50f;
    [SerializeField] private float knockbackForceMax = 75f;
    [SerializeField] private float damage = 3f;
    [SerializeField] private float damageCooldown = 2f;

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
                movement.SetInDanger(dirFromTornado, scaledForce);

            if (health != null && Time.time >= lastDamageTime + damageCooldown)
            {
                health.TakeDamage(damage);
                lastDamageTime = Time.time;
            }
        }
        else if (distance <= pullRadius)
        {
            float pullProximity = 1f - Mathf.Clamp01((distance - dangerRadius) / (pullRadius - dangerRadius));
            float scaledPull = Mathf.Lerp(pullForce, pullForce * 4f, pullProximity);
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