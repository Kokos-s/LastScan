using UnityEngine;

public class BossHealthReset : MonoBehaviour
{
    [SerializeField] private float graceAfterRespawn = 0.5f;   

    private Health bossHealth;
    private RoverHealth roverHealth;
    private bool roverWasDead;
    private float graceUntil;

    private void Awake()
    {
        bossHealth = GetComponent<Health>();
    }

    private void OnDisable()
    {
        if (bossHealth != null) bossHealth.Invulnerable = false;
    }

    private void Update()
    {
        if (bossHealth == null) return;

        if (roverHealth == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) roverHealth = p.GetComponentInParent<RoverHealth>();
            if (roverHealth == null) return;
            roverWasDead = roverHealth.IsDead;
        }

        bool dead = roverHealth.IsDead;

        if (dead && !roverWasDead)
        {
            
            bossHealth.Invulnerable = true;
            bossHealth.RestoreFull();
        }
        else if (!dead && roverWasDead)
        {
            
            graceUntil = Time.time + graceAfterRespawn;
        }

        roverWasDead = dead;

        if (dead)
            bossHealth.Invulnerable = true;
        else if (Time.time >= graceUntil)
            bossHealth.Invulnerable = false;
    }
}