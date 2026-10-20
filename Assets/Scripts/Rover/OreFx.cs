using UnityEngine;

// Общие материалы и фабрика эффектов для забора руды
public static class OreFx
{
    private static Material spriteMat;
    private static Texture2D softTexture;

    // Мягкий круг для частиц
    public static Texture2D SoftCircle()
    {
        if (softTexture != null) return softTexture;

        const int size = 64;
        softTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        softTexture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) / size * 2f - 1f;
                float dy = (y + 0.5f) / size * 2f - 1f;
                float d = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy));
                float a = Mathf.Pow(1f - d, 2f);
                softTexture.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        softTexture.Apply();
        return softTexture;
    }

    // Мягкий белый материал, не зависит от освещения (годится и для URP, и для Built-in)
    public static Material Sprite()
    {
        if (spriteMat != null) return spriteMat;

        spriteMat = new Material(Shader.Find("Sprites/Default"));
        spriteMat.mainTexture = SoftCircle();
        spriteMat.color = Color.white;
        return spriteMat;
    }

    // Плоский белый материал для сфер-минералов
    private static Material solidMat;
    public static Material Solid()
    {
        if (solidMat != null) return solidMat;

        solidMat = new Material(Shader.Find("Sprites/Default"));
        solidMat.color = Color.white;
        return solidMat;
    }

    // Белая светящаяся сфера со шлейфом и собственным светом
    public static GameObject CreateSpark(Vector3 position, float scale, Material overrideMaterial)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Object.Destroy(go.GetComponent<Collider>());
        go.name = "OreSpark";
        go.transform.position = position;
        go.transform.localScale = Vector3.one * scale;

        Renderer r = go.GetComponent<Renderer>();
        r.sharedMaterial = overrideMaterial != null ? overrideMaterial : Solid();
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;

        TrailRenderer trail = go.AddComponent<TrailRenderer>();
        trail.sharedMaterial = Sprite();
        trail.time = 0.4f;
        trail.startWidth = scale * 0.9f;
        trail.endWidth = 0f;
        trail.minVertexDistance = 0.05f;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        Gradient g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.7f, 0.95f, 1f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        trail.colorGradient = g;

        Light l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = Color.white;
        l.range = 5f;
        l.intensity = 2.5f;
        l.shadows = LightShadows.None;

        return go;
    }
}