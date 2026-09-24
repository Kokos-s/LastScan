using UnityEngine;

public class UnloadZoneLights : MonoBehaviour
{
    [SerializeField] private float speed = 0.1f;

    private Material lineMaterial;
    private float offset;

    private void Start()
    {
        lineMaterial = GetComponent<LineRenderer>().material;
    }

    private void Update()
    {
        offset += speed * Time.deltaTime;
        offset = Mathf.Repeat(offset, 1f);

        lineMaterial.SetTextureOffset("_BaseMap", new Vector2(offset, 0f));
    }

    private void OnDestroy()
    {
        if (lineMaterial != null)
            Destroy(lineMaterial);
    }
}

