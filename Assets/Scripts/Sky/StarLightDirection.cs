using UnityEngine;

public class StarLightDirection : MonoBehaviour
{
    [SerializeField] private Transform star;
    [SerializeField] private Transform worldCenter;

    private void LateUpdate()
    {
        Vector3 direction = worldCenter.position - star.position;

        if (direction.sqrMagnitude > 0f)
            transform.rotation = Quaternion.LookRotation(direction);
    }
}
