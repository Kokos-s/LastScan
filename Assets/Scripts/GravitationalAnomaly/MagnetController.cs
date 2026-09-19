using UnityEngine;

public class MagnetController : MonoBehaviour
{
    public GameObject gravitationalAnomaly;

    void Update()
    {
        transform.position = gravitationalAnomaly.transform.position;
    }
}