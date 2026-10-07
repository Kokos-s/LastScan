using UnityEngine;

public class WheelDust : MonoBehaviour
{
    [SerializeField] private Rigidbody roverBody;
    [SerializeField] private WheelCollider wheel;

    [SerializeField] private float minSpeed = 0.3f;
    [SerializeField] private float maxSpeed = 17f;
    [SerializeField] private float maxEmission = 40f;
    [SerializeField] private float maxDustSpeed = 4f;

    private ParticleSystem dust;

    private void Awake()
    {
        dust = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        float forwardSpeed = Vector3.Dot(roverBody.linearVelocity, roverBody.transform.forward);

        float speed = Mathf.Abs(forwardSpeed);

        ParticleSystem.EmissionModule emission = dust.emission;

        if (!wheel.isGrounded || speed < minSpeed)
        {
            emission.rateOverTime = 0f;
            return;
        }

        float intensity = Mathf.InverseLerp(minSpeed, maxSpeed, speed);

        emission.rateOverTime = intensity * maxEmission;

        ParticleSystem.MainModule main = dust.main;
        main.startSpeed = intensity * maxDustSpeed;

        Vector3 dustDirection = -roverBody.transform.forward;

        if (forwardSpeed < 0f)
            dustDirection = roverBody.transform.forward;

        dustDirection += roverBody.transform.up * 0.3f;

        transform.rotation = Quaternion.LookRotation(dustDirection, roverBody.transform.up);
    }
}
