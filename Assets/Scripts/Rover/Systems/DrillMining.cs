using UnityEngine;
public class DrillMining : MonoBehaviour
{
    [SerializeField] private DrillContact drillContact; 
    [SerializeField] private DrillEnergyUsage drillEnergyUsage; 
    private float timeForNextExtraction = 0f;
    [SerializeField] RoverInventory roverInventory;

    public void LateUpdate() 
    { if (timeForNextExtraction == 0f) 
            timeForNextExtraction = Random.Range(0.5f, 2f);
    
        if (drillContact.Mineral != null && drillEnergyUsage.CanDrill) 
        { 
            timeForNextExtraction -= Time.deltaTime;
            //Debug.Log(timeForNextExtraction);
            if (timeForNextExtraction <= 0f) 
            { 
                if(drillContact.Mineral.TryExtractOre())
                    roverInventory.AddOre();
                timeForNextExtraction = 0f; 
            }
        } 
    }
}


