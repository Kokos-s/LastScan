using UnityEngine;

public class RoverGripper : MonoBehaviour
{
    [SerializeField] private Transform fingerA;
    [SerializeField] private Transform fingerB;
    [SerializeField] private Vector3 fingerAAxis = Vector3.up;
    [SerializeField] private Vector3 fingerBAxis = Vector3.up;
    [SerializeField] private float fingerAClosedAngle = 20f;    
    [SerializeField] private float fingerBClosedAngle = -20f;   
    [SerializeField] private float closeTime = 0.2f;
    [SerializeField] private float openTime = 0.12f;

    private Quaternion restA;
    private Quaternion restB;
    private float current;   
    private float target;

    private void Awake()
    {
        if (fingerA != null) restA = fingerA.localRotation;
        if (fingerB != null) restB = fingerB.localRotation;
    }

    public void Close() => target = 1f;
    public void Open() => target = 0f;

    private void OnDisable()
    {
        target = 0f;
        current = 0f;
        Apply();
    }

    
    private void LateUpdate()
    {
        float time = target > current ? closeTime : openTime;
        float speed = 1f / Mathf.Max(0.01f, time);
        current = Mathf.MoveTowards(current, target, speed * Time.deltaTime);
        Apply();
    }

    private void Apply()
    {
        float k = Mathf.SmoothStep(0f, 1f, current);
        if (fingerA != null)
            fingerA.localRotation = restA * Quaternion.AngleAxis(fingerAClosedAngle * k, fingerAAxis);
        if (fingerB != null)
            fingerB.localRotation = restB * Quaternion.AngleAxis(fingerBClosedAngle * k, fingerBAxis);
    }
}