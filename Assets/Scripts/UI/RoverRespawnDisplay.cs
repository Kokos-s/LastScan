using UnityEngine;
using TMPro;

public class RoverRespawnDisplay : MonoBehaviour
{
    [SerializeField] private RoverRespawn roverRespawn;
    [SerializeField] private GameObject respawnPanel;
    [SerializeField] private TextMeshProUGUI countdownLabel;

    private void LateUpdate()
    {
        respawnPanel.SetActive(roverRespawn.IsRespawning);

        if (!roverRespawn.IsRespawning)
            return;

        int secondsLeft = Mathf.CeilToInt(roverRespawn.RespawnTimeLeft);
        countdownLabel.text = "Respawning in " + secondsLeft + "...";
    }
}
