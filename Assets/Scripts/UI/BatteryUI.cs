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

    void Update()
    {
        float energyPercent = energy.CurrentEnergy / energy.MaxEnergy;
        fillBar.fillAmount = energyPercent;

        if (energyPercent <= lowThreshold)
            fillBar.color = lowEnergyColor;
        else if (energyPercent <= mediumThreshold)
            fillBar.color = mediumEnergyColor;
        else
            fillBar.color = highEnergyColor;
    }
}
