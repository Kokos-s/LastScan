using UnityEngine;
using UnityEngine.InputSystem;

public class RobotInput : MonoBehaviour
{
    [SerializeField] private RoverHealth roverHealth;
    [SerializeField] private WreckOrePickup wreckOrePickup;

    [Header("Attack")]
    [SerializeField] private float attackHitTime = 0.35f; // чуть позже кадра с событием DealDamage
    [SerializeField] private float inputBuffer = 0.4f;    // сколько секунд клик помнится

    private Animator animator;
    private RobotController controller;
    private float nextAttackTime;
    private float bufferedUntil;

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

        // Запоминаем клик
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            bufferedUntil = Time.time + inputBuffer;

        // Начинаем удар, как только прошло окно отмены предыдущего
        if (Time.time < bufferedUntil && Time.time >= nextAttackTime)
        {
            bufferedUntil = 0f;
            nextAttackTime = Time.time + attackHitTime;
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");
        }
    }
}