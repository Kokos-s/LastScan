using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 15f, -25f);

    void LateUpdate()
    {
        transform.position = target.position + offset;
    }
}