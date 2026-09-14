using UnityEngine;

public class Mineral : MonoBehaviour
{
    [SerializeField] private int remainingOre;
    [SerializeField] private DrillTipDisplay drillTipDisplay;

    public int RemainingOre
    {
        get { return remainingOre; }
    }

    private void Awake()
    {
        remainingOre = Random.Range(2, 7);
    }

    void OnTriggerEnter(Collider other)
    {
        RoverEnergy rover = other.GetComponentInParent<RoverEnergy>();

        if (rover != null)
            drillTipDisplay.ShowTooltip();
    }

    public bool TryExtractOre()
    {
        if (remainingOre > 0)
        {
            remainingOre--;
            if (remainingOre == 0)
                Destroy(gameObject);
            return true;
        }

        return false;
    }
}