using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BossController : MonoBehaviour
{
    public enum State { Chase, DrillAttack, RockAttack, DiscAttack, Recover, Dead }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform drillPivot;
    [SerializeField] private Transform drillModel;
    [SerializeField] private Renderer drillRenderer;
    [SerializeField] private DrillHitbox drillHitbox;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Transform armJoint;     // плечо руки с буром
    [SerializeField] private Transform elbowJoint;   // локоть руки с буром

    [Header("Drill Attack")]
    [SerializeField] private float drillRange = 7f;
    [SerializeField] private float windupTime = 0.8f;
    [SerializeField] private float strikeTime = 0.35f;
    [SerializeField] private float recoverTime = 1.3f;
    [SerializeField] private float lungeDistance = 3f;
    [SerializeField] private float turnSpeed = 8f;

    [Header("Arm (плечо бура)")]
    [SerializeField] private Vector3 armAxis = Vector3.up;
    [SerializeField] private float armWindupAngle = -35f;
    [SerializeField] private float armStrikeAngle = 60f;

    [Header("Elbow (локоть бура)")]
    [SerializeField] private Vector3 elbowAxis = Vector3.up;
    [SerializeField] private float elbowWindupAngle = 15f;
    [SerializeField] private float elbowStrikeAngle = -40f;

    [Header("Drill Visuals")]
    [SerializeField] private float idleSpin = 200f;
    [SerializeField] private float windupSpin = 700f;
    [SerializeField] private float strikeSpin = 1500f;
    [SerializeField] private float pullBackOffset = 0.6f;
    [SerializeField] private Color telegraphColor = Color.red;

    [Header("Rock Attack")]
    [SerializeField] private float rockMinRange = 12f;
    [SerializeField] private float rockCooldown = 4f;
    [SerializeField] private float pickupDistance = 12f;
    [SerializeField] private float rockStopDistance = 8f;
    [SerializeField] private float rockApproachTimeout = 20f;
    [SerializeField] private float pickupTime = 0.5f;
    [SerializeField] private float throwWindup = 1.0f;
    [SerializeField] private float throwSpeed = 14f;
    [SerializeField] private float leadFactor = 0.7f;
    [SerializeField] private float rockRecover = 1.0f;
    [SerializeField] private float raiseHeight = 0.6f;

    [Header("Disc Attack (диски)")]
    [SerializeField] private BossDisc[] discs;
    [SerializeField] private Transform discStorage;
    [SerializeField] private Transform discHoldPoint;        // точка в кисти бросающей руки
    [SerializeField] private float discMaxRange = 40f;
    [SerializeField] private int discsPerAttack = 2;
    [SerializeField] private float discPickupTime = 0.5f;
    [SerializeField] private float discWindup = 0.7f;
    [SerializeField] private float discInterval = 0.4f;
    [SerializeField] private float discSpread = 25f;
    [SerializeField] private float discRecover = 1.2f;
    [SerializeField] private float discCooldown = 5f;

    [Header("Throw Arm (рука, бросающая диски)")]
    [SerializeField] private Transform throwShoulder;
    [SerializeField] private Transform throwElbow;           // можно оставить пустым
    [SerializeField] private Vector3 throwShoulderAxis = Vector3.up;
    [SerializeField] private Vector3 throwElbowAxis = Vector3.up;
    [SerializeField] private float throwShoulderBack = -70f;   // замах назад
    [SerializeField] private float throwShoulderForward = 50f; // конец броска
    [SerializeField] private float throwElbowBack = 60f;       // локоть согнут при замахе
    [SerializeField] private float throwElbowForward = -20f;   // локоть разогнут при броске
    [SerializeField] private float throwSwingTime = 0.18f;     // скорость самого взмаха
    [SerializeField, Range(0.1f, 1f)] private float releasePoint = 0.6f; // в какой момент взмаха диск вылетает
    [SerializeField] private float throwReturnTime = 0.3f;

    private NavMeshAgent agent;
    private State state;
    private float currentSpin;
    private Color baseColor;
    private Vector3 pivotRestPos;
    private Vector3 holdRestPos;
    private Quaternion armRestRot;
    private Quaternion elbowRestRot;
    private Quaternion throwShoulderRest;
    private Quaternion throwElbowRest;
    private Health health;
    private Rigidbody playerBody;
    private RockProjectile heldRock;
    private BossDisc heldDisc;
    private float nextRockTime;
    private float nextDiscTime;
    private float nextCollectTime;
    private float defaultStopDistance;

    public State CurrentState => state;

    private Transform DiscHand => discHoldPoint != null ? discHoldPoint : holdPoint;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        playerBody = player.GetComponent<Rigidbody>();
        pivotRestPos = drillPivot.localPosition;
        holdRestPos = holdPoint.localPosition;
        armRestRot = armJoint.localRotation;
        elbowRestRot = elbowJoint.localRotation;
        if (throwShoulder != null) throwShoulderRest = throwShoulder.localRotation;
        if (throwElbow != null) throwElbowRest = throwElbow.localRotation;
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
        nextDiscTime = Time.time + 3f;
        nextCollectTime = Time.time + 3f;
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
        {
            StartCoroutine(DrillAttackRoutine());
            return;
        }

        bool canDisc = StoredDiscCount() > 0 && discStorage != null
                       && dist <= discMaxRange && Time.time >= nextDiscTime;
        bool canRock = dist >= rockMinRange && Time.time >= nextRockTime
                       && RockProjectile.FindNearest(transform.position) != null;
        bool canCollect = StoredDiscCount() == 0 && Time.time >= nextCollectTime
                          && FindNearestLyingDisc() != null;

        if (canCollect)
        {
            StartCoroutine(DiscCollectRoutine());
        }
        else if (canDisc && canRock)
        {
            if (Random.value < 0.5f) StartCoroutine(DiscAttackRoutine());
            else StartCoroutine(RockAttackRoutine());
        }
        else if (canDisc) StartCoroutine(DiscAttackRoutine());
        else if (canRock) StartCoroutine(RockAttackRoutine());
    }

    private void SetArm(float shoulderAngle, float elbowAngle)
    {
        armJoint.localRotation = armRestRot * Quaternion.AngleAxis(shoulderAngle, armAxis);
        elbowJoint.localRotation = elbowRestRot * Quaternion.AngleAxis(elbowAngle, elbowAxis);
    }

    private void SetThrowArm(float shoulderAngle, float elbowAngle)
    {
        if (throwShoulder != null)
            throwShoulder.localRotation = throwShoulderRest * Quaternion.AngleAxis(shoulderAngle, throwShoulderAxis);
        if (throwElbow != null)
            throwElbow.localRotation = throwElbowRest * Quaternion.AngleAxis(elbowAngle, throwElbowAxis);
    }

    // ---------------- БУР ----------------
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

    // ---------------- КАМНИ ----------------
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

    // ---------------- ДИСКИ ----------------
    private int StoredDiscCount()
    {
        int n = 0;
        foreach (var d in discs) if (d != null && d.IsStored) n++;
        return n;
    }

    private BossDisc GetStoredDisc()
    {
        foreach (var d in discs) if (d != null && d.IsStored) return d;
        return null;
    }

    private BossDisc FindNearestLyingDisc()
    {
        BossDisc best = null;
        float bestDist = float.MaxValue;
        foreach (var d in discs)
        {
            if (d == null || !d.IsLying) continue;
            float dist = (d.transform.position - transform.position).sqrMagnitude;
            if (dist < bestDist) { bestDist = dist; best = d; }
        }
        return best;
    }

    private IEnumerator DiscAttackRoutine()
    {
        state = State.DiscAttack;
        agent.ResetPath();
        agent.isStopped = true;

        for (int i = 0; i < discsPerAttack; i++)
        {
            BossDisc disc = GetStoredDisc();
            if (disc == null) break;

            // 1. Достаём диск из отсека в кисть
            disc.Hold();
            heldDisc = disc;

            Vector3 from = disc.transform.position;
            float t = 0f;
            while (t < discPickupTime)
            {
                t += Time.deltaTime;
                FaceTarget(turnSpeed);
                disc.transform.position = Vector3.Lerp(from, DiscHand.position, t / discPickupTime);
                yield return null;
            }
            disc.transform.SetParent(DiscHand);
            disc.transform.localPosition = Vector3.zero;

            // 2. Замах: плечо уходит назад, локоть сгибается
            t = 0f;
            while (t < discWindup)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / discWindup);
                FaceTarget(turnSpeed);
                SetThrowArm(throwShoulderBack * k, throwElbowBack * k);
                yield return null;
            }

            // 3. Взмах: диск вылетает в момент releasePoint
            bool released = false;
            t = 0f;
            while (t < throwSwingTime)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / throwSwingTime);
                SetThrowArm(Mathf.Lerp(throwShoulderBack, throwShoulderForward, k),
                            Mathf.Lerp(throwElbowBack, throwElbowForward, k));

                if (!released && k >= releasePoint)
                {
                    ReleaseDisc(disc, i);
                    released = true;
                }
                yield return null;
            }
            if (!released) ReleaseDisc(disc, i);

            // 4. Рука возвращается
            t = 0f;
            while (t < throwReturnTime)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / throwReturnTime);
                SetThrowArm(Mathf.Lerp(throwShoulderForward, 0f, k),
                            Mathf.Lerp(throwElbowForward, 0f, k));
                yield return null;
            }

            yield return new WaitForSeconds(discInterval);
        }

        SetThrowArm(0f, 0f);

        state = State.Recover;
        yield return new WaitForSeconds(discRecover);

        nextDiscTime = Time.time + discCooldown;
        agent.isStopped = false;
        state = State.Chase;
    }

    private void ReleaseDisc(BossDisc disc, int index)
    {
        Vector3 dir = player.position - disc.transform.position;
        dir.y = 0f;
        float angle = (index == 0) ? 0f : Random.Range(-discSpread, discSpread);
        dir = Quaternion.Euler(0f, angle, 0f) * dir;

        disc.Launch(dir);
        heldDisc = null;
    }

    private IEnumerator DiscCollectRoutine()
    {
        state = State.DiscAttack;
        BossDisc disc = FindNearestLyingDisc();
        if (disc == null) { nextCollectTime = Time.time + 3f; state = State.Chase; yield break; }

        agent.stoppingDistance = rockStopDistance;
        agent.isStopped = false;
        bool reached = false;
        float timeout = rockApproachTimeout;
        while (timeout > 0f && disc.IsLying)
        {
            if (Vector3.Distance(transform.position, player.position) <= drillRange)
            {
                agent.stoppingDistance = defaultStopDistance;
                nextCollectTime = Time.time + 2f;
                state = State.Chase;
                yield break;
            }

            agent.SetDestination(disc.transform.position);
            if (Vector3.Distance(transform.position, disc.transform.position) <= pickupDistance)
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
            nextCollectTime = Time.time + 3f;
            state = State.Chase;
            yield break;
        }

        agent.ResetPath();
        agent.isStopped = true;

        disc.Hold();
        Vector3 from = disc.transform.position;
        float t = 0f;
        while (t < discPickupTime)
        {
            t += Time.deltaTime;
            FacePoint(from, turnSpeed);
            disc.transform.position = Vector3.Lerp(from, discStorage.position, t / discPickupTime);
            yield return null;
        }
        disc.ReturnHome();

        yield return new WaitForSeconds(0.5f);

        nextDiscTime = Time.time + 1f;
        agent.isStopped = false;
        state = State.Chase;
    }

    // ---------------- ОБЩЕЕ ----------------
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
        SetThrowArm(0f, 0f);

        if (heldRock != null)
        {
            heldRock.Release(Vector3.zero);
            heldRock = null;
        }
        if (heldDisc != null)
        {
            heldDisc.Lie();
            heldDisc = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, drillRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rockMinRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, discMaxRange);
    }
}