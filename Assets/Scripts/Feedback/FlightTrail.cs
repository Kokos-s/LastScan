using UnityEngine;

public class FlightTrail : MonoBehaviour
{
    private RockProjectile rock;
    private BossDisc disc;
    private TrailRenderer[] trails;
    private ParticleSystem[] particles;
    private bool active;

    private void Awake()
    {
        rock = GetComponent<RockProjectile>();
        disc = GetComponent<BossDisc>();
        trails = GetComponentsInChildren<TrailRenderer>(true);
        particles = GetComponentsInChildren<ParticleSystem>(true);
        Apply(false);
    }

    private void Update()
    {
        bool thrown = false;
        if (rock != null) thrown = rock.IsThrown;
        else if (disc != null) thrown = disc.State == BossDisc.DiscState.Flying;

        if (thrown != active) Apply(thrown);
    }

    private void Apply(bool on)
    {
        active = on;

        foreach (var t in trails)
        {
            if (t == null) continue;
            if (on) t.Clear();
            t.emitting = on;
        }

        foreach (var p in particles)
        {
            if (p == null) continue;
            var em = p.emission;
            em.enabled = on;
            if (on && !p.isPlaying) p.Play();
        }
    }

    
    private void OnDestroy()
    {
        if (!gameObject.scene.isLoaded) return;

        foreach (var t in trails)
        {
            if (t == null) continue;
            t.emitting = false;
            t.transform.SetParent(null);
            Destroy(t.gameObject, t.time + 0.2f);
        }
        foreach (var p in particles)
        {
            if (p == null || p.GetComponent<TrailRenderer>() != null) continue;
            p.transform.SetParent(null);
            Destroy(p.gameObject, 3f);
        }
    }
}