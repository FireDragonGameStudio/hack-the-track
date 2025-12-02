using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class DataServerManager : MonoBehaviour {

    // Event fired after all servers are started
    public UnityEvent OnServersStarted;
    public TextMeshProUGUI ServerStartedText;

    private void Awake() {
        DontDestroyOnLoad(gameObject); // Persist across scene loads

        Screen.SetResolution(1920, 1080, true);

#if UNITY_EDITOR
        // Register callback for exiting play mode
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
    }

    private IEnumerator Start() {
        string buildFolder = Application.isEditor ?
            @"C:\Users\marku\Unity\hack-the-track\HackTheTrack\Build\"
            : Path.GetDirectoryName(Application.dataPath);

        Application.OpenURL(Path.Combine(buildFolder, "leaderboard_server.exe"));
        Application.OpenURL(Path.Combine(buildFolder, "section_endurance_server.exe"));
        Application.OpenURL(Path.Combine(buildFolder, "telemetry_vehicle_server.exe"));

        ServerStartedText.text = "Starting Servers...\nLeaderboard Server started...\nEndurance Server started...\nTelemetry Server started...";

        yield return new WaitForSeconds(6f);
        OnServersStarted?.Invoke();
    }

    private void Update() {
        if (Keyboard.current.escapeKey.wasPressedThisFrame) {
            Application.Quit();
        }
    }

    private void OnApplicationQuit() {
        // Clean up processes when Unity quits
        StopServers();
    }

#if UNITY_EDITOR
    private void OnPlayModeStateChanged(PlayModeStateChange state) {
        if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode) {
            StopServers();
        }
    }
#endif

    private void StopServers() {
        TryKill("leaderboard_server");
        TryKill("section_endurance_server");
        TryKill("telemetry_vehicle_server");
    }

    private void TryKill(string processName) {
        try {
            // Fallback: kill processes by name
            foreach (var proc in Process.GetProcessesByName(processName)) {
                try {
                    proc.Kill();
                    UnityEngine.Debug.Log($"Killed orphaned process {processName}");
                } catch (Exception ex) {
                    UnityEngine.Debug.LogWarning($"Failed to kill {processName}: {ex.Message}");
                }
            }
        } catch (Exception ex) {
            UnityEngine.Debug.LogWarning($"Error killing {processName}: {ex.Message}");
        }
    }
}
