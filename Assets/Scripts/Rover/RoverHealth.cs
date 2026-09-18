using UnityEngine;
using UnityEngine.UI;
using TMPro; // Для работы с TextMeshPro

public class RoverHealth : MonoBehaviour
{
    [Header("Здоровье Rover")]
    public float maxHealth = 10f;
    public float currentHealth;
    public Slider healthSlider;

    [Header("Экран проигрыша")]
    [Tooltip("Ссылка на объект с текстом Game Over")]
    public GameObject gameOverTextObject;
    [Tooltip("Ссылка на скрипт управления/движения ровера")]
    public MonoBehaviour movementScript;

    [Header("Визуальные состояния (4 3D-модели)")]
    [Tooltip("Модель при здоровье > 90%")]
    public GameObject modelStage1;
    [Tooltip("Модель при здоровье 50% - 89%")]
    public GameObject modelStage2;
    [Tooltip("Модель при здоровье 1% - 49%")]
    public GameObject modelStage3;
    [Tooltip("Модель при 0% здоровья")]
    public GameObject modelStage4;

    public bool IsDead => currentHealth <= 0;

    private void Awake()
    {
        if (maxHealth <= 0) maxHealth = 10f;
        currentHealth = maxHealth;
    }

    private void Start()
    {
        InitSlider();
        UpdateVisualState();

        // Скрываем надпись Game Over в начале игры
        if (gameOverTextObject != null)
        {
            gameOverTextObject.SetActive(false);
        }
    }

    private void InitSlider()
    {
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        UpdateVisualState();

        if (IsDead)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        Debug.Log("💀 Rover's Health is 0. Game Over.");

        // 1. Показываем надпись Game Over
        if (gameOverTextObject != null)
        {
            gameOverTextObject.SetActive(true);
        }

        // 2. Отключаем скрипт движения ровера
        if (movementScript != null)
        {
            movementScript.enabled = false;
        }
    }

    private void UpdateVisualState()
    {
        float healthPercent = currentHealth / maxHealth;

        if (modelStage1 != null) modelStage1.SetActive(false);
        if (modelStage2 != null) modelStage2.SetActive(false);
        if (modelStage3 != null) modelStage3.SetActive(false);
        if (modelStage4 != null) modelStage4.SetActive(false);

        if (healthPercent > 0.90f)
        {
            if (modelStage1 != null) modelStage1.SetActive(true);
        }
        else if (healthPercent >= 0.50f)
        {
            if (modelStage2 != null) modelStage2.SetActive(true);
        }
        else if (healthPercent > 0f)
        {
            if (modelStage3 != null) modelStage3.SetActive(true);
        }
        else
        {
            if (modelStage4 != null) modelStage4.SetActive(true);
        }
    }
}