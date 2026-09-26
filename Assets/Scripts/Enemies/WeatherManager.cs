using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    [SerializeField] private Transform rover;
    [SerializeField] private Transform[] weatherPoints;
    [SerializeField] private WeatherEvent[] weatherEvents;

    [SerializeField] private float minSpawnInterval = 20f;
    [SerializeField] private float maxSpawnInterval = 40f;

    [SerializeField] private float minDistanceFromRover = 150f;
    [SerializeField] private float minDistanceBetweenEvents = 200f;

    private float spawnTimer;

    private void Start()
    {
        foreach (WeatherEvent weather in weatherEvents)
        {
            weather.Prepare();
        }

        spawnTimer = Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    private void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer > 0f)
            return;

        WeatherEvent weather = GetRandomAvailableEvent();
        Transform point = GetRandomAvailablePoint();

        if (weather == null || point == null)
        {
            spawnTimer = 2f;
            return;
        }

        weather.Begin(point.position, weatherPoints, this);

        spawnTimer = Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    private WeatherEvent GetRandomAvailableEvent()
    {
        List<WeatherEvent> availableEvents = new List<WeatherEvent>();

        foreach (WeatherEvent weather in weatherEvents)
        {
            if (!weather.IsActive)
                availableEvents.Add(weather);
        }

        if (availableEvents.Count == 0)
            return null;

        int index = Random.Range(0, availableEvents.Count);
        return availableEvents[index];
    }

    private Transform GetRandomAvailablePoint()
    {
        List<Transform> availablePoints = new List<Transform>();

        foreach (Transform point in weatherPoints)
        {
            if (IsPointAvailable(point.position))
                availablePoints.Add(point);
        }

        if (availablePoints.Count == 0)
            return null;

        int index = Random.Range(0, availablePoints.Count);
        return availablePoints[index];
    }

    private bool IsPointAvailable(Vector3 position)
    {
        if (GetGroundDistance(position, rover.position) < minDistanceFromRover)
            return false;

        foreach (WeatherEvent weather in weatherEvents)
        {
            if (!weather.IsActive)
                continue;

            if (GetGroundDistance(position, weather.transform.position) < minDistanceBetweenEvents)
                return false;
        }

        return true;
    }

    private float GetGroundDistance(Vector3 first, Vector3 second)
    {
        first.y = 0f;
        second.y = 0f;

        return Vector3.Distance(first, second);
    }

    public bool CanMoveTo(WeatherEvent movingWeather, Vector3 nextPosition)
    {
        foreach (WeatherEvent weather in weatherEvents)
        {
            if (weather == movingWeather || !weather.IsActive)
                continue;

            float distance = GetGroundDistance(
                nextPosition,
                weather.transform.position
            );

            if (distance < minDistanceBetweenEvents)
                return false;
        }

        return true;
    }
}