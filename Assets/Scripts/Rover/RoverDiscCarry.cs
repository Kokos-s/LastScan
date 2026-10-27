using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RoverDiscCarry : MonoBehaviour
{
    [SerializeField] private Transform holdPoint;
    [SerializeField] private float pickupRadius = 8f;
    [SerializeField] private float pickupTime = 0.3f;

    [Header("Бросок камня")]
    [SerializeField] private float rockThrowSpeed = 25f;
    [SerializeField] private float rockMinFlightTime = 0.7f;
    [SerializeField] private float rockThrowDistance = 60f;   // на какое расстояние летит камень, если босса нет в прицеле

    [Header("Прицеливание")]
    [SerializeField] private float aimAssistAngle = 12f;      // половина конуса помощи прицела (градусы)
    [SerializeField] private float aimYawOffset = 0f;         // если «вперёд» у модели ровера смотрит не туда: 90, -90 или 180

    private RoverHealth health;
    private RobotController controller;
    private BossDisc heldDisc;
    private RockProjectile heldRock;
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

        if (heldDisc != null) ThrowDisc();
        else if (heldRock != null) ThrowRock();
        else TryPickup();
    }

    private Vector3 AimForward()
    {
        Vector3 f = Quaternion.Euler(0f, aimYawOffset, 0f) * transform.forward;
        f.y = 0f;
        return f.sqrMagnitude < 0.001f ? Vector3.forward : f.normalized;
    }

    // Возвращает точку босса, если он в конусе прицела
    private bool TryGetAssistedTarget(Vector3 from, out Vector3 point)
    {
        point = Vector3.zero;
        var boss = GameObject.FindGameObjectWithTag("Boss");
        if (boss == null) return false;

        var c = boss.GetComponent<Collider>();
        point = c != null ? c.bounds.center : boss.transform.position;

        Vector3 toBoss = point - from;
        toBoss.y = 0f;
        if (toBoss.sqrMagnitude < 0.01f) return true;

        return Vector3.Angle(AimForward(), toBoss) <= aimAssistAngle;
    }

    private void TryPickup()
    {
        float bestDist = pickupRadius * pickupRadius;
        BossDisc bestDisc = null;
        RockProjectile bestRock = null;

        foreach (var d in BossDisc.All)
        {
            if (!d.IsLying) continue;
            float dist = (d.transform.position - transform.position).sqrMagnitude;
            if (dist < bestDist) { bestDist = dist; bestDisc = d; bestRock = null; }
        }

        foreach (var r in RockProjectile.All)
        {
            if (!r.IsAvailable) continue;
            float dist = (r.transform.position - transform.position).sqrMagnitude;
            if (dist < bestDist) { bestDist = dist; bestRock = r; bestDisc = null; }
        }

        if (bestDisc != null) StartCoroutine(PickupDisc(bestDisc));
        else if (bestRock != null) StartCoroutine(PickupRock(bestRock));
    }

    private IEnumerator PickupDisc(BossDisc disc)
    {
        picking = true;
        disc.Hold();
        heldDisc = disc;

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

    private IEnumerator PickupRock(RockProjectile rock)
    {
        picking = true;
        rock.Grab();
        heldRock = rock;

        Vector3 from = rock.transform.position;
        float t = 0f;
        while (t < pickupTime)
        {
            t += Time.deltaTime;
            rock.transform.position = Vector3.Lerp(from, holdPoint.position, t / pickupTime);
            yield return null;
        }
        rock.transform.SetParent(holdPoint);
        rock.transform.localPosition = Vector3.zero;
        picking = false;
    }

    private void ThrowDisc()
    {
        Vector3 from = heldDisc.transform.position;
        Vector3 dir = TryGetAssistedTarget(from, out Vector3 point)
            ? point - from
            : AimForward();

        heldDisc.LaunchByPlayer(dir);
        heldDisc = null;
    }

    private void ThrowRock()
    {
        Vector3 start = heldRock.transform.position;

        Vector3 target = TryGetAssistedTarget(start, out Vector3 point)
            ? point
            : start + AimForward() * rockThrowDistance;

        float flight = Mathf.Max(rockMinFlightTime, Vector3.Distance(start, target) / rockThrowSpeed);
        Vector3 velocity = (target - start - 0.5f * Physics.gravity * flight * flight) / flight;

        heldRock.ReleaseByPlayer(velocity);
        heldRock = null;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 p = holdPoint != null ? holdPoint.position : transform.position;
        Vector3 f = AimForward();

        Gizmos.color = Color.green;
        Gizmos.DrawLine(p, p + f * 40f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(p, p + Quaternion.Euler(0f, aimAssistAngle, 0f) * f * 40f);
        Gizmos.DrawLine(p, p + Quaternion.Euler(0f, -aimAssistAngle, 0f) * f * 40f);
    }
}