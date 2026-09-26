using UnityEngine;

public class TornadoAppearance : MonoBehaviour
{
    [SerializeField] private float startHeight = 40f;
    [SerializeField] private float appearanceTime = 5f;

    private Vector3 normalLocalPosition;
    private float timer;

    private void Awake()
    {
        normalLocalPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        timer = 0f;
        SetHeight(startHeight);
    }

    private void LateUpdate()
    {
        if (timer >= appearanceTime)
            return;

        timer += Time.deltaTime;

        float progress = Mathf.Clamp01(timer / appearanceTime);
        float height = Mathf.Lerp(startHeight, 0f, progress);

        SetHeight(height);
    }

    private void SetHeight(float height)
    {
        Vector3 offset = transform.parent.InverseTransformVector(Vector3.up * height);
        transform.localPosition = normalLocalPosition + offset;
    }

    private void OnDisable()
    {
        transform.localPosition = normalLocalPosition;
    }
}
