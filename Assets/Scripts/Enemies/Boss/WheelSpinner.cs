using UnityEngine;

public class WheelSpinner : MonoBehaviour
{
    [SerializeField] private Transform[] wheels;
    [SerializeField] private float wheelRadius = 0.4f;
    [SerializeField] private Vector3 spinAxis = Vector3.right;

    private Vector3 lastPos;

    private void Start() => lastPos = transform.position;

    private void LateUpdate()
    {
        Vector3 delta = transform.position - lastPos;
        lastPos = transform.position;

        float distance = Vector3.Dot(delta, transform.forward);
        float angle = distance / wheelRadius * Mathf.Rad2Deg;

        foreach (var w in wheels)
            w.Rotate(spinAxis, angle, Space.Self);
    }
}