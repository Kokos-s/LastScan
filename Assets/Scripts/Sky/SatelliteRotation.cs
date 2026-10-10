using UnityEngine;

public class SatelliteRotation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 0.5f;
    [SerializeField] private float axisTilt = 25f;

    private Vector3 rotationAxis;

    private void Start()
    {
        Quaternion tilt = Quaternion.Euler(0f, 0f, axisTilt);
        rotationAxis = tilt * Vector3.up;
    }

    private void Update()
    {
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.World);
    }
}
