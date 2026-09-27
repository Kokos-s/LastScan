using UnityEngine;
using UnityEngine.UI;

public class BatteryUI : MonoBehaviour
{
    [SerializeField] private RoverEnergy energy;
    [SerializeField] private Image fillBar;
    [SerializeField] private Color highEnergyColor;
    [SerializeField] private Color mediumEnergyColor;
    [SerializeField] private Color lowEnergyColor;
    [SerializeField] private float mediumThreshold = 0.5f;
    [SerializeField] private float lowThreshold = 0.2f;

    [SerializeField] private float fillSpeed = 3f; 

    private float displayedPercent = 1f; 

    void Start()
    {
        
        displayedPercent = energy.CurrentEnergy / energy.MaxEnergy;
    }

    void Update()
    {
        float targetPercent = energy.CurrentEnergy / energy.MaxEnergy;

        
        displayedPercent = Mathf.MoveTowards(displayedPercent, targetPercent, fillSpeed * Time.deltaTime);

        fillBar.fillAmount = displayedPercent;

        if (displayedPercent <= lowThreshold)
            fillBar.color = lowEnergyColor;
        else if (displayedPercent <= mediumThreshold)
            fillBar.color = mediumEnergyColor;
        else
            fillBar.color = highEnergyColor;
    }
}