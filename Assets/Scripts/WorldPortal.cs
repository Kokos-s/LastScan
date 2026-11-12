using UnityEngine;

public class WorldPortal : MonoBehaviour
{
    [SerializeField] private bool enterCave = true;

    private WorldSwitcher switcher;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (switcher == null)
            switcher = FindFirstObjectByType<WorldSwitcher>();

        if (switcher == null) return;

        if (enterCave) switcher.EnterCave();
        else switcher.ExitCave();
    }
}