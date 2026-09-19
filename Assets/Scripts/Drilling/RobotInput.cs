using UnityEngine;
using UnityEngine.InputSystem;

public class RobotInput : MonoBehaviour
{
    private Animator animator;
    private RobotController controller;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<RobotController>();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame && controller.GetCurrentMineral() != null)
        {
            animator.SetTrigger("Drill");
        }

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            animator.SetTrigger("Attack");
        }
    }
}