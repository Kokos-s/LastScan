using UnityEngine;

public class AnomalyDamage : MonoBehaviour
{
    [SerializeField] private float damage = 1f;
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private AnomalyGlow anomalyGlow;
    [SerializeField] private AudioSource audioSource;

    private float nextDamageTime;

    private void OnTriggerStay(Collider other)
    {
        if (Time.time < nextDamageTime)
            return;

        RoverHealth roverHealth = other.GetComponentInParent<RoverHealth>();

        if (roverHealth == null || roverHealth.IsDead)
            return;

        nextDamageTime = Time.time + damageInterval;
        roverHealth.TakeDamage(damage);
        if (anomalyGlow != null)
            anomalyGlow.Flash();
        if (audioSource != null)
        {
            audioSource.time = 0.2f;
            audioSource.Play();
        }    
            
    }
}
