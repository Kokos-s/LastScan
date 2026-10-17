using UnityEngine;

public class MinimapInterferenceController : MonoBehaviour
{
    [SerializeField] private MagnetRange[] anomalies;
    [SerializeField] private CanvasGroup[] interferenceLayers;

    private void LateUpdate()
    {
        float strength = 0f;

        foreach (MagnetRange anomaly in anomalies)
        {
            if (anomaly == null)
                continue;
            strength = Mathf.Max(strength, anomaly.InterferenceStrength);
        }

        foreach (CanvasGroup layer in interferenceLayers)
        {
            if (layer == null)
                continue;
            layer.alpha = strength;
        }
    }
}
