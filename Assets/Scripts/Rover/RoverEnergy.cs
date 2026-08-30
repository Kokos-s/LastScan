using UnityEngine;

public class RoverEnergy : MonoBehaviour
{
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float currentEnergy;
    [SerializeField] private float movementDrainRate = 2f;
    [SerializeField] private float regenRate = 5f;

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
        {
            currentEnergy -= movementDrainRate * Time.deltaTime;
        }

        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
        isPlayerMoving = false;
        Debug.Log("current energy" + currentEnergy);
    }

    public void ReportPlayerMovement()
    {
        isPlayerMoving = true;
    } 
}
