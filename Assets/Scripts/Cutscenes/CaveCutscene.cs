using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using Unity.Cinemachine;

public class CaveCutscene : MonoBehaviour
{
    [SerializeField] private GameMissions missions;
    [SerializeField] private PlayableDirector director;
    [SerializeField] private MissionCompletePanel missionCompletePanel;
    [SerializeField] private CaveBlockade blockade;
    [SerializeField] private CinemachineBrain brain;
    [SerializeField] private string cameraScriptName = "CameraFollow";
    [SerializeField] private MonoBehaviour[] disableDuringCutscene;
    [SerializeField] private GameObject[] hideDuringCutscene;

    private bool isPlaying = false;
    private MonoBehaviour cameraScript;
    private Vector3 savedPosition;
    private Quaternion savedRotation;

    private void Awake()
    {
        brain.enabled = false;

        Type type = Type.GetType(cameraScriptName + ", Assembly-CSharp");
        if (type != null)
            cameraScript = FindFirstObjectByType(type) as MonoBehaviour;

        if (cameraScript == null)
            Debug.LogWarning("CaveCutscene: скрипт камеры '" + cameraScriptName + "' не найден. Проверьте имя.");
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

        savedPosition = brain.transform.position;
        savedRotation = brain.transform.rotation;

        if (cameraScript != null)
            cameraScript.enabled = false;

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
        brain.transform.SetPositionAndRotation(savedPosition, savedRotation);

        if (cameraScript != null)
            cameraScript.enabled = true;

        foreach (var c in disableDuringCutscene)
            c.enabled = true;

        foreach (var g in hideDuringCutscene)
            g.SetActive(true);

        if (blockade != null)
            blockade.Clear();

        if (missionCompletePanel != null)
            missionCompletePanel.Show();
    }
}