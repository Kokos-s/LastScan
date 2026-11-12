using System.Collections.Generic;
using UnityEngine;

public class PickupHighlight : MonoBehaviour
{
    private Material[] mats;
    private Color[] baseEmission;
    private bool[] supportsEmission;
    private bool initialized;
    private bool on;
    private float amount;

    private Color color = new Color(1f, 0.6f, 0.15f);
    private float intensity = 2.5f;
    private float pulseSpeed = 5f;

    public void Configure(Color c, float i, float speed)
    {
        color = c;
        intensity = i;
        pulseSpeed = speed;
    }

    public void SetHighlighted(bool value)
    {
        if (value && !initialized) Init();
        on = value;
    }

    private void Init()
    {
        initialized = true;

        var list = new List<Material>();
        foreach (var r in GetComponentsInChildren<Renderer>(true))
        {
            if (!(r is MeshRenderer) && !(r is SkinnedMeshRenderer)) continue;
            foreach (var m in r.materials) list.Add(m);   
        }

        mats = list.ToArray();
        baseEmission = new Color[mats.Length];
        supportsEmission = new bool[mats.Length];

        for (int i = 0; i < mats.Length; i++)
        {
            supportsEmission[i] = mats[i].HasProperty("_EmissionColor");
            if (supportsEmission[i])
            {
                baseEmission[i] = mats[i].GetColor("_EmissionColor");
                mats[i].EnableKeyword("_EMISSION");
            }
        }
    }

    private void Update()
    {
        if (!initialized) return;

        float target = on ? 1f : 0f;
        if (Mathf.Approximately(amount, target) && !on) return;

        amount = Mathf.MoveTowards(amount, target, Time.unscaledDeltaTime * 6f);
        float pulse = 0.65f + 0.35f * Mathf.Sin(Time.unscaledTime * pulseSpeed);

        for (int i = 0; i < mats.Length; i++)
        {
            if (mats[i] == null || !supportsEmission[i]) continue;
            mats[i].SetColor("_EmissionColor", baseEmission[i] + color * (intensity * amount * pulse));
        }
    }

    private void OnDestroy()
    {
        if (mats == null) return;
        foreach (var m in mats) if (m != null) Destroy(m);
    }
}
