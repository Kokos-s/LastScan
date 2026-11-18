using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class RobotArmReach : MonoBehaviour
{
    [SerializeField] private TwoBoneIKConstraint ik;
    [SerializeField] private Transform ikTarget;
    [SerializeField] private Transform tip;          
    [SerializeField] private Transform gripPoint;    
    [SerializeField] private RobotController controller;
    [SerializeField] private RobotFingers fingers;

    [SerializeField] private float weightInSpeed = 4f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float arriveDistance = 0.05f;
    [SerializeField] private float reachTimeout = 2f;
    [SerializeField] private float fadeOutTime = 0.25f;
    [SerializeField] private float closeWaitTimeout = 0.6f;
    [SerializeField] private float fingerHoldTime = 1f;   

    private Animator animator;
    private Coroutine routine;
    private Vector3 gripLocalOffset;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (controller == null)
            controller = GetComponent<RobotController>();

        gripLocalOffset = tip.InverseTransformPoint(gripPoint.position);
        ik.weight = 0f;
    }

    private void OnDisable()
    {
        ik.weight = 0f;
        routine = null;
    }

    public void BeginPickup()
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(PickupRoutine());
    }

    private IEnumerator PickupRoutine()
    {
        animator.ResetTrigger("PickupDone");

        var mineral = controller.GetLockedMineral();
        if (mineral == null)
        {
            animator.SetTrigger("PickupDone");
            routine = null;
            yield break;
        }

        ikTarget.position = tip.position;

        float elapsed = 0f;

        while (elapsed < reachTimeout)
        {
            elapsed += Time.deltaTime;

            ik.weight = Mathf.MoveTowards(ik.weight, 1f, weightInSpeed * Time.deltaTime);

            Vector3 offsetWorld = tip.TransformVector(gripLocalOffset);
            Vector3 desired = mineral.transform.position - offsetWorld;

            ikTarget.position = Vector3.MoveTowards(
                ikTarget.position, desired, moveSpeed * Time.deltaTime);

            bool close = Vector3.Distance(gripPoint.position, mineral.transform.position) <= arriveDistance;

            if (close && mineral.RiseFinished)
                break;

            yield return null;
        }

        if (fingers != null)
        {
            fingers.Close();

            float waited = 0f;
            while (!fingers.IsClosed && waited < closeWaitTimeout)
            {
                waited += Time.deltaTime;
                yield return null;
            }
        }

        mineral.AttachTo(gripPoint);
        animator.SetTrigger("PickupDone");

        float startWeight = ik.weight;
        float t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            ik.weight = Mathf.Lerp(startWeight, 0f, t / fadeOutTime);
            yield return null;
        }

        ik.weight = 0f;

        
        if (fingers != null)
        {
            yield return new WaitForSeconds(fingerHoldTime);
            fingers.Release();
        }

        routine = null;
    }
}