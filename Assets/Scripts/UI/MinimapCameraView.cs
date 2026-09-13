using UnityEngine;

public class MinimapCameraView : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private RectTransform minimap;

    private RectTransform cone;

    private void Awake()
    {
        cone = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        Vector3 position = minimapCamera.WorldToViewportPoint(playerCamera.transform.position);
        cone.anchoredPosition = new Vector2((position.x - 0.5f) * minimap.rect.width, (position.y - 0.5f) * minimap.rect.height);
        float angle = minimapCamera.transform.eulerAngles.y - playerCamera.transform.eulerAngles.y;
        cone.localRotation = Quaternion.Euler(0f, 0f, angle + 45f);
    }
}
