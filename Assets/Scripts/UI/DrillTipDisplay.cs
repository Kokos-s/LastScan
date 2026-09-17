using UnityEngine;

public class DrillTipDisplay : MonoBehaviour
{
    [SerializeField] private GameObject drillipLabel;
    [SerializeField] private float displayDuration = 15f;
    private bool hasBeenShown = false;
    private float timer = 0f;

    public void ShowTooltip()
    {
        if (hasBeenShown)
            return;
        hasBeenShown = true;
        drillipLabel.SetActive(true);
        timer = displayDuration;
    }

    void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
                drillipLabel.SetActive(false);
        }
    }
}
