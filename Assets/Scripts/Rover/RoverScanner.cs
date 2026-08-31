using UnityEngine;

public class RoverScanner : MonoBehaviour
{
    [SerializeField] private RoverEnergy energy;
    [SerializeField] private float scanEnergyCost = 10f;
    [SerializeField] private AnomalyMovement[] anomalies;
    [SerializeField] private GameObject scanPulsePrefab;
    [SerializeField] private GameObject scanActiveLabel;
    [SerializeField] private GameObject notEnoughEnergyLabel;
    [SerializeField] private float notEnoughEnergyLabelDuration = 2f;
    private RoverControls controls;
    private float scanLabelTimer = 0f;
    private float notEnoughEnergyTimer = 0f;

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
        if (controls.Rover.Scan.WasPressedThisFrame() & scanLabelTimer <= 0f)
            TryActivateScan();

        if (scanLabelTimer > 0f)
        {
            scanLabelTimer -= Time.deltaTime;

            if (scanLabelTimer <= 0f)
                scanActiveLabel.SetActive(false);
        }

        if (notEnoughEnergyTimer > 0f)
        {
            notEnoughEnergyTimer -= Time.deltaTime;

            if (notEnoughEnergyTimer <= 0f)
                notEnoughEnergyLabel.SetActive(false);
        }
    }

    void TryActivateScan()
    {
        if (energy.TryConsume(scanEnergyCost))
        {
            GameObject pulseObject = Instantiate(scanPulsePrefab, transform.position, Quaternion.identity);
            ScanPulseEffect pulseEffect = pulseObject.GetComponent<ScanPulseEffect>();

            scanActiveLabel.SetActive(true);
            scanLabelTimer = pulseEffect.Duration;

            foreach (AnomalyMovement anomaly in anomalies)
            {
                anomaly.OnScanUsed(transform.position);
            }
        }
        else
        {
            notEnoughEnergyLabel.SetActive(true);
            notEnoughEnergyTimer = notEnoughEnergyLabelDuration;
        }
    }
}
