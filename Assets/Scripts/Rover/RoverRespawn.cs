using UnityEngine;
using UnityEngine.AI;

public class RoverRespawn : MonoBehaviour
{
    [SerializeField] private RoverHealth roverHealth;
    [SerializeField] private RoverInventory roverInventory;
    [SerializeField] private RoverMovement roverMovement;
    [SerializeField] private RoverEnergy roverEnergy;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float respawnDelay = 5f;
    [SerializeField] private GameObject wreckPrefab;
    [SerializeField] private WorldSwitcher worldSwitcher;

    
    [SerializeField] private float mainPartMass = 400f;           
    [SerializeField] private float mainPartLinearDamping = 3f;    
    [SerializeField] private float mainPartAngularDamping = 10f;
    [SerializeField] private bool mainPartKeepUpright = true;     
    [SerializeField] private float mainPartFriction = 1f;         
    [SerializeField, Range(0.05f, 1f)] private float bossSlowFactor = 0.3f; 
    [SerializeField] private bool dropMainPartToGround = true;
    [SerializeField] private bool addNavObstacle = true;
    [SerializeField] private string mainPartLayer = "Columns";    

    
    [SerializeField] private bool decorIgnoresRover = true;

    private Rigidbody roverBody;
    private float respawnTimer;

    public bool IsRespawning
    {
        get { return roverHealth.IsDead; }
    }

    public float RespawnTimeLeft
    {
        get { return Mathf.Max(0f, respawnDelay - respawnTimer); }
    }

    private void Awake()
    {
        roverBody = GetComponent<Rigidbody>();

        if (worldSwitcher == null)
            worldSwitcher = FindFirstObjectByType<WorldSwitcher>();
    }

    private void Update()
    {
        if (!roverHealth.IsDead)
        {
            respawnTimer = 0f;
            return;
        }

        respawnTimer += Time.deltaTime;

        if (respawnTimer >= respawnDelay)
            Respawn();
    }

    private void Respawn()
    {
        int lostOre = roverInventory.CollectedOre;
        Vector3 wreckPosition = roverBody.position;
        Quaternion wreckRotation = roverBody.rotation;
        Vector3 wreckVelocity = roverBody.linearVelocity;
        Vector3 wreckAngularVelocity = roverBody.angularVelocity;
        Vector3 roverCenter = roverBody.worldCenterOfMass;
        roverBody.linearVelocity = Vector3.zero;
        roverBody.angularVelocity = Vector3.zero;
        roverMovement.ResetAfterRespawn();

        Transform point = spawnPoint;
        Transform worldRoot = null;
        if (worldSwitcher != null)
        {
            worldRoot = worldSwitcher.CurrentWorldRoot;
            if (worldSwitcher.InCave && worldSwitcher.CaveSpawn != null)
                point = worldSwitcher.CaveSpawn;
        }

        roverBody.position = point.position;
        roverBody.rotation = point.rotation;

        respawnTimer = 0f;
        roverInventory.RemoveOre(roverInventory.CollectedOre);
        roverEnergy.AddEnergy(roverEnergy.MaxEnergy);

        GameObject wreck = Instantiate(wreckPrefab, wreckPosition, wreckRotation, worldRoot);

        
        foreach (var part in wreck.GetComponentsInChildren<RockProjectile>(true))
            part.MarkPlayerOnly();

        WreckOre wreckOre = wreck.GetComponentInChildren<WreckOre>();
        wreckOre.SetOre(lostOre);

        
        Rigidbody mainRb = wreckOre.GetComponentInParent<Rigidbody>();
        if (mainRb == null) mainRb = wreckOre.GetComponentInChildren<Rigidbody>();

        Rigidbody[] pieces = wreck.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody piece in pieces)
        {
            
            if (piece == mainRb) continue;

            Vector3 offset = piece.worldCenterOfMass - roverCenter;
            piece.linearVelocity = wreckVelocity + Vector3.Cross(wreckAngularVelocity, offset);
            piece.angularVelocity = wreckAngularVelocity;
        }

