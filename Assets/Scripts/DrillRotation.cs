using UnityEngine;

public class DrillRotation : MonoBehaviour
{
    [Header("Teeth Rows")]
    public Transform row1;
    public Transform row2;
    public Transform row3;
    public Transform row4;

    [Header("Rotation Settings")]
    public float rotationSpeed = 300f;

    [Tooltip("Check to reverse the rotation direction of all rows")]
    public bool reverseDirection = false;

    void Update()
    {
        
        float directionMultiplier = reverseDirection ? -1f : 1f;

        
        float speedOdd = rotationSpeed * directionMultiplier * Time.deltaTime;
        float speedEven = -rotationSpeed * directionMultiplier * Time.deltaTime; // Negative value inverses the rotation

        
        if (row1 != null) row1.Rotate(Vector3.forward * speedOdd);
        if (row3 != null) row3.Rotate(Vector3.forward * speedOdd);

       
        if (row2 != null) row2.Rotate(Vector3.forward * speedEven);
        if (row4 != null) row4.Rotate(Vector3.forward * speedEven);
    }
}