using UnityEngine;

public class ControlsHintDisplay : MonoBehaviour
{
    [SerializeField] private GameObject hintLabel;
    [SerializeField] private float displayDuration = 5f;
    private float timer;

    void Start()
    {
        timer = displayDuration;
    }

    void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
                hintLabel.SetActive(false);
        }
    }
}
