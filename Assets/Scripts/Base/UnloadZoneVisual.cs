using UnityEngine;

public class UnloadZoneVisual : MonoBehaviour
{
    [SerializeField] private BoxCollider unloadZone;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private int pointsPerSide = 20;
    [SerializeField] private float heightOffset = 0.05f;
    [SerializeField] private float rayHeight = 30f;

    private LineRenderer line;

    private void Start()
    {
        line = GetComponent<LineRenderer>();

        Vector3 center = unloadZone.center;
        Vector3 halfSize = unloadZone.size / 2f;

        float left = center.x - halfSize.x;
        float right = center.x + halfSize.x;
        float back = center.z - halfSize.z;
        float front = center.z + halfSize.z;
        float bottom = center.y - halfSize.y;

        line.useWorldSpace = true;
        line.loop = true;
        line.positionCount = pointsPerSide * 4;

        for (int i = 0; i < pointsPerSide; i++)
        {
            float progress = (float)i / pointsPerSide;
            PlacePoint(i, Mathf.Lerp(left, right, progress), bottom, back);
            PlacePoint(pointsPerSide + i, right, bottom, Mathf.Lerp(back, front, progress));
            PlacePoint(pointsPerSide * 2 + i, Mathf.Lerp(right, left, progress), bottom, front);
            PlacePoint(pointsPerSide * 3 + i, left, bottom, Mathf.Lerp(front, back, progress));
        }
    }

    private void PlacePoint(int index, float x, float y, float z)
    {
        Vector3 point = unloadZone.transform.TransformPoint(new Vector3(x, y, z));

        Vector3 rayStart = point + Vector3.up * rayHeight;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayHeight * 2f, groundMask, QueryTriggerInteraction.Ignore))
            point = hit.point + Vector3.up * heightOffset;

        line.SetPosition(index, point);
    }
}
