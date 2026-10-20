using UnityEngine;
using UnityEngine.UI;

public class MinimapNoise : MonoBehaviour
{
    [SerializeField] private int textureSize = 64;
    [SerializeField] private float changeInterval = 0.06f;
    [SerializeField] private float opacity = 0.2f;

    private RawImage noiseImage;
    private Texture2D noiseTexture;
    private Color32[] pixels;
    private float timer;

    private void Awake()
    {
        noiseImage = GetComponent<RawImage>();
        noiseTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        noiseTexture.filterMode = FilterMode.Point;
        pixels = new Color32[textureSize * textureSize];
        noiseImage.texture = noiseTexture;
    }

    private void OnEnable()
    {
        noiseImage.enabled = true;
        timer = 0f;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer > 0f)
            return;

        timer = changeInterval;

        for (int i = 0; i < pixels.Length; i++)
        {
            byte brightness = (byte)Random.Range(0, 256);
            pixels[i] = new Color32(brightness, brightness, brightness, 255);
        }

        noiseTexture.SetPixels32(pixels);
        noiseTexture.Apply(false);

        noiseImage.color = new Color(1f, 1f, 1f, opacity);
    }

    private void OnDisable()
    {
        if (noiseImage != null)
            noiseImage.enabled = false;
    }

    private void OnDestroy()
    {
        if (noiseTexture != null)
            Destroy(noiseTexture);
    }
}