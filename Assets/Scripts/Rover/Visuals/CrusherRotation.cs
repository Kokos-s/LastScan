using UnityEngine;

public class CrusherRotation : MonoBehaviour
{
    
    public float rotationSpeed = 100f;

    void Update()
    {
        
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }
}