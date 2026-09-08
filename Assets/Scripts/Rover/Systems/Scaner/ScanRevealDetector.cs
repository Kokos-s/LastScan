using UnityEngine;

public class ScanRevealDetector : MonoBehaviour
{
    private ScanReveal[] targets;

    public void SetTargets(ScanReveal[] scannerTargets)
    {
        targets = new ScanReveal[scannerTargets.Length];

        for (int i = 0; i < scannerTargets.Length; i++)
        {
            targets[i] = scannerTargets[i];
        }
    }

    void LateUpdate()
    {
        if (targets == null)
            return;

        float radius = transform.lossyScale.x / 2f;

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null)
            {
                float distance = Vector3.Distance(
                    transform.position,
                    targets[i].transform.position
                );

                if (distance <= radius)
                {
                    targets[i].Reveal();
                    targets[i] = null;
                }
            }
        }
    }
}