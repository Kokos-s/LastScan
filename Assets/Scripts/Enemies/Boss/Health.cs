using UnityEngine;
using UnityEngine.Events;

public interface IDamageable
{
    void TakeDamage(float amount);
}

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;

    public float Current { get; private set; }
    public float Max => maxHealth;
    public float Normalized => Current / maxHealth;

    public UnityEvent<float> onDamaged;
    public UnityEvent onDeath;

    private void Awake() => Current = maxHealth;

    public void TakeDamage(float amount)
    {
        if (Current <= 0f) return;

        Current = Mathf.Max(0f, Current - amount);
        onDamaged?.Invoke(Current);

        if (Current <= 0f) onDeath?.Invoke();
    }
}