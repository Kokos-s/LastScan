using UnityEngine;

public class DrillHintDisplay : MonoBehaviour
{
    [SerializeField] private GameObject drillHint;
    [SerializeField] private float displayDuration = 5f;

    public void ShowTooltip()
    {
        if (!enabled)
            return;

        drillHint.SetActive(true);
        Destroy(drillHint, displayDuration);
        enabled = false;
    }
}
