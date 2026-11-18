using System.Collections.Generic;
using UnityEngine;

public class ChargeSpeedLines : MonoBehaviour
{
    
    [SerializeField] private float minSpeed = 15f;       
    [SerializeField] private float fullSpeed = 70f;      

    [SerializeField] private float trailTime = 0.3f;     
    [SerializeField] private float lineWidth = 0.35f;    
    [SerializeField] private Color lineColor = new Color(1f, 0.55f, 0.35f, 0.9f);
    [SerializeField] private Material lineMaterial;      

    [SerializeField] private bool useBodyBox = true;     
    [SerializeField] private int heightLevels = 3;       
    [SerializeField] private bool centerLines = true;    
    [SerializeField] private float sideInset = 0.9f;     
    [SerializeField] private Transform[] extraAnchors;   

    private BossController boss;
    private BoxCollider box;
    private readonly List<TrailRenderer> trails = new List<TrailRenderer>();
    private Material mat;
    private Vector3 lastPos;
    private bool emitting;

    private void Awake()
    {
        boss = GetComponent<BossController>();
        box = GetComponent<BoxCollider>();

        mat = lineMaterial != null ? new Material(lineMaterial) : new Material(Shader.Find("Sprites/Default"));

        if (useBodyBox && box != null) BuildFromBox();
        if (extraAnchors != null)
            foreach (var a in extraAnchors)
                if (a != null) AddTrail(a, Vector3.zero);

        lastPos = transform.position;
        SetEmitting(false);
    }

    private void BuildFromBox()
    {
        Vector3 c = box.center;
        Vector3 half = box.size * 0.5f;

        int levels = Mathf.Max(1, heightLevels);
        for (int i = 0; i < levels; i++)
        {
            float t = levels == 1 ? 0.5f : (float)i / (levels - 1);
            float y = Mathf.Lerp(-half.y * 0.6f, half.y * 0.8f, t);

            AddTrail(box.transform, new Vector3(c.x - half.x * sideInset, c.y + y, c.z - half.z));
            AddTrail(box.transform, new Vector3(c.x + half.x * sideInset, c.y + y, c.z - half.z));

            if (centerLines)
                AddTrail(box.transform, new Vector3(c.x, c.y + y, c.z - half.z));
        }
    }

    private void AddTrail(Transform parent, Vector3 localPos)
    {
        var go = new GameObject("SpeedLine");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;

        var tr = go.AddComponent<TrailRenderer>();
        tr.sharedMaterial = mat;
        tr.time = trailTime;
        tr.minVertexDistance = 0.3f;
        tr.alignment = LineAlignment.View;
        tr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        tr.receiveShadows = false;
        tr.autodestruct = false;
        tr.emitting = false;

        
        tr.widthCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
        tr.widthMultiplier = lineWidth;

        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(lineColor, 0f), new GradientColorKey(lineColor, 1f) },
            new[] { new GradientAlphaKey(lineColor.a, 0f), new GradientAlphaKey(0f, 1f) });
        tr.colorGradient = g;

        trails.Add(tr);
    }

    private void Update()
    {
        float dt = Mathf.Max(Time.deltaTime, 0.0001f);
        float speed = (transform.position - lastPos).magnitude / dt;
        lastPos = transform.position;

        bool charging = boss != null && boss.CurrentState == BossController.State.ChargeAttack;
        bool want = charging && speed > minSpeed;

        if (want != emitting) SetEmitting(want);

        if (emitting)
        {
            float k = Mathf.Clamp01(speed / Mathf.Max(1f, fullSpeed));
            foreach (var t in trails)
                if (t != null) t.widthMultiplier = lineWidth * Mathf.Lerp(0.4f, 1f, k);
        }
    }

    private void SetEmitting(bool on)
    {
        emitting = on;
        foreach (var t in trails)
        {
            if (t == null) continue;
            if (on) t.Clear();       
            t.emitting = on;         
        }
    }

    private void OnDisable()
    {
        foreach (var t in trails)
        {
            if (t == null) continue;
            t.emitting = false;
            t.Clear();
        }
        emitting = false;
    }

    private void OnDestroy()
    {
        if (mat != null) Destroy(mat);
    }
}