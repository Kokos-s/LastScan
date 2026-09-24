using UnityEngine;
using TMPro;

public class DeliveryHintDisplay : MonoBehaviour
{
    [SerializeField] private RoverInventory inventory;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private float displayDuration = 5f;

    private void Awake()
    {
        label.enabled = false;
    }

    private void Update()
    {
        if (inventory.CollectedOre > 0)
        {
            label.enabled = true;
            Destroy(gameObject, displayDuration);
            enabled = false;
        }
    }
}