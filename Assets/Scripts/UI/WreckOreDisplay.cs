using UnityEngine;
using TMPro;

public class WreckOreDisplay : MonoBehaviour
{
    [SerializeField] private WreckOrePickup wreckOrePickup;
    [SerializeField] private TMP_Text pickupText;

    private void LateUpdate()
    {
        if (wreckOrePickup.ShowCollectedMessage)
        {
            pickupText.gameObject.SetActive(true);
            pickupText.text = "Collected " + wreckOrePickup.LastCollectedAmount + " minerals";
        }
        else if (wreckOrePickup.CanRecover)
        {
            pickupText.gameObject.SetActive(true);

            if (wreckOrePickup.IsPickingUp)
                pickupText.text = "Collecting minerals...";
            else
                pickupText.text = "Hold E to collect minerals";
        }
        else
        {
            pickupText.gameObject.SetActive(false);
        }
    }
}
