using UnityEngine;

public class SceneTrigger : MonoBehaviour
{
    [SerializeField] private string sceneName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}