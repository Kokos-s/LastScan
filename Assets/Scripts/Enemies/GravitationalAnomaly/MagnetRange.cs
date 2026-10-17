using UnityEngine;

public class MagnetRange : MonoBehaviour
{
    public float attractionForce = 17f;
    public float maxForceDistance = 100f;
    public float extraDownForce = 2f;
    [SerializeField] private Transform rover;
    [SerializeField] private float interferenceStartDistance = 80f;
    [SerializeField] private float interferenceFullDistance = 10f;

    private Rigidbody playerRb;

    public float InterferenceStrength
    {
        get
        {
            if (!isActiveAndEnabled || rover == null)
                return 0f;

            Vector3 direction = transform.position - rover.position;
            direction.y = 0f;
            float distance = direction.magnitude;
            float strength = Mathf.InverseLerp(interferenceStartDistance, interferenceFullDistance, distance);
            return strength * strength * strength;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerRb = other.attachedRigidbody;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerRb = null;
    }

    private void FixedUpdate()
    {
        if (playerRb == null)
            return;

        if (maxForceDistance <= 0f)
            return;

        Vector3 direction = transform.position - playerRb.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance > maxForceDistance)
            return;

        float strength = 1f - Mathf.InverseLerp(0f, maxForceDistance, distance);
        float currentForce = attractionForce * strength;
        playerRb.AddForce(direction.normalized * currentForce, ForceMode.Acceleration);
        playerRb.AddForce(Vector3.down * extraDownForce * strength, ForceMode.Acceleration);
    }
}