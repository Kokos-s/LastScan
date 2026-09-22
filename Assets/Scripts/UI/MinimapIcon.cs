using UnityEngine;

[DefaultExecutionOrder(100)]
public class MinimapIcon : MonoBehaviour
{
    [SerializeField] private Transform minimapCamera;

    private void LateUpdate()
    {
        transform.rotation = minimapCamera.rotation;
    }
}