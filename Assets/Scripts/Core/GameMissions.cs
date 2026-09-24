using UnityEngine;

public class GameMissions : MonoBehaviour
{
    [SerializeField] private int targetOreAmount = 50;
    private int deliveredOre = 0;

    public int TargetOreAmount
    {
        get { return targetOreAmount; }
    }

    public int DeliveredOre
    { 
        get { return deliveredOre; } 
    }
    
    public void AddDeliveryOre(int amount)
    {
        deliveredOre += amount;
    }

}