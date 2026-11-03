using UnityEngine;
using System.Collections;

public class VictoryManager : MonoBehaviour
{
    public static VictoryManager Instance;

    [SerializeField] private GameObject victoryScreen;
    [SerializeField] private AudioClip victoryMusic;

    private bool isShowing = false;
    private void Awake()
    {
        Instance = this;
        victoryScreen.SetActive(false);
    }

    public void ShowVictoryScreen()
    {
        if (isShowing) return;

        isShowing = true;
        StartCoroutine(VictorySequence());
    }
    private IEnumerator VictorySequence()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        AudioListener.pause = true;
        victoryScreen.SetActive(true);
        victoryScreen.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Audio_Manager.Instance.PlayMusic(victoryMusic);
        Time.timeScale = 0f;
    }
}


