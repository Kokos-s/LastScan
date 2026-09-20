using UnityEngine;

public class ScanReveal : MonoBehaviour
{
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