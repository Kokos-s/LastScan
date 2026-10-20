using UnityEngine;

public class WreckOre : MonoBehaviour
{
    [SerializeField] private int storedOre;

    [Header("Свечение")]
    [SerializeField] private Color glowColor = new Color(0.4f, 0.95f, 1f);
    [SerializeField] private Vector3 glowSize = new Vector3(3f, 1.2f, 4.5f);  // область, где летят частицы (локальные оси остова)
    [SerializeField] private float glowHeight = 1f;
    [SerializeField] private float lightRange = 16f;
    [SerializeField] private float idleLight = 3f;
    [SerializeField] private float chargeLight = 12f;
    [SerializeField] private float idleRate = 14f;
    [SerializeField] private float chargeRate = 60f;
    [SerializeField] private float pulseSpeed = 2.5f;

    private Light glow;
    private ParticleSystem motes;
    private float charge;

    public int StoredOre
    {
        get { return storedOre; }
    }

    private void Awake()
    {
        BuildLight();
        BuildParticles();
    }

    private void BuildLight()
    {
        GameObject go = new GameObject("OreGlow");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.up * (glowHeight + 1f);

        glow = go.AddComponent<Light>();
        glow.type = LightType.Point;
        glow.color = glowColor;
        glow.range = lightRange;
        glow.shadows = LightShadows.None;
        glow.enabled = false;
    }

    private void BuildParticles()
    {
        GameObject go = new GameObject("OreMotes");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.up * glowHeight;

        motes = go.AddComponent<ParticleSystem>();
        motes.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = motes.main;
        main.loop = true;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.55f);
        main.startColor = glowColor;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 200;

        var shape = motes.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = glowSize;

        // Частицы медленно поднимаются вверх (все три оси в одном режиме: Two Constants)
        var vel = motes.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.x = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
        vel.y = new ParticleSystem.MinMaxCurve(0.8f, 1.6f);
        vel.z = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);

        // Плавное появление и затухание
        var alpha = motes.colorOverLifetime;
        alpha.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(0f, 1f) });
        alpha.color = g;

        var rend = go.GetComponent<ParticleSystemRenderer>();
        rend.renderMode = ParticleSystemRenderMode.Billboard;
        rend.sharedMaterial = OreFx.Sprite();
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        var em = motes.emission;
        em.rateOverTime = 0f;
    }

    private void Update()
    {
        if (glow == null || motes == null) return;

        bool hasOre = storedOre > 0;
        var em = motes.emission;

        if (!hasOre)
        {
            glow.enabled = false;
            em.rateOverTime = 0f;
            return;
        }

        if (!motes.isPlaying) motes.Play();

        glow.enabled = true;
        float pulse = 0.8f + 0.2f * Mathf.Sin(Time.time * pulseSpeed);
        glow.intensity = Mathf.Lerp(idleLight * pulse, chargeLight, charge);
        em.rateOverTime = Mathf.Lerp(idleRate, chargeRate, charge);
    }

    // 0..1, вызывается из WreckOrePickup, пока игрок держит кнопку забора
    public void SetCharge(float value)
    {
        charge = Mathf.Clamp01(value);
    }

    public void SetOre(int amount)
    {
        storedOre = amount;
    }

    public int TakeOre()
    {
        int amount = storedOre;
        storedOre = 0;
        charge = 0f;
        return amount;
    }
}