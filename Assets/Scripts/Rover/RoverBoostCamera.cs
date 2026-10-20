using UnityEngine;

public class RoverBoostCamera : MonoBehaviour
{
    [SerializeField] private RoverMovement roverMovement;
    [SerializeField] private Rigidbody roverRb;
    [SerializeField] private float boostStartSpeed = 4f;
    [SerializeField] private float boostFullSpeed = 20f;
    [SerializeField] private float additionalFov = 8f;
    [SerializeField] private float changeSpeed = 5f;

    private Camera targetCamera;
    private float normalFov;

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
        normalFov = targetCamera.fieldOfView;
    }

    private void LateUpdate()
    {
        float targetFov = normalFov;

        if (roverMovement.IsBoosting)
        {
            Vector3 velocity = roverRb.linearVelocity;
            velocity.y = 0f;
            float speed = velocity.magnitude;
            float strength = Mathf.InverseLerp(boostStartSpeed, boostFullSpeed, speed);
            targetFov += additionalFov * strength;
        }

        targetCamera.fieldOfView = Mathf.MoveTowards(targetCamera.fieldOfView, targetFov, changeSpeed * Time.deltaTime);
    }

    private void OnDisable()
    {
        if (targetCamera != null)
            targetCamera.fieldOfView = normalFov;
    }
}
