using UnityEngine;

public class RadarScanRing : MonoBehaviour
{
    private void Awake()
    {
        LineRenderer line = GetComponent<LineRenderer>();

        int segments = 128;
        line.positionCount = segments;
        line.loop = true;
        line.useWorldSpace = false;

        for (int i = 0; i < segments; i++)
        {
            float angle = i * 2f * Mathf.PI / segments;

            Vector3 point = new Vector3(Mathf.Cos(angle) * 0.5f, 0f, Mathf.Sin(angle) * 0.5f);
            line.SetPosition(i, point);
        }
    }
}
