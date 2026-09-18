using UnityEngine;

public class Magnet : MonoBehaviour
{
    [Header("Configuración")]
    public Transform enemyCore;
    public float attractionForce = 4f;

    private Transform playerTransform;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerTransform = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerTransform = null;
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;


        Vector3 direction = (enemyCore.position - playerTransform.position).normalized;


        playerTransform.position += direction * attractionForce * Time.deltaTime;
    }
}