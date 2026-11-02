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
    [SerializeField] private float rockThrowDistance = 60f;

    [Header("Прицеливание")]
    [SerializeField] private float aimAssistAngle = 12f;
    [SerializeField] private float aimYawOffset = 0f;

    [Header("Подсветка предметов, которые можно взять")]
    [SerializeField] private bool highlightEnabled = true;
    [SerializeField] private Color highlightColor = new Color(1f, 0.6f, 0.15f);
    [SerializeField] private float highlightIntensity = 2.5f;
    [SerializeField] private float highlightPulseSpeed = 5f;

    private RoverHealth health;
    private RobotController controller;
    private BossDisc heldDisc;
    private RockProjectile heldRock;
    private bool picking;
    private PickupHighlight currentHighlight;

    private void Awake()
    {
        health = GetComponent<RoverHealth>();
        controller = GetComponent<RobotController>();
    }

    private void OnDisable() => SetHighlight(null);

    private void Update()
    {
        if (health != null && health.IsDead) { SetHighlight(null); return; }

        UpdateHighlight();

        if (controller != null && controller.IsBusy) return;
        if (picking) return;
        if (Mouse.current == null || !Mouse.current.rightButton.wasPressedThisFrame) return;

        if (heldDisc != null) ThrowDisc();
        else if (heldRock != null) ThrowRock();
        else TryPickup();
    }

    // ---------------- ПОДСВЕТКА ----------------
    private void UpdateHighlight()
    {
        if (!highlightEnabled || picking || heldDisc != null || heldRock != null)
        {
            SetHighlight(null);
            return;
        }

        FindNearestPickup(out BossDisc disc, out RockProjectile rock);

        GameObject target = disc != null ? disc.gameObject : (rock != null ? rock.gameObject : null);
        if (target == null)
        {
            SetHighlight(null);
            return;
        }

        var hl = target.GetComponent<PickupHighlight>();
        if (hl == null) hl = target.AddComponent<PickupHighlight>();
        hl.Configure(highlightColor, highlightIntensity, highlightPulseSpeed);
        SetHighlight(hl);
    }

    private void SetHighlight(PickupHighlight next)
    {
        if (next == currentHighlight) return;
        if (currentHighlight != null) currentHighlight.SetHighlighted(false);
        currentHighlight = next;
        if (currentHighlight != null) currentHighlight.SetHighlighted(true);
    }

    private void FindNearestPickup(out BossDisc bestDisc, out RockProjectile bestRock)
    {
        float bestDist = pickupRadius * pickupRadius;
        bestDisc = null;
        bestRock = null;

        foreach (var d in BossDisc.All)
        {
            if (d == null || !d.IsLying) continue;
            float dist = (d.transform.position - transform.position).sqrMagnitude;
            if (dist < bestDist) { bestDist = dist; bestDisc = d; bestRock = null; }
        }

        foreach (var r in RockProjectile.All)
        {
            if (r == null || !r.IsAvailable) continue;
            float dist = (r.transform.position - transform.position).sqrMagnitude;
            if (dist < bestDist) { bestDist = dist; bestRock = r; bestDisc = null; }
        }
    }

    // ---------------- ПРИЦЕЛ ----------------
    private Vector3 AimForward()
    {
        Vector3 f = Quaternion.Euler(0f, aimYawOffset, 0f) * transform.forward;
        f.y = 0f;
        return f.sqrMagnitude < 0.001f ? Vector3.forward : f.normalized;
    }

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

    // ---------------- ПОДБОР ----------------
    private void TryPickup()
    {
        FindNearestPickup(out BossDisc disc, out RockProjectile rock);

        SetHighlight(null);

        if (disc != null) StartCoroutine(PickupDisc(disc));
        else if (rock != null) StartCoroutine(PickupRock(rock));
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

    // ---------------- БРОСОК ----------------
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

        Gizmos.color = new Color(1f, 0.6f, 0.15f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}