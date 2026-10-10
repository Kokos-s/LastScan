using UnityEngine;

public class WreckOre : MonoBehaviour
{
    [SerializeField] private int storedOre;

    public int StoredOre
    {
        get { return storedOre; }
    }

    public void SetOre(int amount)
    {
        storedOre = amount;
    }

    public int TakeOre()
    {
        int amount = storedOre;
        storedOre = 0;
        return amount;
    }
}