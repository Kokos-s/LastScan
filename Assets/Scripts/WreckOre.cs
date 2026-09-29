using UnityEngine;

public class WreckOre : MonoBehaviour
{
    private int storedOre;

    public void SetOre(int amount)
    {
        storedOre = amount;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (storedOre <= 0)
            return;

        RoverInventory inventory = other.GetComponentInParent<RoverInventory>();
        RoverHealth health = other.GetComponentInParent<RoverHealth>();

        if (inventory == null || health == null || health.IsDead)
            return;

        inventory.AddOre(storedOre);
        storedOre = 0;
    }
}
