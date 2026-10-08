using UnityEngine;

public class BoostDisplay : MonoBehaviour
{
    [SerializeField] private RoverMovement roverMovement;
    [SerializeField] private GameObject boostIndicator;

    private void LateUpdate()
    {
        boostIndicator.SetActive(roverMovement.IsBoosting);
    }
}