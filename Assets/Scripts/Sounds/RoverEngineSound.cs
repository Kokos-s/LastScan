using UnityEngine;

public class RoverEngineSound : MonoBehaviour
{
    [SerializeField] private RoverMovement roverMovement;
    [SerializeField] private AudioSource engineSound;

    [SerializeField] private float idleVolume = 0.1f;
    [SerializeField] private float driveVolume = 0.3f;
    [SerializeField] private float boostVolume = 0.5f;

    [SerializeField] private float idlePitch = 0.8f;
    [SerializeField] private float drivePitch = 1.1f;
    [SerializeField] private float boostPitch = 1.5f;

    [SerializeField] private float smoothSpeed = 2f;

    private void Start()
    {
        engineSound.volume = idleVolume;
        engineSound.pitch = idlePitch;
    }

    private void Update()
    {
        float targetVolume = idleVolume;
        float targetPitch = idlePitch;

        if (roverMovement.IsMotorRunning)
        {
            targetVolume = driveVolume;
            targetPitch = drivePitch;

            if (roverMovement.IsBoosting)
            {
                targetVolume = boostVolume;
                targetPitch = boostPitch;
            }
        }

        engineSound.volume = Mathf.MoveTowards(engineSound.volume, targetVolume, smoothSpeed*Time.deltaTime);

        engineSound.pitch = Mathf.MoveTowards(engineSound.pitch, targetPitch, smoothSpeed*Time.deltaTime);
    }
}