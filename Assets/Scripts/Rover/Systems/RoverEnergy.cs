using UnityEngine;

public class RoverEnergy : MonoBehaviour
{
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float currentEnergy;
    [SerializeField] private float movementDrainRate = 2f;
    [SerializeField] private float regenRate = 5f;
    [SerializeField] private float drainMultiplier = 1f;

    private bool isPlayerMoving = false;

    public float CurrentEnergy
    {
        get { return currentEnergy; }
    }
    public float MaxEnergy
    {
        get { return maxEnergy; }
    }

    void Start()
    {
        currentEnergy = maxEnergy;
    }

    void LateUpdate()
    {
        if (isPlayerMoving)
            currentEnergy -= movementDrainRate * drainMultiplier * Time.deltaTime;

        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
        isPlayerMoving = false;
    }

    public void ReportPlayerMovement()
    {
        isPlayerMoving = true;
    }

    public bool TryConsume(float amount)
    {
        float actualAmount = amount * drainMultiplier;

        if (currentEnergy >= actualAmount)
        {
            currentEnergy -= actualAmount;
            return true;
        }

        return false;
    }

    public void AddEnergy(float amount)
    {
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
    }

    public void SetDrainMultiplier(float multiplier)
    {
        drainMultiplier = multiplier;
    }
}