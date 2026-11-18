using UnityEngine;
using UnityEngine.Events;

public interface IDamageable
{
    void TakeDamage(float amount);

    
    void TakeImpactDamage(float amount) => TakeDamage(amount);
}

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;

    public float Current { get; private set; }
    public float Max => maxHealth;
    public float Normalized => Current / maxHealth;

    
    public bool Invulnerable { get; set; }

    public UnityEvent<float> onDamaged;
    public UnityEvent onRestored;
    public UnityEvent onDeath;

    private void Awake() => Current = maxHealth;

    public void TakeDamage(float amount)
    {
        if (Invulnerable) return;
        if (Current <= 0f) return;

        Current = Mathf.Max(0f, Current - amount);
        onDamaged?.Invoke(Current);

        if (Current <= 0f) onDeath?.Invoke();
    }

    
    public void RestoreFull()
    {
        if (Current <= 0f) return;

        Current = maxHealth;
        onRestored?.Invoke();
    }
}