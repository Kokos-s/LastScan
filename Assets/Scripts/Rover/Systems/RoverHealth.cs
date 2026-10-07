using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RoverHealth : MonoBehaviour, IDamageable
{
    public float maxHealth = 10f;
    public float currentHealth;
    public Image healthFill;

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
        if (healthFill != null)
        {
            healthFill.fillAmount = currentHealth / maxHealth;
        }

        UpdateVisualState();
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) 
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);

        if(healthFill != null)
        { 

            healthFill.fillAmount = currentHealth / maxHealth;
        }

        if (DamageVignette.Instance != null)
            DamageVignette.Instance.FlashDamage();

        UpdateVisualState();

        if (IsDead)
            GameOver();
    }

    private void GameOver()
    {
        if (movementScript != null)
            movementScript.enabled = false;
    }

    public void Repair(float amount)
    {
        if (IsDead)
            return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        if (healthFill != null)
            healthFill.fillAmount = currentHealth / maxHealth;

        UpdateVisualState();
    }

    public void Revive()
    {
        currentHealth = maxHealth;

        if (healthFill != null)
            healthFill.fillAmount = currentHealth / maxHealth;

        UpdateVisualState();

        if (movementScript != null)
            movementScript.enabled = true;
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