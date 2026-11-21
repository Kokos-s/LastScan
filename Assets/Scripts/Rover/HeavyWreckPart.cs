using UnityEngine;
using UnityEngine.AI;



public class HeavyWreckPart : MonoBehaviour
{
    [SerializeField, Range(0.05f, 1f)] private float slowFactor = 0.3f;
    [SerializeField] private float releaseDelay = 0.25f;  

    private NavMeshAgent slowedAgent;
    private float baseSpeed;
    private float lastContact;

    public void SetSlowFactor(float value) => slowFactor = Mathf.Clamp(value, 0.05f, 1f);

    private void OnCollisionStay(Collision collision)
    {
        var agent = collision.collider.GetComponentInParent<NavMeshAgent>();
        if (agent == null || !agent.enabled) return;
        if (collision.collider.GetComponentInParent<BossController>() == null) return;

        if (slowedAgent != agent)
        {
            Restore();
            slowedAgent = agent;
            baseSpeed = agent.speed;
        }

        agent.speed = baseSpeed * slowFactor;
        lastContact = Time.time;
    }

    private void Update()
    {
        if (slowedAgent != null && Time.time - lastContact > releaseDelay)
            Restore();
    }

    private void Restore()
    {
        if (slowedAgent != null) slowedAgent.speed = baseSpeed;
        slowedAgent = null;
    }

    private void OnDisable() => Restore();
    private void OnDestroy() => Restore();
}