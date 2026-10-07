using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BossController : MonoBehaviour
{
    public enum State { Chase, DrillAttack, RockAttack, Recover, Dead }

    [SerializeField] private Transform player;
    [SerializeField] private Transform drillPivot;
    [SerializeField] private Transform drillModel;
    [SerializeField] private Renderer drillRenderer;
    [SerializeField] private DrillHitbox drillHitbox;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Transform armJoint;
    [SerializeField] private Transform elbowJoint;

    [SerializeField] private float drillRange = 7f;
    [SerializeField] private float windupTime = 0.8f;
    [SerializeField] private float strikeTime = 0.35f;
    [SerializeField] private float recoverTime = 1.3f;
    [SerializeField] private float lungeDistance = 3f;
    [SerializeField] private float turnSpeed = 8f;

    [SerializeField] private Vector3 armAxis = Vector3.up;
    [SerializeField] private float armWindupAngle = -35f;
    [SerializeField] private float armStrikeAngle = 60f;

    [SerializeField] private Vector3 elbowAxis = Vector3.up;
    [SerializeField] private float elbowWindupAngle = 15f;
    [SerializeField] private float elbowStrikeAngle = -40f;

    [SerializeField] private float idleSpin = 200f;
    [SerializeField] private float windupSpin = 700f;
    [SerializeField] private float strikeSpin = 1500f;
    [SerializeField] private float pullBackOffset = 0.6f;
    [SerializeField] private Color telegraphColor = Color.red;

    [SerializeField] private float rockMinRange = 12f;
    [SerializeField] private float rockCooldown = 4f;
    [SerializeField] private float pickupDistance = 4f;
    [SerializeField] private float rockStopDistance = 2.5f;
    [SerializeField] private float rockApproachTimeout = 12f;
    [SerializeField] private float pickupTime = 0.5f;
    [SerializeField] private float throwWindup = 1.0f;
    [SerializeField] private float throwSpeed = 14f;
    [SerializeField] private float leadFactor = 0.7f;
    [SerializeField] private float rockRecover = 1.0f;
    [SerializeField] private float raiseHeight = 0.6f;

    private NavMeshAgent agent;
    private State state;
    private float currentSpin;
    private Color baseColor;
    private Vector3 pivotRestPos;
    private Vector3 holdRestPos;
    private Quaternion armRestRot;
    private Quaternion elbowRestRot;
    private Health health;
    private Rigidbody playerBody;
    private RockProjectile heldRock;
    private float nextRockTime;
    private float defaultStopDistance;

    public State CurrentState => state;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        playerBody = player.GetComponent<Rigidbody>();
        pivotRestPos = drillPivot.localPosition;
        holdRestPos = holdPoint.localPosition;
        armRestRot = armJoint.localRotation;
        elbowRestRot = elbowJoint.localRotation;
        baseColor = drillRenderer.material.color;
        defaultStopDistance = agent.stoppingDistance;
        currentSpin = idleSpin;
        drillHitbox.gameObject.SetActive(false);
    }

    private void OnEnable() { if (health) health.onDeath.AddListener(OnDeath); }
    private void OnDisable() { if (health) health.onDeath.RemoveListener(OnDeath); }

    private void Start()
    {
        state = State.Chase;
        nextRockTime = Time.time + 2f;
    }

    private void Update()
    {
        drillModel.Rotate(0f, 0f, currentSpin * Time.deltaTime, Space.Self);

        if (state == State.Chase) UpdateChase();
    }

    private void UpdateChase()
    {
        agent.SetDestination(player.position);

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= drillRange)
            StartCoroutine(DrillAttackRoutine());
        else if (dist >= rockMinRange && Time.time >= nextRockTime
                 && RockProjectile.FindNearest(transform.position) != null)
            StartCoroutine(RockAttackRoutine());
    }

    private void SetArm(float shoulderAngle, float elbowAngle)
    {
        armJoint.localRotation = armRestRot * Quaternion.AngleAxis(shoulderAngle, armAxis);
        elbowJoint.localRotation = elbowRestRot * Quaternion.AngleAxis(elbowAngle, elbowAxis);
    }

    private IEnumerator DrillAttackRoutine()
    {
        state = State.DrillAttack;
        agent.ResetPath();
        agent.isStopped = true;

        float t = 0f;
        while (t < windupTime)
        {
            t += Time.deltaTime;
            float k = t / windupTime;

            FaceTarget(turnSpeed);
            currentSpin = Mathf.Lerp(idleSpin, windupSpin, k);
            drillRenderer.material.color = Color.Lerp(baseColor, telegraphColor, k);
            drillPivot.localPosition = pivotRestPos + Vector3.back * (pullBackOffset * k);
            SetArm(armWindupAngle * k, elbowWindupAngle * k);
            yield return null;
        }

        drillHitbox.gameObject.SetActive(true);
        currentSpin = strikeSpin;

        Vector3 lungeDir = transform.forward;
        float lungeSpeed = lungeDistance / strikeTime;
        t = 0f;
        while (t < strikeTime)
        {
            t += Time.deltaTime;
            agent.Move(lungeDir * (lungeSpeed * Time.deltaTime));
            float k = t / strikeTime;
            SetArm(Mathf.Lerp(armWindupAngle, armStrikeAngle, k),
                   Mathf.Lerp(elbowWindupAngle, elbowStrikeAngle, k));
            yield return null;
        }
        drillHitbox.gameObject.SetActive(false);

        state = State.Recover;
        currentSpin = idleSpin;
        drillRenderer.material.color = baseColor;
        drillPivot.localPosition = pivotRestPos;

        Quaternion armFrom = armJoint.localRotation;
        Quaternion elbowFrom = elbowJoint.localRotation;
        float back = 0.4f;
        t = 0f;
        while (t < back)
        {
            t += Time.deltaTime;
            float k = t / back;
            armJoint.localRotation = Quaternion.Slerp(armFrom, armRestRot, k);
            elbowJoint.localRotation = Quaternion.Slerp(elbowFrom, elbowRestRot, k);
            yield return null;
        }
        yield return new WaitForSeconds(Mathf.Max(0f, recoverTime - back));

        agent.isStopped = false;
        state = State.Chase;
    }

    private IEnumerator RockAttackRoutine()
    {
        state = State.RockAttack;
        RockProjectile rock = RockProjectile.FindNearest(transform.position);
        if (rock == null) { state = State.Chase; yield break; }

        agent.stoppingDistance = rockStopDistance;
        agent.isStopped = false;
        bool reached = false;
        float timeout = rockApproachTimeout;
        while (timeout > 0f && rock != null && rock.IsAvailable)
        {
            if (Vector3.Distance(transform.position, player.position) <= drillRange)
            {
                agent.stoppingDistance = defaultStopDistance;
                nextRockTime = Time.time + 1f;
                state = State.Chase;
                yield break;
            }

            agent.SetDestination(rock.transform.position);
            if (Vector3.Distance(transform.position, rock.transform.position) <= pickupDistance)
            {
                reached = true;
                break;
            }
            timeout -= Time.deltaTime;
            yield return null;
        }

        agent.stoppingDistance = defaultStopDistance;

        if (!reached)
        {
            nextRockTime = Time.time + 2f;
            state = State.Chase;
            yield break;
        }

        agent.ResetPath();
        agent.isStopped = true;

        heldRock = rock;
        rock.Grab();
        Vector3 startPos = rock.transform.position;
        float t = 0f;
        while (t < pickupTime)
        {
            t += Time.deltaTime;
            FacePoint(startPos, turnSpeed);
            rock.transform.position = Vector3.Lerp(startPos, holdPoint.position, t / pickupTime);
            yield return null;
        }
        rock.transform.SetParent(holdPoint);
        rock.transform.localPosition = Vector3.zero;

        t = 0f;
        while (t < throwWindup)
        {
            t += Time.deltaTime;
            FaceTarget(turnSpeed);
            holdPoint.localPosition = holdRestPos + Vector3.up * (raiseHeight * (t / throwWindup));
            yield return null;
        }

        Vector3 start = rock.transform.position;
        float flight = Mathf.Clamp(Vector3.Distance(start, player.position) / throwSpeed, 0.4f, 1.6f);
        Vector3 target = player.position;
        if (playerBody != null)
            target += playerBody.linearVelocity * flight * leadFactor;

        Vector3 velocity = (target - start - 0.5f * Physics.gravity * flight * flight) / flight;
        rock.Release(velocity);
        heldRock = null;
        holdPoint.localPosition = holdRestPos;

        state = State.Recover;
        yield return new WaitForSeconds(rockRecover);

        nextRockTime = Time.time + rockCooldown;
        agent.isStopped = false;
        state = State.Chase;
    }

    private void FaceTarget(float speed) => FacePoint(player.position, speed);

    private void FacePoint(Vector3 point, float speed)
    {
        Vector3 dir = point - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        transform.rotation = Quaternion.Slerp(
            transform.rotation, Quaternion.LookRotation(dir), speed * Time.deltaTime);
    }

    private void OnDeath()
    {
        StopAllCoroutines();
        state = State.Dead;
        drillHitbox.gameObject.SetActive(false);
        agent.stoppingDistance = defaultStopDistance;
        if (agent.isOnNavMesh) agent.isStopped = true;
        currentSpin = 0f;

        if (heldRock != null)
        {
            heldRock.Release(Vector3.zero);
            heldRock = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, drillRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rockMinRange);
    }
}