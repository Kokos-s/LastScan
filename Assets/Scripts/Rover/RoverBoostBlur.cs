using UnityEngine;

public class RoverBoostBlur : MonoBehaviour
{
    [SerializeField] private RoverMovement roverMovement;
    [SerializeField] private Rigidbody roverBody;
    [SerializeField] private Material blurMaterial;

    [SerializeField] private float startSpeed = 4f;
    [SerializeField] private float fullSpeed = 20f;
    [SerializeField] private float maxBlur = 0.025f;
    [SerializeField] private float changeSpeed = 0.03f;

    private float currentBlur;

    private void OnEnable()
    {
        currentBlur = 0f;
        blurMaterial.SetFloat("_Strength", 0f);
    }

    private void LateUpdate()
    {
        float targetBlur = 0f;

        if (roverMovement.IsBoosting)
        {
            Vector3 velocity = roverBody.linearVelocity;
            velocity.y = 0f;
            float intensity = Mathf.InverseLerp(startSpeed, fullSpeed, velocity.magnitude);
            targetBlur = maxBlur * intensity;
        }

        currentBlur = Mathf.MoveTowards(currentBlur, targetBlur, changeSpeed * Time.deltaTime);
        blurMaterial.SetFloat("_Strength", currentBlur);
    }

    private void OnDisable()
    {
        blurMaterial.SetFloat("_Strength", 0f);
    }
}