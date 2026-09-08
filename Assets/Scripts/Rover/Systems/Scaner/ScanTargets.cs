using UnityEngine;

public class ScanTargets : MonoBehaviour
{
    [SerializeField] private ScanReveal[] targets;

    public ScanReveal[] Targets
    {
        get { return targets; }
    }
}
