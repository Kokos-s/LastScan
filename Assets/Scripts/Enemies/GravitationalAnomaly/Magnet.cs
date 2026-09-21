using UnityEngine;

public class MagnetRange : MonoBehaviour
{
    public float attractionForce = 17f;
    public float maxForceDistance = 30f;
    public float extraDownForce = 2f;

    private Rigidbody playerRb;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerRb = other.attachedRigidbody;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerRb = null;
        }
    }

    private void FixedUpdate()
    {
        if (playerRb == null) return;

        Vector3 flatDirection = transform.position - playerRb.position;
        flatDirection.y = 0f;
        flatDirection = flatDirection.normalized;

        float distance = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(playerRb.position.x, 0, playerRb.position.z)
        );

        if (distance <= maxForceDistance)
        {
            playerRb.AddForce(flatDirection * attractionForce, ForceMode.Acceleration);
            playerRb.AddForce(Vector3.down * extraDownForce, ForceMode.Acceleration);
        }
    }
}