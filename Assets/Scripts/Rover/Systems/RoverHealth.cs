using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class RoverHealth : MonoBehaviour
{
    
    public float maxHealth = 10f;
    public float currentHealth;
    public Slider healthSlider;
        
    public GameObject gameOverTextObject;
    
    public MonoBehaviour movementScript;
            
    public GameObject modelStage1;
    public GameObject modelStage2;
    public GameObject modelStage3;
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
        Debug.Log("Rover's Health is 0. Game Over.");

        
        if (gameOverTextObject != null)
        {
            gameOverTextObject.SetActive(true);
        }

        
        if (movementScript != null)
        {
            movementScript.enabled = false;
        }
    }

    private void UpdateVisualState()
    {
        float healthPercent = currentHealth / maxHealth;
        GameObject activeModel;

        if (healthPercent > 0.90f)
            activeModel = modelStage1;
        else if (healthPercent >= 0.50f)
            activeModel = modelStage2;
        else if (healthPercent > 0f)
            activeModel = modelStage3;
        else
            activeModel = modelStage4;

        if (modelStage1 != null)
            modelStage1.SetActive(modelStage1 == activeModel);

        if (modelStage2 != null)
            modelStage2.SetActive(modelStage2 == activeModel);

        if (modelStage3 != null)
            modelStage3.SetActive(modelStage3 == activeModel);

        if (modelStage4 != null)
            modelStage4.SetActive(modelStage4 == activeModel);
    }
}