        roverHealth.Revive();

        SetupMainPart(mainRb);
        SetupDecor(wreck, mainRb);
    }

    
    private void SetupMainPart(Rigidbody mainRb)
    {
        if (mainRb == null) return;

        
        mainRb.isKinematic = false;
        mainRb.linearVelocity = Vector3.zero;
        mainRb.angularVelocity = Vector3.zero;
        mainRb.mass = mainPartMass;
        mainRb.linearDamping = mainPartLinearDamping;
        mainRb.angularDamping = mainPartAngularDamping;
        mainRb.interpolation = RigidbodyInterpolation.Interpolate;
        mainRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        if (mainPartKeepUpright)
            mainRb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        
        if (!string.IsNullOrEmpty(mainPartLayer))
        {
            int layer = LayerMask.NameToLayer(mainPartLayer);
            if (layer >= 0)
            {
                foreach (var t in mainRb.GetComponentsInChildren<Transform>(true))
                    t.gameObject.layer = layer;
            }
        }

        
        var mat = new PhysicsMaterial("HeavyWreck")
        {
            dynamicFriction = mainPartFriction,
            staticFriction = mainPartFriction,
            bounciness = 0f,
            frictionCombine = PhysicsMaterialCombine.Maximum,
            bounceCombine = PhysicsMaterialCombine.Minimum
        };
        foreach (var c in mainRb.GetComponentsInChildren<Collider>(true))
            if (c != null && !c.isTrigger) c.material = mat;

        Collider mainCol = mainRb.GetComponentInChildren<Collider>();
        if (mainCol == null) return;

        
        if (dropMainPartToGround)
        {
            Bounds b = mainCol.bounds;
            Vector3 from = new Vector3(b.center.x, b.max.y + 1f, b.center.z);
            if (Physics.Raycast(from, Vector3.down, out RaycastHit hit, 200f,
                                LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore))
            {
                float delta = hit.point.y - b.min.y;
                mainRb.transform.position += Vector3.up * delta;
            }
        }

        
        if (addNavObstacle && mainRb.GetComponent<NavMeshObstacle>() == null)
        {
            Bounds b = mainCol.bounds;
            Transform tr = mainRb.transform;
            Vector3 s = tr.lossyScale;

            var obs = mainRb.gameObject.AddComponent<NavMeshObstacle>();
            obs.shape = NavMeshObstacleShape.Box;
            obs.center = tr.InverseTransformPoint(b.center);
            obs.size = new Vector3(
                b.size.x / Mathf.Max(0.0001f, Mathf.Abs(s.x)),
                b.size.y / Mathf.Max(0.0001f, Mathf.Abs(s.y)),
                b.size.z / Mathf.Max(0.0001f, Mathf.Abs(s.z)));
            obs.carving = true;
            obs.carveOnlyStationary = true;
        }

        
        var heavy = mainRb.GetComponent<HeavyWreckPart>();
        if (heavy == null) heavy = mainRb.gameObject.AddComponent<HeavyWreckPart>();
        heavy.SetSlowFactor(bossSlowFactor);
    }

    
    private void SetupDecor(GameObject wreck, Rigidbody mainRb)
    {
        if (!decorIgnoresRover) return;

        var roverCols = new System.Collections.Generic.List<Collider>();
        foreach (var rc in GetComponentsInChildren<Collider>(false))
            if (rc != null && rc.enabled && !rc.isTrigger) roverCols.Add(rc);

        foreach (var c in wreck.GetComponentsInChildren<Collider>(true))
        {
            if (c == null || c.isTrigger) continue;

            Rigidbody owner = c.attachedRigidbody;

            if (owner != null && owner == mainRb) continue;
            if (owner != null && owner.GetComponent<RockProjectile>() != null) continue;

            if (!c.enabled || !c.gameObject.activeInHierarchy) continue;

            foreach (var rc in roverCols)
                Physics.IgnoreCollision(c, rc, true);
        }
    }
}