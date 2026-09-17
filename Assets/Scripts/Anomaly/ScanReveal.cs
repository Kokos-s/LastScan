using UnityEngine;

public class ScanReveal : MonoBehaviour
{
   /* [SerializeField] private Renderer targetRenderer;
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
    }   */

    [SerializeField] private GameObject radarMarker;
    [SerializeField] private float revealDuration = 15f;

    private float revealTimer;

    private void OnEnable()
    {
        revealTimer = 0f;
        radarMarker.SetActive(false);
    }

    private void OnDisable()
    {
        if (radarMarker != null)
            radarMarker.SetActive(false);
    }

    private void Update()
    {
        if (revealTimer <= 0f)
            return;

        revealTimer -= Time.deltaTime;

        if (revealTimer <= 0f)
            radarMarker.SetActive(false);
    }

    public void Reveal()
    {
        if (!isActiveAndEnabled)
            return;

        radarMarker.SetActive(true);
        revealTimer = revealDuration;
    }

}