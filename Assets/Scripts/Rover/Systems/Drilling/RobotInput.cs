using UnityEngine;
using UnityEngine.InputSystem;

public class RobotInput : MonoBehaviour
{
    [SerializeField] private RoverHealth roverHealth;
    [SerializeField] private WreckOrePickup wreckOrePickup;

    
    [SerializeField] private float attackHitTime = 0.35f; 
    [SerializeField] private float inputBuffer = 0.4f;    

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

        
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            bufferedUntil = Time.time + inputBuffer;

        
        if (Time.time < bufferedUntil && Time.time >= nextAttackTime)
        {
            bufferedUntil = 0f;
            nextAttackTime = Time.time + attackHitTime;
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");
        }
    }
}