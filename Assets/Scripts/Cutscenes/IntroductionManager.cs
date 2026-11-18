using UnityEngine;
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
    [SerializeField] private float fadeDuration = 0.8f;
    [SerializeField] private float typingSpeed = 0.04f;

    private CanvasGroup[] imageGroups;

    private void Start()
    {
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

        StartCoroutine(PlayIntroduction());
    }
    private IEnumerator PlayIntroduction()
    {
        for (int i = 0; i < slides.Length; i++)
        {
            slides[i].backgroundImage.SetActive(true);

            descriptionText.text = slides[i].description;
            descriptionText.maxVisibleCharacters = 0;

            yield return StartCoroutine(
                FadeImage(imageGroups[i], 0f, 1f)
            );

            yield return StartCoroutine(
                TypeText(slides[i].description)
            );

            yield return new WaitForSecondsRealtime(
                slides[i].displayDuration
            );

            yield return StartCoroutine(
                FadeImage(imageGroups[i], 1f, 0f)
            );

            slides[i].backgroundImage.SetActive(false);
        }

        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
    private IEnumerator TypeText(string text)
    {
        descriptionText.text = "";
        descriptionText.maxVisibleCharacters = 0;

        descriptionText.text = text;

        for (int i = 0; i <= text.Length; i++)
        {
            descriptionText.maxVisibleCharacters = i;

            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        descriptionText.maxVisibleCharacters = int.MaxValue;
    }
    private IEnumerator FadeImage(
        CanvasGroup group,
        float startAlpha,
        float endAlpha)
    {
        float elapsedTime = 0f;

        group.alpha = startAlpha;

        while (elapsedTime < fadeDuration)
        {
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
}
