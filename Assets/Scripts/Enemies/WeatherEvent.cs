using System.Collections;
using UnityEngine;

public class WeatherEvent : MonoBehaviour
{
    [SerializeField] private MonoBehaviour[] effectScripts;

    [SerializeField] private float heightOffset = 0f;
    [SerializeField] private float moveSpeed = 5f;

    [SerializeField] private float formationTime = 5f;
    [SerializeField] private float minDuration = 30f;
    [SerializeField] private float maxDuration = 60f;

    private ParticleSystem[] particles;
    private Collider[] effectColliders;

    private Transform[] movementPoints;
    private Vector3 targetPosition;

    private WeatherManager weatherManager;

    private bool isActive = false;

    public bool IsActive
    {
        get { return isActive; }
    }

    public void Prepare()
    {
        particles = GetComponentsInChildren<ParticleSystem>(true);
        effectColliders = GetComponentsInChildren<Collider>(true);

        SetEffects(false);

        foreach (ParticleSystem system in particles)
            system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);

        gameObject.SetActive(false);
    }

    public void Begin(Vector3 position, Transform[] points, WeatherManager manager)
    {
        weatherManager = manager;
        movementPoints = points;
        position.y += heightOffset;
        transform.position = position;
        isActive = true;
        gameObject.SetActive(true);

        foreach (ParticleSystem system in particles)
        {
            system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            system.Play(false);
        }

        ChooseTarget();
        StartCoroutine(EventRoutine());
    }

    private void Update()
    {
        if (!isActive)
            return;

        Vector3 nextPosition = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (!weatherManager.CanMoveTo(this, nextPosition))
        {
            ChooseTarget();
            return;
        }

        transform.position = nextPosition;

        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
            ChooseTarget();
    }

    private void ChooseTarget()
    {
        int index = Random.Range(0, movementPoints.Length);
        targetPosition = movementPoints[index].position;
        targetPosition.y += heightOffset;
    }

    private IEnumerator EventRoutine()
    {
        yield return new WaitForSeconds(formationTime);
        SetEffects(true);
        float duration = Random.Range(minDuration, maxDuration);
        yield return new WaitForSeconds(duration);
        SetEffects(false);

        foreach (ParticleSystem system in particles)
        {
            system.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }

        while (HasLivingParticles())
            yield return null;

        isActive = false;
        gameObject.SetActive(false);
    }

    private void SetEffects(bool active)
    {
        foreach (MonoBehaviour effect in effectScripts)
            effect.enabled = active;

        foreach (Collider effectCollider in effectColliders)
            effectCollider.enabled = active;
    }

    private bool HasLivingParticles()
    {
        foreach (ParticleSystem system in particles)
            if (system.IsAlive(false))
                return true;

        return false;
    }
}