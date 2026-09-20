using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoverRecoveryDisplay : MonoBehaviour
{
    [SerializeField] private RoverRecovery roverRecovery;
    [SerializeField] private Slider recoverySlider;
    [SerializeField] private TMP_Text recoveryText;

    private void Start()
    {
        recoverySlider.minValue = 0f;
        recoverySlider.maxValue = 1f;
        recoverySlider.wholeNumbers = false;
        recoverySlider.interactable = false;
        recoverySlider.value = 0f;
        recoveryText.text = "Stuck? Hold R!";
    }

    private void Update()
    {
        recoverySlider.value = roverRecovery.RecoveryProgress;

        if (roverRecovery.IsDead)
            recoveryText.text = "Rover destroyed";
        else if (roverRecovery.IsDamageBlocked)
            recoveryText.text = "Damage taken!";
        else if (roverRecovery.RecoveryProgress > 0f)
            recoveryText.text = "Preparing to teleport...";
        else
            recoveryText.text = "Stuck? Hold R!";
    }
}
