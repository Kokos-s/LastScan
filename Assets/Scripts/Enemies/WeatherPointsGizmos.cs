using UnityEngine;

public class WeatherPointsGizmos : MonoBehaviour
{
    [SerializeField] private float pointRadius = 3f;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform point = transform.GetChild(i);
            Gizmos.DrawWireSphere(point.position, pointRadius);
        }
    }
}
