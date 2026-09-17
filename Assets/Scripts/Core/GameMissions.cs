using UnityEngine;

public class GameMissions : MonoBehaviour
{
    [SerializeField] private int targetOreAmount = 50;

    public int TargetOreAmount
    {
        get { return targetOreAmount; }
    }
}