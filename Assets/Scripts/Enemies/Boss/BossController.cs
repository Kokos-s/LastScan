using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BossController : MonoBehaviour
{
    public enum State { Chase, DrillAttack, RockAttack, DiscAttack, Recover, Dead }

    [Header("References")]
    [SerializeField] private Transform player;       // можно оставить пустым: ищется по тегу Player
    [SerializeField] private Transform drillPivot;
    [SerializeField] private Transform drillModel;
    [SerializeField] private Renderer drillRenderer;
    [SerializeField] private DrillHitbox drillHitbox;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Transform armJoint;     // плечо руки с буром
    [SerializeField] private Transform elbowJoint;   // локоть руки с буром

    [Header("Phases (фазы по здоровью)")]
    [SerializeField, Range(0.05f, 0.95f)] private float rangedPhaseThreshold = 0.5f; // ниже этого здоровья босс предпочитает дальние атаки
    [SerializeField] private float phase1RangedChance = 0.2f;       // шанс дальней атаки в фазе 1 (проверка раз в 3 с)
    [SerializeField] private float phase1CooldownMultiplier = 2f;   // кулдауны дальних атак в фазе 1
    [SerializeField] private float phase2CooldownMultiplier = 0.6f; // кулдауны дальних атак в фазе 2
    [SerializeField] private float phase2KeepDistance = 20f;        // на каком расстоянии босс останавливается в фазе 2

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
    [SerializeField] private float rockMinRange = 12f;        // только в фазе 1
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
    [SerializeField] private float rockMinFlightTime = 1.2f;
    [SerializeField] private float rockMaxFlightTime = 2.5f;
    [SerializeField] private float stuckCheckTime = 1.5f;
    [SerializeField] private float stuckMinMove = 0.5f;
    [SerializeField] private float rockBlacklistTime = 15f;

    [Header("Disc Attack (диски)")]
    [SerializeField] private BossDisc[] discs;
    [SerializeField] private Transform discStorage;
    [SerializeField] private Transform discHoldPoint;
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
    [SerializeField] private Transform throwElbow;
    [SerializeField] private Vector3 throwShoulderAxis = Vector3.up;
    [SerializeField] private Vector3 throwElbowAxis = Vector3.up;
    [SerializeField] private float throwShoulderBack = -70f;
    [SerializeField] private float throwShoulderForward = 50f;
    [SerializeField] private float throwElbowBack = 60f;
    [SerializeField] private float throwElbowForward = -20f;
    [SerializeField] private float throwSwingTime = 0.18f;
    [SerializeField, Range(0.1f, 1f)] private float releasePoint = 0.6f;
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
    private float nextPhase1Roll;
    private float nextRockSearch;
    private RockProjectile cachedRock;
    private float defaultStopDistance;
    private bool started = false;
    private bool warnedNoPlayer = false;
    private bool wasRangedPhase = false;
    private readonly Dictionary<RockProjectile, float> rockBlacklist = new Dictionary<RockProjectile, float>();

    public State CurrentState => state;

    private Transform DiscHand => discHoldPoint != null ? discHoldPoint : holdPoint;

    // Фаза 2: здоровье ниже порога
    private bool IsRangedPhase => health != null && health.Normalized <= rangedPhaseThreshold;
    private float CooldownMul => IsRangedPhase ? phase2CooldownMultiplier : phase1CooldownMultiplier;

    // Прервать дальнюю атаку ради бура можно только в фазе 1
    private bool ShouldAbortForDrill(float dist) => !IsRangedPhase && dist <= drillRange;

    private bool ResolvePlayer()
    {
        if (player == null)
        {
            GameObject roverObject = GameObject.FindGameObjectWithTag("Player");
            if (roverObject != null)
                player = roverObject.transform;
        }

        if (player == null)
        {
            if (!warnedNoPlayer)
            {
                Debug.LogWarning("BossController: объект с тегом Player не найден. Босс ждёт появления ровера.");
                warnedNoPlayer = true;
            }
            return false;
        }

        if (playerBody == null)
            playerBody = player.GetComponentInParent<Rigidbody>();

        warnedNoPlayer = false;
        return true;
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        ResolvePlayer();
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

    private void OnEnable()
    {
        if (health) health.onDeath.AddListener(OnDeath);

        if (started && state != State.Chase && state != State.Dead)
            ResetAfterInterrupt();
    }

    private void OnDisable()
    {
        if (health) health.onDeath.RemoveListener(OnDeath);
    }

    private void Start()
    {
        state = State.Chase;
        started = true;
        nextRockTime = Time.time + 2f;
        nextDiscTime = Time.time + 3f;
        nextCollectTime = Time.time + 3f;
        nextPhase1Roll = Time.time + 3f;
    }

    private void Update()
    {
        drillModel.Rotate(0f, 0f, currentSpin * Time.deltaTime, Space.Self);

        if (!ResolvePlayer()) return;

        // смена фазы: сбрасываем кулдауны, чтобы босс сразу перешёл на дальние атаки
        bool ranged = IsRangedPhase;
        if (ranged != wasRangedPhase)
        {
            wasRangedPhase = ranged;
            if (ranged)
            {
                nextRockTime = Time.time;
                nextDiscTime = Time.time;
                nextCollectTime = Time.time;
                nextRockSearch = 0f;
            }
        }

        if (state == State.Chase) UpdateChase();
    }

    private void ResetAfterInterrupt()
    {
        StopAllCoroutines();

        drillHitbox.gameObject.SetActive(false);
        currentSpin = idleSpin;
        drillRenderer.material.color = baseColor;
        drillPivot.localPosition = pivotRestPos;
        holdPoint.localPosition = holdRestPos;
        armJoint.localRotation = armRestRot;
        elbowJoint.localRotation = elbowRestRot;
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

        agent.stoppingDistance = defaultStopDistance;
        if (agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = false;
        }

        state = State.Chase;
        nextRockTime = Time.time + 2f;
        nextDiscTime = Time.time + 2f;
        nextCollectTime = Time.time + 2f;
    }

    // Раз в 0.5 с ищем достижимый камень, между поисками используем результат
    private void RefreshRockCache(float dist)
    {
        float minRange = IsRangedPhase ? 0f : rockMinRange;

        if (dist < minRange)
        {
            cachedRock = null;
            return;
        }

        if (cachedRock != null && !cachedRock.IsAvailable) cachedRock = null;

        if (Time.time >= nextRockSearch)
        {
            nextRockSearch = Time.time + 0.5f;
            cachedRock = FindReachableRock();
        }
    }

    private void UpdateChase()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        bool phase2 = IsRangedPhase;

        // что есть из дальних атак (без учёта кулдаунов)
        bool discReady = StoredDiscCount() > 0 && discStorage != null && dist <= discMaxRange;
        bool discToCollect = StoredDiscCount() == 0 && FindNearestLyingDisc() != null;
        RefreshRockCache(dist);
        bool rockReady = cachedRock != null;
        bool hasRangedAccess = discReady || discToCollect || rockReady;

        // ---------- движение ----------
        agent.stoppingDistance = (phase2 && hasRangedAccess) ? phase2KeepDistance : defaultStopDistance;
        agent.SetDestination(player.position);

        // ---------- бур ----------
        bool drillAllowed = !phase2 || !hasRangedAccess;
        if (drillAllowed && dist <= drillRange)
        {
            agent.stoppingDistance = defaultStopDistance;
            StartCoroutine(DrillAttackRoutine());
            return;
        }

        // ---------- допуск к дальним атакам ----------
        bool tryRanged = phase2;
        if (!phase2 && Time.time >= nextPhase1Roll)
        {
            nextPhase1Roll = Time.time + 3f;
            tryRanged = Random.value < phase1RangedChance;
        }

        bool canCollect = discToCollect && Time.time >= nextCollectTime;
        if (canCollect && (phase2 || dist > drillRange))
        {
            StartCoroutine(DiscCollectRoutine());
            return;
        }

        if (!tryRanged) return;

        bool canDisc = discReady && Time.time >= nextDiscTime;
        bool canRock = rockReady && Time.time >= nextRockTime;

        if (canDisc && canRock)
        {
            if (Random.value < 0.5f) StartCoroutine(DiscAttackRoutine());
            else StartCoroutine(RockAttackRoutine(cachedRock));
        }
        else if (canDisc) StartCoroutine(DiscAttackRoutine());
        else if (canRock) StartCoroutine(RockAttackRoutine(cachedRock));
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
    private RockProjectile FindReachableRock()
    {
        RockProjectile best = null;
        float bestDist = float.MaxValue;
        var path = new NavMeshPath();

        foreach (var r in RockProjectile.All)
        {
            if (r == null || !r.IsAvailable) continue;
            if (rockBlacklist.TryGetValue(r, out float until) && Time.time < until) continue;

            float d = (r.transform.position - transform.position).sqrMagnitude;
            if (d >= bestDist) continue;

            if (!NavMesh.SamplePosition(r.transform.position, out NavMeshHit hit, 6f, NavMesh.AllAreas)) continue;
            if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete) continue;

            bestDist = d;
            best = r;
        }
        return best;
    }

    private void AbandonRock(RockProjectile rock)
    {
        if (rock != null) rockBlacklist[rock] = Time.time + rockBlacklistTime;
        cachedRock = null;
        agent.stoppingDistance = defaultStopDistance;
        nextRockTime = Time.time + 1f;
        agent.isStopped = false;
        state = State.Chase;
    }

    private IEnumerator RockAttackRoutine(RockProjectile rock)
    {
        state = State.RockAttack;
        if (rock == null) { state = State.Chase; yield break; }

        agent.stoppingDistance = rockStopDistance;
        agent.isStopped = false;
        bool reached = false;
        float timeout = rockApproachTimeout;
        float stuckTimer = stuckCheckTime;
        Vector3 lastPos = transform.position;
        float pickupDist = Mathf.Max(pickupDistance, rockStopDistance + 0.5f);

        while (timeout > 0f && rock != null && rock.IsAvailable)
        {
            if (ShouldAbortForDrill(Vector3.Distance(transform.position, player.position)))
            {
                agent.stoppingDistance = defaultStopDistance;
                nextRockTime = Time.time + 1f;
                state = State.Chase;
                yield break;
            }

            agent.SetDestination(rock.transform.position);
            if (Vector3.Distance(transform.position, rock.transform.position) <= pickupDist)
            {
                reached = true;
                break;
            }

            stuckTimer -= Time.deltaTime;
            if (stuckTimer <= 0f)
            {
                if ((transform.position - lastPos).magnitude < stuckMinMove)
                {
                    Debug.Log("Босс не может доехать до камня " + rock.name + ", бросаю его");
                    AbandonRock(rock);
                    yield break;
                }
                lastPos = transform.position;
                stuckTimer = stuckCheckTime;
            }

            timeout -= Time.deltaTime;
            yield return null;
        }

        agent.stoppingDistance = defaultStopDistance;

        if (!reached)
        {
            AbandonRock(rock);
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
        Vector3 aimPoint = player.position;
        var pc = player.GetComponentInChildren<Collider>();
        if (pc != null) aimPoint = pc.bounds.center;

        float flight = Mathf.Clamp(Vector3.Distance(start, aimPoint) / throwSpeed,
                                   rockMinFlightTime, rockMaxFlightTime);
        Vector3 target = aimPoint;
        if (playerBody != null)
            target += playerBody.linearVelocity * flight * leadFactor;

        Vector3 velocity = (target - start - 0.5f * Physics.gravity * flight * flight) / flight;
        rock.Release(velocity);
        heldRock = null;
        cachedRock = null;
        holdPoint.localPosition = holdRestPos;

        state = State.Recover;
        yield return new WaitForSeconds(rockRecover);

        nextRockTime = Time.time + rockCooldown * CooldownMul;
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

            t = 0f;
            while (t < discWindup)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / discWindup);
                FaceTarget(turnSpeed);
                SetThrowArm(throwShoulderBack * k, throwElbowBack * k);
                yield return null;
            }

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

        nextDiscTime = Time.time + discCooldown * CooldownMul;
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
        float stuckTimer = stuckCheckTime;
        Vector3 lastPos = transform.position;
        float pickupDist = Mathf.Max(pickupDistance, rockStopDistance + 0.5f);

        while (timeout > 0f && disc != null && disc.IsLying)
        {
            if (ShouldAbortForDrill(Vector3.Distance(transform.position, player.position)))
            {
                agent.stoppingDistance = defaultStopDistance;
                nextCollectTime = Time.time + 2f;
                state = State.Chase;
                yield break;
            }

            agent.SetDestination(disc.transform.position);
            if (Vector3.Distance(transform.position, disc.transform.position) <= pickupDist)
            {
                reached = true;
                break;
            }

            stuckTimer -= Time.deltaTime;
            if (stuckTimer <= 0f)
            {
                if ((transform.position - lastPos).magnitude < stuckMinMove) break;
                lastPos = transform.position;
                stuckTimer = stuckCheckTime;
            }

            timeout -= Time.deltaTime;
            yield return null;
        }

        agent.stoppingDistance = defaultStopDistance;

        if (!reached)
        {
            nextCollectTime = Time.time + 5f;
            agent.isStopped = false;
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
    private void FaceTarget(float speed)
    {
        if (player != null) FacePoint(player.position, speed);
    }

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
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, phase2KeepDistance);
    }
}