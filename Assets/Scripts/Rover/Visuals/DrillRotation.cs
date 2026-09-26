using UnityEngine;

public class DrillRotation : MonoBehaviour
{
    [SerializeField] private float accelerationTime = 1f;
    [SerializeField] private float decelerationTime = 1.5f;
    [SerializeField] private float maxRotationSpeed = 600f;

    [SerializeField] private Transform row1;
    [SerializeField] private Transform row2;
    [SerializeField] private Transform row3;
    [SerializeField] private Transform row4;

    [SerializeField] private DrillEnergyUsage drillEnergyUsage;
    [SerializeField] private RobotController robotController;

    private float currentRotationSpeed = 0f;
    private float rotationAngle = 0f;

    private Quaternion startRotation1;
    private Quaternion startRotation2;
    private Quaternion startRotation3;
    private Quaternion startRotation4;

    private void Awake()
    {
        SaveRotations();
    }

    private void SaveRotations()
    {
        if (row1 != null)
            startRotation1 = row1.localRotation;

        if (row2 != null)
            startRotation2 = row2.localRotation;

        if (row3 != null)
            startRotation3 = row3.localRotation;

        if (row4 != null)
            startRotation4 = row4.localRotation;
    }

    private void LateUpdate()
    {
        if (robotController.IsBusy || robotController.GetCurrentMineral() != null)
        {
            currentRotationSpeed = 0f;
            rotationAngle = 0f;

            SaveRotations();
            return;
        }

        if (drillEnergyUsage.CanDrill)
        {
            currentRotationSpeed += maxRotationSpeed / accelerationTime * Time.deltaTime;

            if (currentRotationSpeed > maxRotationSpeed)
                currentRotationSpeed = maxRotationSpeed;
        }
        else
        {
            currentRotationSpeed -=  maxRotationSpeed / decelerationTime * Time.deltaTime;

            if (currentRotationSpeed < 0f)
                currentRotationSpeed = 0f;
        }

        rotationAngle += currentRotationSpeed * Time.deltaTime;
        rotationAngle = Mathf.Repeat(rotationAngle, 360f);

        Quaternion oddRotation = Quaternion.AngleAxis(rotationAngle, Vector3.forward);

        Quaternion evenRotation = Quaternion.AngleAxis(-rotationAngle, Vector3.forward);

        if (row1 != null)
            row1.localRotation = startRotation1 * oddRotation;

        if (row3 != null)
            row3.localRotation = startRotation3 * oddRotation;

        if (row2 != null)
            row2.localRotation = startRotation2 * evenRotation;

        if (row4 != null)
            row4.localRotation = startRotation4 * evenRotation;
    }
}