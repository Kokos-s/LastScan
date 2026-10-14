using System;
using UnityEngine;

public class GameMissions : MonoBehaviour
{
    [SerializeField] private int targetOreAmount = 2;

    private int deliveredOre = 0;
    private bool missionCompleted = false;

    public int TargetOreAmount => targetOreAmount;
    public int DeliveredOre => deliveredOre;

    public event Action OnMissionCompleted;

    public void AddDeliveryOre(int amount)
    {
        if (missionCompleted) return;

        deliveredOre += amount;

        if (deliveredOre >= targetOreAmount)
        {
            deliveredOre = targetOreAmount;
            missionCompleted = true;
            OnMissionCompleted?.Invoke();
        }
    }
}