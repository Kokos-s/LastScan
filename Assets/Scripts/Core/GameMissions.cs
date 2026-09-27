using UnityEngine;

public class GameMissions : MonoBehaviour
{
    [SerializeField] private int targetOreAmount = 2;
    [SerializeField] private MissionCompletePanel missionCompletePanel;

    private int deliveredOre = 0;
    private bool missionCompleted = false;

    public int TargetOreAmount => targetOreAmount;
    public int DeliveredOre => deliveredOre;

    public void AddDeliveryOre(int amount)
    {
        if (missionCompleted) return;

        deliveredOre += amount;

        if (deliveredOre >= targetOreAmount)
        {
            deliveredOre = targetOreAmount;
            missionCompleted = true;

            if (missionCompletePanel != null)
                missionCompletePanel.Show();
        }
    }
}