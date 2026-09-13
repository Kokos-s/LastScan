using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float height = 150f;

    private void LateUpdate()
    {
        transform.position = target.position + Vector3.up * height;
        transform.rotation = Quaternion.Euler(90f, target.eulerAngles.y, 0f);
    }
}