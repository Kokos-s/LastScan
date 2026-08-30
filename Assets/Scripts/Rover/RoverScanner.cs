using UnityEngine;

public class RoverScanner : MonoBehaviour
{
    [SerializeField] private RoverEnergy energy;
    [SerializeField] private float scanEnergyCost = 15f;
    [SerializeField] private AnomalyMovement[] anomalies;
    private RoverControls controls;

    private void Awake()
    {
        controls = new RoverControls();
    }

    private void OnEnable()
    {
        controls.Rover.Enable();
    }

    private void OnDisable()
    {
        controls.Rover.Disable();
    }

    void Update()
    {
        if (controls.Rover.Scan.WasPressedThisFrame())
        {
            TryActivateScan();
        }
    }

    void TryActivateScan()
    {
        if (energy.TryConsume(scanEnergyCost))
        {
            Debug.Log("Scan activated!");
            foreach (AnomalyMovement anomaly in anomalies)
            {
                anomaly.OnScanUsed(transform.position);
            }
        }
        else
        {
            Debug.Log("Not enough energy to scan!");
        }
    }
}
