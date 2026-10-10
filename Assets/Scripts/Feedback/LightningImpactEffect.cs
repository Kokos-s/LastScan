using UnityEngine;
using DigitalRuby.LightningBolt;

public class LightningImpactEffect : MonoBehaviour
{
    public static LightningImpactEffect Instance { get; private set; }

    [SerializeField] private LightningBoltScript lightningPrefab;
    [SerializeField] private float lightningLifetime = 0.25f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void TriggerLightning(Transform from, Transform to)
    {
        if (lightningPrefab == null || from == null || to == null)
            return;

        LightningBoltScript instance = Instantiate(lightningPrefab);
        instance.StartObject = from.gameObject;
        instance.EndObject = to.gameObject;

        Destroy(instance.gameObject, lightningLifetime);
    }
}