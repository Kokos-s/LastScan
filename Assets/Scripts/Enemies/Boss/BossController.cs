using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BossController : MonoBehaviour
{
    public enum State { Chase, DrillAttack, RockAttack, DiscAttack, ChargeAttack, Stunned, Recover, Dead }

    [SerializeField] private Transform player;
    [SerializeField] private Transform drillPivot;
    [SerializeField] private Transform drillModel;
    [SerializeField] private Renderer drillRenderer;
    [SerializeField] private DrillHitbox drillHitbox;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Transform armJoint;
    [SerializeField] private Transform elbowJoint;

    [SerializeField, Range(0.05f, 0.95f)] private float rangedPhaseThreshold = 0.5f;
    [SerializeField] private float phase1RangedChance = 0.2f;
    [SerializeField] private float phase1CooldownMultiplier = 2f;
    [SerializeField] private float phase2CooldownMultiplier = 0.6f;
    [SerializeField] private float phase2KeepDistance = 20f;

    [SerializeField, Range(0.01f, 0.9f)] private float chargePhaseThreshold = 0.2f;
    [SerializeField] private float chargeWindupTime = 0.9f;
    [SerializeField, Range(0.1f, 1f)] private float chargeAimUntil = 0.85f;
    [SerializeField] private float chargeAimTurnSpeed = 14f;
    [SerializeField] private float chargeSteerRate = 0f;
    [SerializeField] private float chargeTopSpeed = 70f;
    [SerializeField] private float chargeAcceleration = 250f;
    [SerializeField] private float chargeRange = 1000f;
    [SerializeField] private float chargeTimeLimit = 10f;
    [SerializeField] private float chargeStartMaxDistance = 150f;
    [SerializeField] private LayerMask chargeObstacleMask;
    [SerializeField] private float chargeIgnoreHeight = 3f;
    [SerializeField] private float chargeSkin = 0.3f;
    [SerializeField] private float chargeDamage = 4f;
    [SerializeField] private float chargeKnockback = 15f;
    [SerializeField] private float chargeWidthMultiplier = 1.7f;
    [SerializeField] private float chargeHitForward = 2f;
    [SerializeField] private bool chargeStopsOnPlayer = true;
    [SerializeField] private float chargeStunTime = 3f;
    [SerializeField] private float chargeSlideBackTime = 0.6f;
    [SerializeField] private float chargeCooldownAfterStun = 1.5f;
    [SerializeField] private float chargeHitShake = 1.2f;
    [SerializeField] private float chargeCrashShake = 2f;
    [SerializeField] private bool showChargeArrow = true;
    [SerializeField, Range(0.3f, 1f)] private float chargeArrowShaft = 0.75f;
    [SerializeField] private float chargeArrowHeight = 0.3f;
    [SerializeField] private Color chargeArrowColor = new Color(1f, 0.1f, 0.1f, 1f);
    [SerializeField] private AudioClip chargeWindupClip;
    [SerializeField] private AudioClip crashClip;

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
    [SerializeField] private float pickupDistance = 12f;
    [SerializeField] private float rockStopDistance = 8f;
    [SerializeField] private float rockApproachTimeout = 20f;
    [SerializeField] private float pickupTime = 0.5f;
    [SerializeField] private float throwWindup = 1.0f;
    [SerializeField] private float rockSwingTime = 0.25f;
    [SerializeField] private float throwSpeed = 14f;
    [SerializeField] private float leadFactor = 0.7f;
    [SerializeField] private float rockRecover = 1.0f;
    [SerializeField] private float rockMinFlightTime = 1.2f;
    [SerializeField] private float rockMaxFlightTime = 2.5f;
    [SerializeField] private float stuckCheckTime = 1.5f;
    [SerializeField] private float stuckMinMove = 0.5f;
    [SerializeField] private float rockBlacklistTime = 15f;

    [SerializeField] private Transform fingerA;
    [SerializeField] private Transform fingerB;
    [SerializeField] private Vector3 fingerAAxis = Vector3.up;
    [SerializeField] private Vector3 fingerBAxis = Vector3.up;
    [SerializeField] private float fingerAClosedAngle = 20f;
    [SerializeField] private float fingerBClosedAngle = -20f;
    [SerializeField] private float fingerCloseTime = 0.25f;
    [SerializeField] private float fingerOpenTime = 0.12f;

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
    private BoxCollider bossBox;
    private DizzyStars dizzy;
    private State state;
    private float currentSpin;
    private Color baseColor;
    private Vector3 pivotRestPos;
    private Quaternion armRestRot;
    private Quaternion elbowRestRot;
    private Quaternion throwShoulderRest;
    private Quaternion throwElbowRest;
    private Quaternion fingerARest;
    private Quaternion fingerBRest;
    private float fingerK;
    private Health health;
    private Rigidbody playerBody;
    private RockProjectile heldRock;
    private BossDisc heldDisc;
    private float nextRockTime;
    private float nextDiscTime;
    private float nextCollectTime;
    private float nextPhase1Roll;
    private float nextRockSearch;
    private float nextChargeTime;
    private RockProjectile cachedRock;
    private float defaultStopDistance;
    private bool started = false;
    private bool warnedNoPlayer = false;
    private bool wasRangedPhase = false;
    private bool wasChargePhase = false;
    private float chargeFloorY;
    private GameObject arrowGo;
    private Mesh arrowMesh;
    private Material arrowMat;
    private readonly HashSet<IDamageable> chargeHit = new HashSet<IDamageable>();
    private readonly Dictionary<RockProjectile, float> rockBlacklist = new Dictionary<RockProjectile, float>();

    public State CurrentState => state;

    private Transform DiscHand => discHoldPoint != null ? discHoldPoint : holdPoint;

    private bool IsRangedPhase => health != null && health.Normalized <= rangedPhaseThreshold;
    private bool IsChargePhase => health != null && health.Normalized <= chargePhaseThreshold;
    private float CooldownMul => IsRangedPhase ? phase2CooldownMultiplier : phase1CooldownMultiplier;
    private bool ShouldAbortForDrill(float dist) => !IsRangedPhase && dist <= drillRange;

    private int ObstacleMask => chargeObstacleMask.value != 0
        ? chargeObstacleMask.value
        : LayerMask.GetMask("Walls", "Columns", "Rocks");

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
            warnedNoPlayer = true;
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
        bossBox = GetComponent<BoxCollider>();
        dizzy = GetComponent<DizzyStars>();
        if (dizzy == null) dizzy = gameObject.AddComponent<DizzyStars>();

        ResolvePlayer();
        pivotRestPos = drillPivot.localPosition;
        armRestRot = armJoint.localRotation;
        elbowRestRot = elbowJoint.localRotation;
        if (throwShoulder != null) throwShoulderRest = throwShoulder.localRotation;
        if (throwElbow != null) throwElbowRest = throwElbow.localRotation;
        if (fingerA != null) fingerARest = fingerA.localRotation;
        if (fingerB != null) fingerBRest = fingerB.localRotation;
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
        CleanupCharge(false);
    }

    private void Start()
    {
        state = State.Chase;
        started = true;
        nextRockTime = Time.time + 2f;
        nextDiscTime = Time.time + 3f;
        nextCollectTime = Time.time + 3f;
        nextPhase1Roll = Time.time + 3f;
        nextChargeTime = Time.time + 2f;
    }

    private void Update()
    {
        drillModel.Rotate(0f, 0f, currentSpin * Time.deltaTime, Space.Self);

        if (!ResolvePlayer()) return;

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

        bool charge = IsChargePhase;
        if (charge != wasChargePhase)
        {
            wasChargePhase = charge;
            if (charge) nextChargeTime = Time.time + 1f;
        }

        if (state == State.Chase) UpdateChase();
    }

    private void ResetAfterInterrupt()
    {
        StopAllCoroutines();
        CleanupCharge(true);

        drillHitbox.gameObject.SetActive(false);
        currentSpin = idleSpin;
        drillRenderer.material.color = baseColor;
        drillPivot.localPosition = pivotRestPos;
        armJoint.localRotation = armRestRot;
        elbowJoint.localRotation = elbowRestRot;
        SetThrowArm(0f, 0f);
        SetFingers(0f);

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
        nextChargeTime = Time.time + 2f;
    }

    private void CleanupCharge(bool restoreAgent)
    {
        DestroyArrow();
        if (dizzy != null) dizzy.Hide(true);
        if (agent != null) agent.updateRotation = true;
        if (restoreAgent) RestoreAgent();
    }

    private void RestoreAgent()
    {
        if (agent == null || agent.enabled) return;

        bool found = NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 300f, NavMesh.AllAreas);
        if (found) transform.position = hit.position;
        agent.enabled = true;
        if (found && agent.isOnNavMesh) agent.Warp(hit.position);
    }

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
        if (!agent.isOnNavMesh) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (IsChargePhase)
        {
            agent.stoppingDistance = defaultStopDistance;
            agent.SetDestination(player.position);

            if (Time.time >= nextChargeTime && dist <= chargeStartMaxDistance)
            {
                StartCoroutine(ChargeRoutine());
                return;
            }

            if (dist <= drillRange)
                StartCoroutine(DrillAttackRoutine());
            return;
        }

        bool phase2 = IsRangedPhase;

        bool discReady = StoredDiscCount() > 0 && discStorage != null && dist <= discMaxRange;
        bool discToCollect = StoredDiscCount() == 0 && FindNearestLyingDisc() != null;
        RefreshRockCache(dist);
        bool rockReady = cachedRock != null;
        bool hasRangedAccess = discReady || discToCollect || rockReady;

        agent.stoppingDistance = (phase2 && hasRangedAccess) ? phase2KeepDistance : defaultStopDistance;
        agent.SetDestination(player.position);

        bool drillAllowed = !phase2 || !hasRangedAccess;
        if (drillAllowed && dist <= drillRange)
        {
            agent.stoppingDistance = defaultStopDistance;
            StartCoroutine(DrillAttackRoutine());
            return;
        }

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

    private void SetFingers(float k)
    {
        fingerK = k;
        if (fingerA != null)
            fingerA.localRotation = fingerARest * Quaternion.AngleAxis(fingerAClosedAngle * k, fingerAAxis);
        if (fingerB != null)
            fingerB.localRotation = fingerBRest * Quaternion.AngleAxis(fingerBClosedAngle * k, fingerBAxis);
    }

    private IEnumerator AnimateFingers(float target, float time)
    {
        float from = fingerK;
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            SetFingers(Mathf.Lerp(from, target, Mathf.SmoothStep(0f, 1f, t / time)));
            yield return null;
        }
        SetFingers(target);
    }

    private Vector3 FlatForward()
    {
        Vector3 f = transform.forward;
        f.y = 0f;
        return f.sqrMagnitude < 0.001f ? Vector3.forward : f.normalized;
    }

    private void GetBodyBox(out Vector3 center, out Vector3 half, out Quaternion rot)
    {
        if (bossBox != null)
        {
            Transform tr = bossBox.transform;
            center = tr.TransformPoint(bossBox.center);
            Vector3 s = tr.lossyScale;
            half = new Vector3(Mathf.Abs(bossBox.size.x * s.x), Mathf.Abs(bossBox.size.y * s.y),
                               Mathf.Abs(bossBox.size.z * s.z)) * 0.5f;
            rot = tr.rotation;
        }
        else
        {
            center = transform.position + Vector3.up * 2f;
            half = Vector3.one * 3f;
            rot = transform.rotation;
        }
    }

    private bool IsBlocking(Collider c)
    {
        if (c == null || c.isTrigger) return false;
        if (c.GetComponentInParent<BossController>() != null) return false;
        if (c.GetComponentInParent<RockProjectile>() != null) return false;
        if (c.GetComponentInParent<BossDisc>() != null) return false;
        if (c.bounds.max.y < transform.position.y + chargeIgnoreHeight) return false;
        return true;
    }

    private bool CastObstacle(Vector3 dir, float distance, out float hitDist)
    {
        hitDist = distance;
        GetBodyBox(out Vector3 c, out Vector3 half, out Quaternion rot);

        var inside = Physics.OverlapBox(c, half * 0.9f, rot, ObstacleMask, QueryTriggerInteraction.Ignore);
        foreach (var col in inside)
        {
            if (!IsBlocking(col)) continue;
            hitDist = 0f;
            return true;
        }

        var hits = Physics.BoxCastAll(c, half, dir, rot, distance, ObstacleMask, QueryTriggerInteraction.Ignore);
        bool found = false;
        float best = distance;
        foreach (var h in hits)
        {
            if (!IsBlocking(h.collider)) continue;
            if (h.distance < best) { best = h.distance; found = true; }
        }
        hitDist = best;
        return found;
    }

    private IEnumerator ChargeRoutine()
    {
        if (!agent.isOnNavMesh) yield break;

        state = State.ChargeAttack;
        agent.ResetPath();
        agent.isStopped = true;
        agent.updateRotation = false;
        chargeFloorY = transform.position.y - agent.baseOffset;

        PlayClip(chargeWindupClip);
        CreateArrow();

        float t = 0f;
        while (t < chargeWindupTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / chargeWindupTime);

            if (k < chargeAimUntil) FaceTarget(chargeAimTurnSpeed);

            currentSpin = Mathf.Lerp(idleSpin, windupSpin, k);
            drillRenderer.material.color = Color.Lerp(baseColor, telegraphColor, k);
            drillPivot.localPosition = pivotRestPos + Vector3.back * (pullBackOffset * k)
                                       + Random.insideUnitSphere * (0.15f * k);
            SetArm(armWindupAngle * k, elbowWindupAngle * k);

            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * Mathf.Lerp(8f, 30f, k));
            UpdateArrow(FlatForward(), 0.35f + 0.5f * pulse);
            yield return null;
        }

        Vector3 dir = FlatForward();
        DestroyArrow();

        agent.enabled = false;
        currentSpin = strikeSpin;
        SetArm(armStrikeAngle, elbowStrikeAngle);
        drillPivot.localPosition = pivotRestPos + Vector3.forward * 0.3f;

        chargeHit.Clear();
        float speed = 0f, traveled = 0f, time = 0f;
        bool crashed = false;

        while (time < chargeTimeLimit && traveled < chargeRange)
        {
            float dt = Time.deltaTime;
            time += dt;

            if (chargeSteerRate > 0f && player != null)
            {
                Vector3 to = player.position - transform.position;
                to.y = 0f;
                if (to.sqrMagnitude > 0.01f)
                {
                    dir = Vector3.RotateTowards(dir, to.normalized, chargeSteerRate * Mathf.Deg2Rad * dt, 0f);
                    dir.y = 0f;
                    dir.Normalize();
                    transform.rotation = Quaternion.LookRotation(dir);
                }
            }

            speed = Mathf.Min(chargeTopSpeed, speed + chargeAcceleration * dt);
            float step = speed * dt;

            bool blocked = CastObstacle(dir, step + chargeSkin, out float d);
            float move = blocked ? Mathf.Max(0f, d - chargeSkin) : step;
            transform.position += dir * move;
            traveled += move;

            bool hitPlayer = CheckChargeHits(dir);

            if (blocked || (hitPlayer && chargeStopsOnPlayer))
            {
                crashed = true;
                break;
            }

            yield return null;
        }

        state = State.Stunned;
        currentSpin = 0f;
        drillRenderer.material.color = baseColor;
        drillPivot.localPosition = pivotRestPos;

        if (crashed)
        {
            PlayClip(crashClip);
            if (HitFeedbackFX.Instance != null)
            {
                HitFeedbackFX.Instance.Shake(chargeCrashShake, 0.5f);
                HitFeedbackFX.Instance.HitStop(0.08f);
            }
        }

        dizzy.Show();

        Quaternion armFrom = armJoint.localRotation;
        Quaternion elbowFrom = elbowJoint.localRotation;
        float back = 0.5f;
        t = 0f;
        while (t < back)
        {
            t += Time.deltaTime;
            float k = t / back;
            armJoint.localRotation = Quaternion.Slerp(armFrom, armRestRot, k);
            elbowJoint.localRotation = Quaternion.Slerp(elbowFrom, elbowRestRot, k);
            yield return null;
        }

        float slide = Mathf.Min(chargeSlideBackTime, Mathf.Max(0f, chargeStunTime - back));
        yield return new WaitForSeconds(Mathf.Max(0f, chargeStunTime - back - slide));

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit nav, 300f, NavMesh.AllAreas))
        {
            Vector3 from = transform.position;
            Vector3 to = new Vector3(nav.position.x, from.y, nav.position.z);
            t = 0f;
            while (t < slide)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / slide));
                yield return null;
            }
            transform.position = to;
        }

        dizzy.Hide();
        RestoreAgent();
        agent.updateRotation = true;
        currentSpin = idleSpin;
        nextChargeTime = Time.time + chargeCooldownAfterStun;
        if (agent.isOnNavMesh) agent.isStopped = false;
        state = State.Chase;
    }

    private bool CheckChargeHits(Vector3 dir)
    {
        GetBodyBox(out Vector3 center, out Vector3 half, out Quaternion rot);

        half.x *= chargeWidthMultiplier;
        center += dir * chargeHitForward;

        bool hitPlayer = false;
        var hits = Physics.OverlapBox(center, half, rot, ~0, QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            if (h.GetComponentInParent<BossController>() != null) continue;
            if (h.GetComponentInParent<DestructibleColumn>() != null) continue;

            var target = h.GetComponentInParent<IDamageable>();
            if (target == null) continue;

            if (target is RoverHealth) hitPlayer = true;
            if (!chargeHit.Add(target)) continue;

            target.TakeImpactDamage(chargeDamage);

            if (target is RoverHealth && playerBody != null)
                playerBody.AddForce((dir + Vector3.up * 0.2f).normalized * chargeKnockback, ForceMode.VelocityChange);

            if (HitFeedbackFX.Instance != null)
                HitFeedbackFX.Instance.Shake(chargeHitShake, 0.3f);
        }
        return hitPlayer;
    }

    private void CreateArrow()
    {
        if (!showChargeArrow || arrowGo != null) return;

        arrowGo = new GameObject("ChargeArrow");
        arrowMesh = new Mesh { name = "ChargeArrow" };
        arrowMesh.MarkDynamic();
        arrowMesh.vertices = new Vector3[7];
        arrowMesh.triangles = new[] { 0, 2, 1, 0, 3, 2, 4, 6, 5 };

        arrowGo.AddComponent<MeshFilter>().sharedMesh = arrowMesh;
        var r = arrowGo.AddComponent<MeshRenderer>();
        arrowMat = new Material(Shader.Find("Sprites/Default"));
        r.sharedMaterial = arrowMat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
    }

    private void UpdateArrow(Vector3 dir, float alpha)
    {
        if (arrowGo == null) return;

        GetBodyBox(out Vector3 center, out Vector3 half, out Quaternion rot);

        float maxLen = Mathf.Min(chargeRange, 2000f);
        bool found = CastObstacle(dir, maxLen, out float travel);
        float length = found ? travel + half.z : maxLen;
        length = Mathf.Max(1f, length);

        float width = half.x * 2f * chargeWidthMultiplier;
        float shaft = width * chargeArrowShaft * 0.5f;
        float headLen = Mathf.Min(width * 0.9f, length * 0.4f);
        float bodyLen = length - headLen;

        arrowMesh.vertices = new[]
        {
            new Vector3(-shaft, 0f, 0f),
            new Vector3( shaft, 0f, 0f),
            new Vector3( shaft, 0f, bodyLen),
            new Vector3(-shaft, 0f, bodyLen),
            new Vector3(-width * 0.5f, 0f, bodyLen),
            new Vector3( width * 0.5f, 0f, bodyLen),
            new Vector3(0f, 0f, length)
        };
        arrowMesh.RecalculateBounds();

        arrowGo.transform.SetPositionAndRotation(
            new Vector3(center.x, chargeFloorY + chargeArrowHeight, center.z),
            Quaternion.LookRotation(dir, Vector3.up));

        Color c = chargeArrowColor;
        c.a = Mathf.Clamp01(alpha);
        arrowMat.color = c;
    }

    private void DestroyArrow()
    {
        if (arrowGo != null) Destroy(arrowGo);
        if (arrowMat != null) Destroy(arrowMat);
        if (arrowMesh != null) Destroy(arrowMesh);
        arrowGo = null;
        arrowMat = null;
        arrowMesh = null;
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip != null) AudioSource.PlayClipAtPoint(clip, transform.position, 1f);
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
            float k = Mathf.SmoothStep(0f, 1f, t / pickupTime);
            rock.transform.position = Vector3.Lerp(startPos, holdPoint.position, k);
            yield return null;
        }
        rock.transform.SetParent(holdPoint);
        rock.transform.localPosition = Vector3.zero;

        yield return StartCoroutine(AnimateFingers(1f, fingerCloseTime));

        t = 0f;
        while (t < throwWindup)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / throwWindup);
            FaceTarget(turnSpeed);
            SetThrowArm(throwShoulderBack * k, throwElbowBack * k);
            yield return null;
        }

        bool released = false;
        t = 0f;
        while (t < rockSwingTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / rockSwingTime);
            FaceTarget(turnSpeed);
            SetThrowArm(Mathf.Lerp(throwShoulderBack, throwShoulderForward, k),
                        Mathf.Lerp(throwElbowBack, throwElbowForward, k));

            if (!released && k >= releasePoint)
            {
                ReleaseRock(rock);
                released = true;
            }
            yield return null;
        }
        if (!released) ReleaseRock(rock);

        t = 0f;
        while (t < throwReturnTime)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / throwReturnTime);
            SetThrowArm(Mathf.Lerp(throwShoulderForward, 0f, k),
                        Mathf.Lerp(throwElbowForward, 0f, k));
            yield return null;
        }
        SetThrowArm(0f, 0f);

        state = State.Recover;
        yield return new WaitForSeconds(rockRecover);

        nextRockTime = Time.time + rockCooldown * CooldownMul;
        agent.isStopped = false;
        state = State.Chase;
    }

    private void ReleaseRock(RockProjectile rock)
    {
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

        StartCoroutine(AnimateFingers(0f, fingerOpenTime));
    }

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
        CleanupCharge(false);
        state = State.Dead;
        drillHitbox.gameObject.SetActive(false);
        agent.stoppingDistance = defaultStopDistance;
        if (agent.isOnNavMesh) agent.isStopped = true;
        currentSpin = 0f;
        SetThrowArm(0f, 0f);
        SetFingers(0f);

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