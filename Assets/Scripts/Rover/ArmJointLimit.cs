using UnityEngine;

public class ArmJointLimit : MonoBehaviour
{
    [SerializeField] private Transform joint;   

    private void LateUpdate()
    {
        Quaternion q = joint.localRotation;

        
        Quaternion twistZ = new Quaternion(0f, 0f, q.z, q.w);
        float magnitude = Mathf.Sqrt(twistZ.z * twistZ.z + twistZ.w * twistZ.w);

        joint.localRotation = magnitude < 0.0001f
            ? Quaternion.identity
            : new Quaternion(0f, 0f, twistZ.z / magnitude, twistZ.w / magnitude);
    }
}