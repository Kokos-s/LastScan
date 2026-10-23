using UnityEngine;

public class CaveTestShortcut : MonoBehaviour
{
    [SerializeField] private WorldSwitcher worldSwitcher;

    private void Update()
    {
        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (ctrl && shift && Input.GetKeyDown(KeyCode.H))
        {
            if (worldSwitcher.InCave)
                worldSwitcher.ExitCave();
            else
                worldSwitcher.EnterCave();
        }
    }
}