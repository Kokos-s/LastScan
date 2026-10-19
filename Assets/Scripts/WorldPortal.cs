using UnityEngine;

public class WorldPortal : MonoBehaviour
{
    [SerializeField] private bool enterCave = true;   // true: вход в пещеру, false: выход из неё

    private WorldSwitcher switcher;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (switcher == null)
            switcher = FindFirstObjectByType<WorldSwitcher>();

        if (switcher == null)
        {
            Debug.LogError("WorldPortal: WorldSwitcher не найден в загруженных сценах.");
            return;
        }

        if (enterCave) switcher.EnterCave();
        else switcher.ExitCave();
    }
}