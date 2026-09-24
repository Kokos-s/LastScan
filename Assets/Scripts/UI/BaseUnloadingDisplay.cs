using UnityEngine;
using TMPro;

public class BaseUnloadingDisplay : MonoBehaviour
{
    [SerializeField] private BaseUnloading unloading;
    [SerializeField] private TextMeshProUGUI label;

    private void Update()
    {
        if (!unloading.RoverInZone)
        {
            label.enabled = false;
            return;
        }

        label.enabled = true;

        if (!unloading.HasOre)
        {
            label.text = "No ore on board.";
        }
        else if (unloading.IsUnloading)
        {
            label.text = "Unloading ore... Repairing rover...";
        }
        else
        {
            label.text = "Hold F to unload ore and repair your rover.";
        }
    }
}