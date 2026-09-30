using UnityEngine;
using TMPro;

public class MissionCompletePanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private GameObject missionPanel;
    [SerializeField] private string message = "Mission finished!";

    private void Awake()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void Show()
    {
        if (missionPanel != null)
            missionPanel.SetActive(false);

        if (panelRoot != null)
            panelRoot.SetActive(true);

        if (messageText != null)
            messageText.text = message;
    }

    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }
}