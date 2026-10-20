using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

public class WorldSwitcher : MonoBehaviour
{
    [Header("Основная сцена")]
    [SerializeField] private GameObject surfaceWorld;
    [SerializeField] private Transform returnPoint;       // CaveReturnPoint

    [Header("Пещера")]
    [SerializeField] private string caveSceneName = "Boss_Cave";
    [SerializeField] private string caveRootName = "CaveWorld";
    [SerializeField] private string caveSpawnName = "CaveSpawnPoint";

    [Header("Затемнение (необязательно)")]
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeTime = 0.4f;

    private GameObject caveWorld;
    private Transform caveSpawn;
    private bool busy = false;
    private bool inCave = false;

    public bool InCave => inCave;

    // Точка появления в пещере (null, пока пещера не загружена)
    public Transform CaveSpawn => caveSpawn;

    // Корень мира, в котором сейчас находится игрок
    public Transform CurrentWorldRoot
    {
        get
        {
            if (inCave && caveWorld != null) return caveWorld.transform;
            return surfaceWorld != null ? surfaceWorld.transform : null;
        }
    }

    [ContextMenu("Test Enter Cave")]
    public void EnterCave()
    {
        if (busy || inCave) return;
        StartCoroutine(EnterRoutine());
    }

    [ContextMenu("Test Exit Cave")]
    public void ExitCave()
    {
        if (busy || !inCave) return;
        StartCoroutine(ExitRoutine());
    }

    private IEnumerator EnterRoutine()
    {
        busy = true;
        yield return Fade(1f);

        Scene caveScene = USceneManager.GetSceneByName(caveSceneName);

        if (!caveScene.isLoaded)
        {
            AsyncOperation op = USceneManager.LoadSceneAsync(caveSceneName, LoadSceneMode.Additive);
            while (!op.isDone)
                yield return null;

            caveScene = USceneManager.GetSceneByName(caveSceneName);
        }

        if (caveWorld == null)
            FindCaveObjects(caveScene);

        if (caveWorld == null || caveSpawn == null)
        {
            Debug.LogError("WorldSwitcher: в сцене '" + caveSceneName + "' не найден объект '" + caveRootName
                           + "' или '" + caveSpawnName + "' внутри него.");
            busy = false;
            yield return Fade(0f);
            yield break;
        }

        caveWorld.SetActive(true);
        surfaceWorld.SetActive(false);
        USceneManager.SetActiveScene(caveScene);
        TeleportRover(caveSpawn);

        inCave = true;
        yield return Fade(0f);
        busy = false;
    }

    private IEnumerator ExitRoutine()
    {
        busy = true;
        yield return Fade(1f);

        surfaceWorld.SetActive(true);
        caveWorld.SetActive(false);
        USceneManager.SetActiveScene(gameObject.scene);
        TeleportRover(returnPoint);

        inCave = false;
        yield return Fade(0f);
        busy = false;
    }

    private void FindCaveObjects(Scene caveScene)
    {
        foreach (GameObject root in caveScene.GetRootGameObjects())
        {
            if (root.name == caveRootName)
            {
                caveWorld = root;
                Transform spawn = root.transform.Find(caveSpawnName);
                if (spawn != null) caveSpawn = spawn;
                break;
            }
        }
    }

    private void TeleportRover(Transform point)
    {
        GameObject rover = GameObject.FindGameObjectWithTag("Player");
        if (rover == null)
        {
            Debug.LogError("WorldSwitcher: объект с тегом Player не найден.");
            return;
        }

        CharacterController cc = rover.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        Rigidbody rb = rover.GetComponent<Rigidbody>();
        if (rb == null) rb = rover.GetComponentInParent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        rover.transform.SetPositionAndRotation(point.position, point.rotation);
        Physics.SyncTransforms();

        if (cc != null) cc.enabled = true;
    }

    private IEnumerator Fade(float target)
    {
        if (fadeGroup == null) yield break;

        float start = fadeGroup.alpha;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            fadeGroup.alpha = Mathf.Lerp(start, target, t / fadeTime);
            yield return null;
        }
        fadeGroup.alpha = target;
    }
}