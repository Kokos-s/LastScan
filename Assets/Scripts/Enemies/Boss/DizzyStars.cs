using UnityEngine;

public class DizzyStars : MonoBehaviour
{
    [SerializeField] private int count = 5;
    [SerializeField] private float orbitRadius = 5f;
    [SerializeField] private float heightAboveTop = 2f;     
    [SerializeField] private float orbitSpeed = 160f;       
    [SerializeField] private float starSize = 1.8f;
    [SerializeField] private float bobAmount = 0.4f;
    [SerializeField] private float selfSpinSpeed = 240f;
    [SerializeField] private Color starColor = new Color(1f, 0.9f, 0.2f, 1f);
    [SerializeField] private Material starMaterial;         
    [SerializeField] private float fadeTime = 0.25f;

    private Transform root;
    private Transform[] stars;
    private Material mat;
    private Mesh mesh;
    private bool visible;
    private float amount;
    private float angle;
    private float topY = 4f;

    public void Show()
    {
        if (root == null) Build();

        var col = GetComponent<Collider>();
        topY = (col != null ? col.bounds.max.y - transform.position.y : 4f) + heightAboveTop;

        root.gameObject.SetActive(true);
        visible = true;
    }

    public void Hide(bool immediate = false)
    {
        visible = false;
        if (immediate)
        {
            amount = 0f;
            if (root != null) root.gameObject.SetActive(false);
        }
    }

    private void Build()
    {
        root = new GameObject("DizzyStars").transform;

        mesh = BuildStarMesh();
        mat = starMaterial != null ? new Material(starMaterial) : new Material(Shader.Find("Sprites/Default"));
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", starColor);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", starColor);

        stars = new Transform[count];
        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("Star" + i);
            go.transform.SetParent(root, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            stars[i] = go.transform;
        }
    }

    private Mesh BuildStarMesh()
    {
        var m = new Mesh { name = "DizzyStar" };
        var v = new Vector3[11];
        v[0] = Vector3.zero;
        for (int i = 0; i < 10; i++)
        {
            float r = (i % 2 == 0) ? 0.5f : 0.2f;
            float a = Mathf.PI / 2f + i * Mathf.PI / 5f;
            v[i + 1] = new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f);
        }

        var tris = new int[30];
        for (int i = 0; i < 10; i++)
        {
            tris[i * 3] = 0;
            tris[i * 3 + 1] = i + 1;
            tris[i * 3 + 2] = (i + 1) % 10 + 1;
        }

        m.vertices = v;
        m.triangles = tris;
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }

    private void LateUpdate()
    {
        if (root == null || !root.gameObject.activeSelf) return;

        amount = Mathf.MoveTowards(amount, visible ? 1f : 0f, Time.deltaTime / Mathf.Max(0.01f, fadeTime));
        if (amount <= 0f && !visible)
        {
            root.gameObject.SetActive(false);
            return;
        }

        angle += orbitSpeed * Time.deltaTime;
        Vector3 center = transform.position + Vector3.up * topY;
        Camera cam = Camera.main;

        for (int i = 0; i < stars.Length; i++)
        {
            float a = (angle + i * 360f / stars.Length) * Mathf.Deg2Rad;
            Vector3 pos = center
                          + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * orbitRadius
                          + Vector3.up * (Mathf.Sin(Time.time * 3f + i * 1.3f) * bobAmount);

            stars[i].position = pos;
            stars[i].rotation = (cam != null ? cam.transform.rotation : Quaternion.identity)
                                * Quaternion.Euler(0f, 0f, Time.time * selfSpinSpeed + i * 40f);
            stars[i].localScale = Vector3.one * (starSize * amount);
        }
    }

    private void OnDisable()
    {
        visible = false;
        amount = 0f;
        if (root != null) root.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (root != null) Destroy(root.gameObject);
        if (mat != null) Destroy(mat);
        if (mesh != null) Destroy(mesh);
    }
}