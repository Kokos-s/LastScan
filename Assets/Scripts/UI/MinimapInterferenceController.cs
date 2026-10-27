using System.Collections.Generic;
using UnityEngine;

public class MinimapInterferenceController : MonoBehaviour
{
    [SerializeField] private Transform rover;
    [SerializeField] private MagnetRange[] anomalies;
    [SerializeField] private CanvasGroup[] interferenceLayers;

    private static readonly HashSet<Sandstorm> sandstorms = new();
    private static readonly HashSet<LightningStormController> lightningStorms = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSources()
    {
        sandstorms.Clear();
        lightningStorms.Clear();
    }

    public static void Register(Sandstorm storm)
    {
        sandstorms.Add(storm);
    }

    public static void Unregister(Sandstorm storm)
    {
        sandstorms.Remove(storm);
    }

    public static void Register(LightningStormController storm)
    {
        lightningStorms.Add(storm);
    }

    public static void Unregister(LightningStormController storm)
    {
        lightningStorms.Remove(storm);
    }

    private void LateUpdate()
    {
        float strength = 0f;

        if (anomalies != null)
        {
            foreach (MagnetRange anomaly in anomalies)
            {
                if (anomaly == null)
                    continue;

                strength = Mathf.Max(strength, anomaly.InterferenceStrength);
            }
        }

        if (rover != null)
        {
            Vector3 roverPosition = rover.position;

            foreach (Sandstorm storm in sandstorms)
            {
                if (storm == null || !storm.isActiveAndEnabled)
                    continue;

                strength = Mathf.Max(strength, storm.GetInterferenceStrength(roverPosition));
            }

            foreach (LightningStormController storm in lightningStorms)
            {
                if (storm == null || !storm.isActiveAndEnabled)
                    continue;

                strength = Mathf.Max(strength, storm.GetInterferenceStrength(roverPosition));
            }
        }

        if (interferenceLayers == null)
            return;

        foreach (CanvasGroup layer in interferenceLayers)
        {
            if (layer != null)
                layer.alpha = strength;
        }
    }
}