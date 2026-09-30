using UnityEngine;
using UnityEngine.UI;

public class BatteryUI : MonoBehaviour
{
    [SerializeField] private RoverEnergy energy;
    [SerializeField] private Image batteryImage;
    [SerializeField] private Sprite[] batterySprites;

    private void Start()
    {
        UpdateBatteryUI();
    }

    private void Update()
    {
        UpdateBatteryUI();
    }

    private void UpdateBatteryUI()
    {
        float percent = energy.CurrentEnergy / energy.MaxEnergy;

        int index = Mathf.RoundToInt(percent * (batterySprites.Length - 1));

        batteryImage.sprite = batterySprites[index];
    }
}