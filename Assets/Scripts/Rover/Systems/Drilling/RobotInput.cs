using UnityEngine;
using UnityEngine.InputSystem;

public class RobotInput : MonoBehaviour
{
    [SerializeField] private RoverHealth roverHealth;
    [SerializeField] private WreckOrePickup wreckOrePickup;

    private Animator animator;
    private RobotController controller;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<RobotController>();
    }

    private void Update()
    {
        if (roverHealth.IsDead)
            return;

        if (Keyboard.current == null)
            return;

        if (controller.IsBusy)
            return;

        if (Keyboard.current.eKey.isPressed && !wreckOrePickup.CanRecover && controller.GetCurrentMineral() != null)
        {
            controller.LockMineral();
            controller.SetBusy(true);
            animator.SetTrigger("Drill");
        }

        // Атака: левая кнопка мыши (F занята починкой на базе)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            animator.SetTrigger("Attack");
    }
}