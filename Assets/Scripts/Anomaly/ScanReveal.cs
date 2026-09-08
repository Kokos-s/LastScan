using UnityEngine;

public class ScanReveal : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material hiddenMaterial;
    [SerializeField] private Material revealedMaterial;
    [SerializeField] private float revealDuration = 10f;
    private float revealTimer = 0f;

    void OnEnable()
    {
        targetRenderer.sharedMaterial = hiddenMaterial;
        revealTimer = 0f;
    }

    void OnDisable()
    {
        if (targetRenderer != null)
            targetRenderer.sharedMaterial = revealedMaterial;
    }

    void Update()
    {
        if (revealTimer > 0f)
        {
            revealTimer -= Time.deltaTime;

            if (revealTimer <= 0f)
                targetRenderer.sharedMaterial = hiddenMaterial;
        }
    }

    public void Reveal()
    {
        if (!isActiveAndEnabled)
            return;

        targetRenderer.sharedMaterial = revealedMaterial;
        revealTimer = revealDuration;
    }
}