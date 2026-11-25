using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class VictoryManager : MonoBehaviour
{
    public static VictoryManager Instance;

    [System.Serializable]
    public class VictorySlide
    {
        public GameObject backgroundImage;

        [TextArea(3, 6)]
        public string description;

        public float displayDuration = 6f;
    }

    [SerializeField] private VictorySlide[] slides;
    [SerializeField] private string menuSceneName = "MainMenu";

    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private GameObject descriptionBackground;
    [SerializeField] private GameObject skipText;

    [SerializeField] private AudioSource narrationSource;
    [SerializeField] private AudioSource musicSource;

    [SerializeField] private GameObject victoryElementsRoot;
    [SerializeField] private GameObject transitionOverlay;
    [SerializeField] private CanvasGroup transitionGroup;
    [SerializeField] private float fadeDuration = 0.8f;
    [SerializeField] private float victoryDelay = 2f;

    private CanvasGroup[] imageGroups;

    [SerializeField] private bool enableTestShortcut = true;

    private bool isShowing = false;
    private bool skipRequested = false;
    private bool finalScreenActive = false;

    private void Awake()
    {
        Instance = this;
        victoryElementsRoot.SetActive(false);

        imageGroups = new CanvasGroup[slides.Length];

        for (int i = 0; i < slides.Length; i++)
        {
            CanvasGroup group = slides[i].backgroundImage.GetComponent<CanvasGroup>();

            if (group == null)
                group = slides[i].backgroundImage.AddComponent<CanvasGroup>();

            imageGroups[i] = group;
            group.alpha = 0f;

            slides[i].backgroundImage.SetActive(false);
        }

        skipText.SetActive(false);

        transitionOverlay.SetActive(false);
        transitionGroup.alpha = 1f;
    }

    private void Update()
    {
        if (enableTestShortcut && Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F))
        {
            ShowVictoryScreen();
        }

        if (isShowing && !finalScreenActive && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            skipRequested = true;
            skipText.SetActive(false);
        }
    }
    public void ShowVictoryScreen()
    {
        if (isShowing) return;

        isShowing = true;
        StartCoroutine(StartVictoryAfterDelay());
    }

    private IEnumerator StartVictoryAfterDelay()
    {
        yield return new WaitForSecondsRealtime(victoryDelay);

        StartCoroutine(VictorySequence());
    }
    private IEnumerator VictorySequence()
    {
        yield return new WaitForSecondsRealtime(0.2f);

        FindFirstObjectByType<SceneManager>().SetCinematicPlaying(true);

        AudioListener.pause = true;
        Time.timeScale = 0f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        victoryElementsRoot.SetActive(true);

        skipText.SetActive(true);

        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource.loop = true;
            musicSource.ignoreListenerPause = true;
            musicSource.Play();
        }

        if (narrationSource != null)
        {
            narrationSource.Stop();
            narrationSource.loop = false;
            narrationSource.ignoreListenerPause = true;
            narrationSource.Play();
        }

        for (int i = 0; i < slides.Length - 1; i++)
        {
            if (skipRequested)
                break;

            slides[i].backgroundImage.SetActive(true);

            descriptionText.gameObject.SetActive(true);
            descriptionBackground.SetActive(true);
            descriptionText.text = slides[i].description;

            yield return StartCoroutine(FadeImage(imageGroups[i], 0f, 1f));

            float elapsed = 0f;

            while (elapsed < slides[i].displayDuration && !skipRequested)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (skipRequested)
                break;

            yield return StartCoroutine(FadeImage(imageGroups[i], 1f, 0f));

            slides[i].backgroundImage.SetActive(false);
        }

        if (narrationSource != null)
            narrationSource.Stop();

        skipText.SetActive(false);

        ShowFinalScreen();

        int lastSlide = slides.Length - 1;

        FindFirstObjectByType<SceneManager>().SetCinematicPlaying(false);

        yield return StartCoroutine(FadeImage(imageGroups[lastSlide], 0f, 1f));
    }
    private void ShowFinalScreen()
    {
        for (int i = 0; i < slides.Length - 1; i++)
        {
            slides[i].backgroundImage.SetActive(false);
        }

        descriptionText.gameObject.SetActive(false);
        descriptionBackground.SetActive(false);
        skipText.SetActive(false);

        slides[slides.Length - 1].backgroundImage.SetActive(true);

        finalScreenActive = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private IEnumerator FadeImage(
        CanvasGroup group,
        float startAlpha,
        float endAlpha)
    {
        float elapsed = 0f;

        group.alpha = startAlpha;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            group.alpha = Mathf.Lerp(
                startAlpha,
                endAlpha,
                elapsed / fadeDuration
            );

            yield return null;
        }

        group.alpha = endAlpha;
    }
    public void ReturnToMenu()
    {
        if (musicSource != null)
            musicSource.Stop();

        Time.timeScale = 1f;
        AudioListener.pause = false;

        UnityEngine.SceneManagement.SceneManager.LoadScene(menuSceneName);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Application.Quit();
    }
}



