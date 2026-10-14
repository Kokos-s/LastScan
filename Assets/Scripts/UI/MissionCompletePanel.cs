using System.Collections;
using UnityEngine;
using TMPro;

public class MissionCompletePanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private GameObject missionPanel;
    [SerializeField] private string message = "Mission finished!";
    [SerializeField] private float autoHideDelay = 6f;

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

        if (autoHideDelay > 0f)
        {
            StopAllCoroutines();
            StartCoroutine(HideAfterDelay());
        }
    }

    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(autoHideDelay);
        Hide();
    }
}