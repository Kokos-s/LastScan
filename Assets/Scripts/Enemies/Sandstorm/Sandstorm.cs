using UnityEngine;

public class Sandstorm : MonoBehaviour
{
    [SerializeField] private float stormRadius = 55f;
    [SerializeField] private float controlResistance = 0.75f;
    [SerializeField] private float energyDrainMultiplier = 3f;
    private Rigidbody affectedRover;

    private void OnTriggerStay(Collider other)
    {
        if (!isActiveAndEnabled)
            return;

        if (!other.CompareTag("Player")) return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;
        affectedRover = rb;

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

    private void OnDisable()
    {
        if (affectedRover == null)
            return;

        RoverMovement movement = affectedRover.GetComponent<RoverMovement>();
        RoverEnergy energy = affectedRover.GetComponent<RoverEnergy>();

        if (movement != null)
            movement.SetStormResistance(0f);

        if (energy != null)
            energy.SetDrainMultiplier(1f);

        affectedRover = null;
    }
}
