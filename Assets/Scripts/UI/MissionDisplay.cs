using TMPro;
using UnityEngine;

public class MissionDisplay : MonoBehaviour
{
    [SerializeField] private RoverInventory inventory;
    [SerializeField] private GameMissions missions;
    [SerializeField] private TextMeshProUGUI label;

    void Update()
    {
        label.text = "COLLECTED:\n" + inventory.CollectedOre + " ore out of " + missions.TargetOreAmount;
    }
}