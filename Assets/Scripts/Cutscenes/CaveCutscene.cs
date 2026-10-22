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

    [Header("Пауза")]
    [SerializeField] private bool pauseGame = true;
    [SerializeField] private bool pauseAudio = false;   // true: заглушить и звуки игры на время ролика

    private bool isPlaying = false;
    private MonoBehaviour cameraScript;
    private Vector3 savedPosition;
    private Quaternion savedRotation;
    private float savedTimeScale = 1f;

    private void Awake()
    {
        brain.enabled = false;

        // Ролик и камера должны идти по реальному времени, иначе при timeScale = 0 они встанут
        director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
        brain.IgnoreTimeScale = true;

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

        // Страховка: если объект выключили посреди ролика, игра не должна остаться на паузе
        if (isPlaying)
        {
            Resume();
            SetScanPulsesVisible(true);
        }
    }

    private void Update()
    {
        if (isPlaying && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            director.Stop();
    }

    private void Pause()
    {
        if (!pauseGame) return;

        savedTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (pauseAudio)
            AudioListener.pause = true;
    }

    private void Resume()
    {
        isPlaying = false;

        if (pauseGame)
        {
            Time.timeScale = savedTimeScale;

            if (pauseAudio)
                AudioListener.pause = false;
        }
    }

    // Прячет или показывает сферы сканера, не останавливая сам скан
    private void SetScanPulsesVisible(bool visible)
    {
        foreach (ScanPulseEffect pulse in ScanPulseEffect.All)
        {
            if (pulse != null)
                pulse.SetVisible(visible);
        }
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

        SetScanPulsesVisible(false);
        Pause();
        director.Play();
    }

    private void OnStopped(PlayableDirector d)
    {
        Resume();
        SetScanPulsesVisible(true);

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