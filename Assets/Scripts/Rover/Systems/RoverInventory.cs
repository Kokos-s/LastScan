using UnityEngine;

public class RoverInventory : MonoBehaviour
{
    [SerializeField] private int collectedOre = 0;
    
    public int CollectedOre
    { 
        get { return collectedOre; }
    }

    public void AddOre()
    {
        collectedOre++;
    }

    public void RemoveOre(int amount)
    {
        collectedOre -= amount;
    }
}
