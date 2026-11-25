using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class SceneManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject controlsMenu;
    public GameObject creditsMenu;
    public GameObject pauseMenu;
    public GameObject pauseBackground;

    private bool isPaused = false;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenu == null || isPaused) return;

            PauseGame();
        }
    }
    public void OpenMainMenu()
    {
        mainMenu.SetActive(true);
        controlsMenu.SetActive(false);
        creditsMenu.SetActive(false);
    }
    public void OpenControlsMenu()
    {
        mainMenu.SetActive(false);
        controlsMenu.SetActive(true);
        creditsMenu.SetActive(false);
    }
    public void OpenCreditsMenu()
    {
        mainMenu.SetActive(false);
        controlsMenu.SetActive(false);
        creditsMenu.SetActive(true);
    }
    public void PauseGame()
    {
        pauseBackground.SetActive(true);
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void ResumeGame()
    {
        pauseBackground.SetActive(false);
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void PlayScene(string name)
    {
        AudioListener.pause = false;
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(name);
    }
    public void OpenPauseControls()
    {
        pauseMenu.SetActive(false);
        controlsMenu.SetActive(true);
    }

    public void BackToPauseMenu()
    {
        controlsMenu.SetActive(false);
        pauseMenu.SetActive(true);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
