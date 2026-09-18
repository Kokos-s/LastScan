using UnityEngine;

public class CloudMovement : MonoBehaviour
{
    [Header("Границы полета (Наш Куб)")]
    [Tooltip("Перетащите сюда ваш куб Terrain из Hierarchy")]
    public GameObject surfaceObject;

    [Header("Настройки полета")]
    [Tooltip("Постоянная высота полета")]
    public float height = 10f;

    [Tooltip("Скорость изменения траектории (чем больше, тем хаотичнее)")]
    public float volatility = 0.5f;

    [Tooltip("Скорость перемещения тучи")]
    public float moveSpeed = 2f;

    private Bounds surfaceBounds;
    private float offsetX;
    private float offsetZ;

    void Start()
    {
        if (surfaceObject == null)
        {
            Debug.LogError("❌ Пожалуйста, перетащите объект куба Terrain в поле Surface Object на скрипте CloudMovement!");
            enabled = false; // Отключаем скрипт, если нет поверхности
            return;
        }

        // Получаем границы куба для ограничения полета
        Renderer rend = surfaceObject.GetComponent<Renderer>();
        Collider col = surfaceObject.GetComponent<Collider>();
        if (rend != null)
        {
            surfaceBounds = rend.bounds;
        }
        else if (col != null)
        {
            surfaceBounds = col.bounds;
        }
        else
        {
            Debug.LogError("❌ На объекте поверхности нет Renderer или Collider!");
            enabled = false;
            return;
        }

        // Инициализируем случайные смещения для шума Перлина
        offsetX = Random.Range(0f, 1000f);
        offsetZ = Random.Range(0f, 1000f);
    }

    void Update()
    {
        // Вычисляем новые координаты на основе шума Перлина и времени
        float noiseX = Mathf.PerlinNoise(Time.time * volatility + offsetX, 0f);
        float noiseZ = Mathf.PerlinNoise(0f, Time.time * volatility + offsetZ);

        // Преобразуем шум (0..1) в координаты внутри границ куба
        float newX = Mathf.Lerp(surfaceBounds.min.x, surfaceBounds.max.x, noiseX);
        float newZ = Mathf.Lerp(surfaceBounds.min.z, surfaceBounds.max.z, noiseZ);

        // Плавное перемещение объекта к новой точке
        Vector3 targetPosition = new Vector3(newX, height, newZ);
        transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    // Визуализация границ полета в окне Scene
    void OnDrawGizmosSelected()
    {
        if (surfaceObject != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 center = new Vector3(surfaceBounds.center.x, height, surfaceBounds.center.z);
            Vector3 size = new Vector3(surfaceBounds.size.x, 0.1f, surfaceBounds.size.z);
            Gizmos.DrawWireCube(center, size);
        }
    }
}