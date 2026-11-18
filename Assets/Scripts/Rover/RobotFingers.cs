using UnityEngine;

public class RobotFingers : MonoBehaviour
{
    [SerializeField] private Transform[] fingers;   
    [SerializeField] private float angle = 45f;     
    [SerializeField] private float speed = 5f;      

    private float amount;   
    private float target;

    public bool IsClosed { get { return amount >= 0.99f; } }

    public void Close() { target = 1f; }
    public void Release() { target = 0f; }

    private void OnDisable()
    {
        amount = target = 0f;
    }

    private void LateUpdate()
    {
        amount = Mathf.MoveTowards(amount, target, speed * Time.deltaTime);

        if (amount <= 0f)
            return;

        for (int i = 0; i < fingers.Length; i++)
        {
            float sign = (i == 0) ? 1f : -1f;
            fingers[i].localRotation *= Quaternion.Euler(sign * angle * amount, 0f, 0f);
        }
    }
}