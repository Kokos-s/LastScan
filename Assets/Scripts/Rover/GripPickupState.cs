using UnityEngine;

public class GripPickupState : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var arm = animator.GetComponent<RobotArmReach>();
        if (arm != null)
            arm.BeginPickup();
    }
}