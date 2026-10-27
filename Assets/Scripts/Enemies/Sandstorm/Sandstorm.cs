using UnityEngine;

public class Sandstorm : MonoBehaviour
{
    [SerializeField] private float stormRadius = 55f;
    [SerializeField] private float controlResistance = 0.75f;
    [SerializeField] private float energyDrainMultiplier = 3f;
    [SerializeField] private float interferenceStrength = 0.75f;
    [SerializeField, Min(0f)] private float interferenceOutsideDistance = 10f;
    [SerializeField, Min(0f)] private float interferencePeakDepth = 5f;

    private Rigidbody affectedRover;

    public float GetInterferenceStrength(Vector3 roverPosition)
    {
        if (!isActiveAndEnabled)
            return 0f;

        Vector3 offset = roverPosition - transform.position;
        offset.y = 0f;

        float radius = Mathf.Max(0f, stormRadius);

        float startDistance = radius + Mathf.Max(0f, interferenceOutsideDistance);

        float peakDistance = Mathf.Max(0f, radius - Mathf.Max(0f, interferencePeakDepth));

        float distance = offset.magnitude;

        float transitionWidth = Mathf.Max(0.001f, startDistance - peakDistance);

        float t = Mathf.Clamp01((startDistance - distance) / transitionWidth);

        return Mathf.SmoothStep(0f, 1f, t) * interferenceStrength;
    }

    private void OnEnable()
    {
        MinimapInterferenceController.Register(this);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isActiveAndEnabled)
            return;

        if (!other.CompareTag("Player"))
            return;

        Rigidbody rb = other.attachedRigidbody;

        if (rb == null)
            return;

        affectedRover = rb;

        RoverMovement movement = rb.GetComponent<RoverMovement>();
        RoverEnergy energy = rb.GetComponent<RoverEnergy>();

        movement?.SetStormResistance(controlResistance);
        energy?.SetDrainMultiplier(energyDrainMultiplier);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        Rigidbody rb = other.attachedRigidbody;

        if (rb == null)
            return;

        RoverMovement movement = rb.GetComponent<RoverMovement>();
        RoverEnergy energy = rb.GetComponent<RoverEnergy>();

        movement?.SetStormResistance(0f);
        energy?.SetDrainMultiplier(1f);

        affectedRover = null;
    }

    private void OnDisable()
    {
        MinimapInterferenceController.Unregister(this);

        if (affectedRover == null)
            return;

        RoverMovement movement =
            affectedRover.GetComponent<RoverMovement>();

        RoverEnergy energy =
            affectedRover.GetComponent<RoverEnergy>();

        movement?.SetStormResistance(0f);
        energy?.SetDrainMultiplier(1f);

        affectedRover = null;
    }
}