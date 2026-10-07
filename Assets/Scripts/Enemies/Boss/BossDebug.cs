using UnityEngine;

public class BossDebug : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float rockMinRange = 12f;

    private BossController boss;
    private BossController.State lastState;
    private float nextLog;

    private void Awake()
    {
        boss = GetComponent<BossController>();
        lastState = boss.CurrentState;
    }

    private void Update()
    {
        if (boss.CurrentState != lastState)
        {
            lastState = boss.CurrentState;
            Debug.Log("СОСТОЯНИЕ: " + lastState);
        }

        if (Time.time < nextLog) return;
        nextLog = Time.time + 1.5f;

        float dist = player ? Vector3.Distance(transform.position, player.position) : -1f;
        int total = RockProjectile.All.Count;
        int available = 0;
        string unavailable = "";

        foreach (var r in RockProjectile.All)
        {
            if (r.IsAvailable) available++;
            else
            {
                var rb = r.GetComponent<Rigidbody>();
                unavailable += r.name + "(kinematic=" + rb.isKinematic + ") ";
            }
        }

        Debug.Log("Дистанция до игрока: " + dist.ToString("F1")
            + " | Rock Min Range: " + rockMinRange
            + " | Камней в списке: " + total
            + " | Доступных: " + available
            + " | Недоступные: " + unavailable);
    }
}