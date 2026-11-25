using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class IntroductionManager : MonoBehaviour
{
    [System.Serializable]
    public class IntroductionSlide
    {
        public GameObject backgroundImage;

        [TextArea(3, 6)]
        public string description;

        public float displayDuration = 6f;
    }

    [SerializeField] private IntroductionSlide[] slides;

    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private GameObject descriptionBackground;

    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject skipText;

    [SerializeField] private AudioSource narrationSource;
    [SerializeField] private AudioSource staticSource;

    [SerializeField] private GameObject introElementsRoot;
    [SerializeField] private GameObject transitionOverlay;
    [SerializeField] private CanvasGroup transitionGroup;
    [SerializeField] private float transitionDuration = 1f;

    [SerializeField] private float fadeDuration = 0.8f;

    private CanvasGroup[] imageGroups;

    private bool skipRequested;
    private bool controlsScreenActive;
    private bool gameStarting;
    private void Start()
    {
        AudioListener.pause = false;
        Time.timeScale = 0f;

        imageGroups = new CanvasGroup[slides.Length];

        for (int i = 0; i < slides.Length; i++)
        {
            CanvasGroup group = slides[i].backgroundImage
                .GetComponent<CanvasGroup>();

            if (group == null)
                group = slides[i].backgroundImage
                    .AddComponent<CanvasGroup>();

            imageGroups[i] = group;

            group.alpha = 0f;
            slides[i].backgroundImage.SetActive(false);
        }

        descriptionText.gameObject.SetActive(true);
        descriptionBackground.SetActive(true);
        startButton.SetActive(false);
        skipText.SetActive(true);

        transitionGroup.alpha = 0f;
        transitionOverlay.SetActive(false);

        staticSource.loop = true;
        staticSource.Play();

        if (narrationSource != null)
            narrationSource.Play();

        StartCoroutine(PlayIntroduction());
    }

    private void Update()
    {
        if (!controlsScreenActive && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            skipText.SetActive(false);
            skipRequested = true;
        }
    }
    private IEnumerator PlayIntroduction()
    {
        int controlsIndex = slides.Length - 1;

        FindFirstObjectByType<SceneManager>().SetCinematicPlaying(true);

        for (int i = 0; i < controlsIndex; i++)
        {
            if (skipRequested)
                break;

            slides[i].backgroundImage.SetActive(true);

            descriptionText.gameObject.SetActive(true);
            descriptionBackground.SetActive(true);

            descriptionText.text = slides[i].description;
            descriptionText.maxVisibleCharacters = 0;

            yield return StartCoroutine (FadeImage(imageGroups[i], 0f, 1f));

            descriptionText.text = slides[i].description;
            descriptionText.maxVisibleCharacters = int.MaxValue;

            if (skipRequested)
                break;

            if (skipRequested)
                break;

            yield return StartCoroutine (WaitForSlideDuration(slides[i].displayDuration));

            if (skipRequested)
                break;

            yield return StartCoroutine (FadeImage(imageGroups[i], 1f, 0f));

            slides[i].backgroundImage.SetActive(false);
        }

        if (narrationSource != null)
            narrationSource.Stop();

        FindFirstObjectByType<SceneManager>().SetCinematicPlaying(false);

        ShowControlsScreen();
    }

    private void ShowControlsScreen()
    {
        controlsScreenActive = true;
        skipText.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        for (int i = 0; i < slides.Length; i++)
        {
            slides[i].backgroundImage.SetActive(false);
        }

        int controlsIndex = slides.Length - 1;

        slides[controlsIndex].backgroundImage.SetActive(true);
        imageGroups[controlsIndex].alpha = 1f;

        descriptionText.gameObject.SetActive(false);
        descriptionBackground.SetActive(false);

        startButton.SetActive(true);
    }

    private IEnumerator WaitForSlideDuration(float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (skipRequested)
                yield break;

            elapsedTime += Time.unscaledDeltaTime;

            yield return null;
        }
    }
    private IEnumerator FadeImage(CanvasGroup group, float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;

        group.alpha = startAlpha;

        while (elapsedTime < fadeDuration)
        {
            if (skipRequested)
                yield break;

            elapsedTime += Time.unscaledDeltaTime;

            group.alpha = Mathf.Lerp(
                startAlpha,
                endAlpha,
                elapsedTime / fadeDuration
            );

            yield return null;
        }

        group.alpha = endAlpha;
    }

    public void StartGame()
    {
        if (!gameStarting)
            StartCoroutine(StartGameTransition());
    }

    private IEnumerator StartGameTransition()
    {
        gameStarting = true;

        if (narrationSource != null)
            narrationSource.Stop();

        if (staticSource != null)
            staticSource.Stop();

        startButton.SetActive(false);

        transitionOverlay.SetActive(true);

        yield return StartCoroutine(
            FadeTransition(0f, 1f)
        );

        yield return new WaitForSecondsRealtime(0.3f);

        introElementsRoot.SetActive(false);

        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        yield return StartCoroutine(
            FadeTransition(1f, 0f)
        );

        transitionOverlay.SetActive(false);
    }
    private IEnumerator FadeTransition(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;

        transitionGroup.alpha = startAlpha;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            transitionGroup.alpha = Mathf.Lerp (startAlpha,endAlpha, elapsedTime / transitionDuration);
            yield return null;
        }

        transitionGroup.alpha = endAlpha;
    }
}

