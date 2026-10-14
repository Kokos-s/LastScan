using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using Unity.Cinemachine;

public class CaveCutscene : MonoBehaviour
{
    [SerializeField] private GameMissions missions;
    [SerializeField] private PlayableDirector director;
    [SerializeField] private MissionCompletePanel missionCompletePanel;
    [SerializeField] private CinemachineBrain brain;
    [SerializeField] private MonoBehaviour[] disableDuringCutscene;
    [SerializeField] private GameObject[] hideDuringCutscene;

    private bool isPlaying = false;
    private Vector3 savedLocalPosition;
    private Quaternion savedLocalRotation;

    private void Awake()
    {
        brain.enabled = false;
    }

    private void OnEnable()
    {
        missions.OnMissionCompleted += Play;
        director.stopped += OnStopped;
    }

    private void OnDisable()
    {
        missions.OnMissionCompleted -= Play;
        director.stopped -= OnStopped;
    }

    private void Update()
    {
        if (isPlaying && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            director.Stop();
    }

    private void Play()
    {
        isPlaying = true;

        savedLocalPosition = brain.transform.localPosition;
        savedLocalRotation = brain.transform.localRotation;
        brain.enabled = true;

        foreach (var c in disableDuringCutscene)
            c.enabled = false;

        foreach (var g in hideDuringCutscene)
            g.SetActive(false);

        director.Play();
    }

    private void OnStopped(PlayableDirector d)
    {
        isPlaying = false;

        brain.enabled = false;
        brain.transform.localPosition = savedLocalPosition;
        brain.transform.localRotation = savedLocalRotation;

        foreach (var c in disableDuringCutscene)
            c.enabled = true;

        foreach (var g in hideDuringCutscene)
            g.SetActive(true);

        if (missionCompletePanel != null)
            missionCompletePanel.Show();
    }
}