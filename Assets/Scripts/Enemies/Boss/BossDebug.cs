using UnityEngine;
using UnityEngine.AI;

public class BossDebug : MonoBehaviour
{
    private BossController boss;
    private NavMeshAgent agent;
    private Transform player;
    private float nextLog;

    private void Awake()
    {
        boss = GetComponent<BossController>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else return;
        }

        if (Time.time < nextLog) return;
        nextLog = Time.time + 1f;

        float dist = Vector3.Distance(transform.position, player.position);
        bool playerOnMesh = NavMesh.SamplePosition(player.position, out NavMeshHit hit, 2f, NavMesh.AllAreas);

        Debug.Log("Состояние: " + boss.CurrentState
            + " | Дистанция: " + dist.ToString("F1")
            + " | Босс на NavMesh: " + agent.isOnNavMesh
            + " | Путь: " + (agent.isOnNavMesh ? agent.pathStatus.ToString() : "-")
            + " | Остановлен: " + (agent.isOnNavMesh ? agent.isStopped.ToString() : "-")
            + " | Ровер на NavMesh: " + playerOnMesh);
    }
}