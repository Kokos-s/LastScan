using UnityEngine;
using UnityEngine.UI;

public class MinimapInterference : MonoBehaviour
{
    [SerializeField] private float minChangeInterval = 0.05f;
    [SerializeField] private float maxChangeInterval = 0.2f;
    [SerializeField] private Color firstColor = Color.white;
    [SerializeField] private Color secondColor = Color.cyan;
    [SerializeField] private float minHeight = 1f;
    [SerializeField] private float maxHeight = 10f;
    [SerializeField] private float minColorTransparency = 0.02f;
    [SerializeField] private float maxColorTransparency = 0.5f;

    private Image lineImage;
    private RectTransform lineRect;
    private RectTransform minimapRect;
    private float timer;

    private void Awake()
    {
        lineImage = GetComponent<Image>();
        lineRect = GetComponent<RectTransform>();
        minimapRect = transform.parent.GetComponent<RectTransform>();
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer > 0f)
            return;

        float changeInterval = Random.Range(minChangeInterval, maxChangeInterval);
        timer = changeInterval;

        lineImage.enabled = Random.value > 0.3f;

        float height = Random.Range(minHeight, maxHeight);
        lineRect.sizeDelta = new Vector2(lineRect.sizeDelta.x, height);

        float halfHeight = minimapRect.rect.height * 0.5f;
        float limit = Mathf.Max(0f, halfHeight - height * 0.5f);

        Vector2 position = lineRect.anchoredPosition;
        position.y = Random.Range(-limit, limit);
        lineRect.anchoredPosition = position;

        Color color = Color.Lerp(firstColor, secondColor, Random.value);
        color.a = Random.Range(minColorTransparency, maxColorTransparency);
        lineImage.color = color;
    }

    private void OnDisable()
    {
        if (lineImage != null)
            lineImage.enabled = false;
    }
}