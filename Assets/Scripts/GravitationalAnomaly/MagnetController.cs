using UnityEngine;

public class MagnetController : MonoBehaviour
{

    public GameObject gravitationalAnomaly;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(gravitationalAnomaly.transform.position.x, gravitationalAnomaly.transform.position.y, gravitationalAnomaly.transform.position.z);
    }
}