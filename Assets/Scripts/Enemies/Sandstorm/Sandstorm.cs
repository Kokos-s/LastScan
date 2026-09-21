using UnityEngine;

public class Sandstorm : MonoBehaviour
{
    [SerializeField] private float stormRadius = 55f;

    [SerializeField] private float controlResistance = 0.75f;

    [SerializeField] private float energyDrainMultiplier = 3f;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        RoverMovement movement = rb.GetComponent<RoverMovement>();
        RoverEnergy energy = rb.GetComponent<RoverEnergy>();

        movement?.SetStormResistance(controlResistance);
        energy?.SetDrainMultiplier(energyDrainMultiplier);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        RoverMovement movement = rb.GetComponent<RoverMovement>();
        RoverEnergy energy = rb.GetComponent<RoverEnergy>();

        movement?.SetStormResistance(0f);
        energy?.SetDrainMultiplier(1f);
    }
}
