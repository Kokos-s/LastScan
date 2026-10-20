using UnityEngine;
using UnityEngine.InputSystem;

public class RoverDiscCarry : MonoBehaviour
{
    [SerializeField] private Transform holdPoint;          // точка в руке/перед ровером
    [SerializeField] private float pickupRadius = 8f;
    [SerializeField] private float pickupTime = 0.3f;

    private RoverHealth health;
    private RobotController controller;
    private BossDisc held;
    private bool picking;

    private void Awake()
    {
        health = GetComponent<RoverHealth>();
        controller = GetComponent<RobotController>();
    }

    private void Update()
    {
        if (health != null && health.IsDead) return;
        if (controller != null && controller.IsBusy) return;
        if (picking) return;
        if (Mouse.current == null || !Mouse.current.rightButton.wasPressedThisFrame) return;

        if (held != null) Throw();
        else TryPickup();
    }

    private void TryPickup()
    {
        BossDisc best = null;
        float bestDist = pickupRadius * pickupRadius;
        foreach (var d in BossDisc.All)
        {
            if (!d.IsLying) continue;
            float dist = (d.transform.position - transform.position).sqrMagnitude;
            if (dist < bestDist) { bestDist = dist; best = d; }
        }
        if (best != null) StartCoroutine(PickupRoutine(best));
    }

    private System.Collections.IEnumerator PickupRoutine(BossDisc disc)
    {
        picking = true;
        disc.Hold();
        held = disc;

        Vector3 from = disc.transform.position;
        float t = 0f;
        while (t < pickupTime)
        {
            t += Time.deltaTime;
            disc.transform.position = Vector3.Lerp(from, holdPoint.position, t / pickupTime);
            yield return null;
        }
        disc.transform.SetParent(holdPoint);
        disc.transform.localPosition = Vector3.zero;
        picking = false;
    }

    private void Throw()
    {
        var boss = GameObject.FindGameObjectWithTag("Boss");
        Vector3 dir = boss != null
            ? boss.transform.position - held.transform.position
            : transform.forward;

        held.LaunchByPlayer(dir);
        held = null;
    }
}