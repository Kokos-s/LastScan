using TMPro;
using UnityEngine;

public class MissionDisplay : MonoBehaviour
{
    [SerializeField] private RoverInventory inventory;
    [SerializeField] private GameMissions missions;
    [SerializeField] private TextMeshProUGUI label;

    void Update()
    {
        label.text = "Ore on board: " + inventory.CollectedOre + "\nDelivered to Base: " + missions.DeliveredOre + " / " + missions.TargetOreAmount;
    }
}