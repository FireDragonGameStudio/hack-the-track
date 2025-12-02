using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;

public class TelemetryVehicleSelector : MonoBehaviour {

    public TelemetryUI TelemetryDisplay;

    public CinemachineCamera[] CinemachineCams;
    public string[] CinemachineCamsLabels;

    [Header("Player Controls")]
    [SerializeField] private bool isPlayerCarActive;
    [SerializeField] private GameObject playerCarObject;
    [SerializeField] private CinemachineCamera playerCarFollowCamera;
    [SerializeField] private CinemachineCamera playerCarCockpitCamera;
    [SerializeField] private GameObject playerSpeedText;
    [SerializeField] private GameObject playerLapTimesParent;
    [SerializeField] private TextMeshProUGUI playerBestTimeText;
    [SerializeField] private TextMeshProUGUI playerCurrentTimeText;
    [SerializeField] private TextMeshProUGUI playerSector3TimeText;
    [SerializeField] private TextMeshProUGUI playerSector2TimeText;
    [SerializeField] private TextMeshProUGUI playerSector1TimeText;

    private GameObject playerCarObjectRef;
    private float currentLapTime;
    private float currentSectorTime;
    private float bestLapTime;
    private bool startTimeRecording;

    private List<TelemetryVehiclePlayer> vehicles = new List<TelemetryVehiclePlayer>();
    private int currentVehicleIndex = 0;
    private int currentCameraIndex = 0;

    private PrometeoCarController carController = null;

    private void Start() {
        for (int i = 0; i < CinemachineCams.Length; i++) {
            CinemachineCams[i].gameObject.SetActive(i == currentCameraIndex);
            CinemachineCams[i].Target.TrackingTarget = vehicles[currentVehicleIndex].GetCameraTarget();
        }
        TelemetryDisplay.CurrentActiveCameraValue.text = CinemachineCamsLabels[currentCameraIndex];

        playerCarFollowCamera.gameObject.SetActive(isPlayerCarActive);
        playerCarCockpitCamera.gameObject.SetActive(isPlayerCarActive);
        playerSpeedText.SetActive(isPlayerCarActive);
        playerLapTimesParent.SetActive(isPlayerCarActive);
    }

    private void Update() {

        if (Keyboard.current.xKey.wasPressedThisFrame) {
            SpawnDespawnPlayerCar();
        }

        if (!isPlayerCarActive && playerCarObjectRef) {
            Destroy(playerCarObjectRef);
            carController = null;

            for (int i = 0; i < CinemachineCams.Length; i++) {
                CinemachineCams[i].gameObject.SetActive(i == currentCameraIndex);
            }

            playerCarCockpitCamera.gameObject.SetActive(isPlayerCarActive);
            playerCarFollowCamera.gameObject.SetActive(isPlayerCarActive);
            playerSpeedText.SetActive(isPlayerCarActive);
            playerLapTimesParent.SetActive(isPlayerCarActive);

            currentLapTime = 0;
            currentSectorTime = 0;
        }

        if (isPlayerCarActive &&
            (Keyboard.current.cKey.wasPressedThisFrame || Keyboard.current.vKey.wasPressedThisFrame)) {
            playerCarCockpitCamera.gameObject.SetActive(!playerCarCockpitCamera.gameObject.activeSelf);
            playerCarFollowCamera.gameObject.SetActive(!playerCarFollowCamera.gameObject.activeSelf);
        }

        if (isPlayerCarActive && (Keyboard.current.ctrlKey.wasPressedThisFrame || Keyboard.current.ctrlKey.wasReleasedThisFrame)) {
            var carFollow = playerCarFollowCamera.gameObject.GetComponent<CinemachineFollow>();
            carFollow.FollowOffset = new Vector3(carFollow.FollowOffset.x, carFollow.FollowOffset.y, carFollow.FollowOffset.z * (-1));
        }

        if (isPlayerCarActive && !playerCarObjectRef) {
            playerCarObjectRef = Instantiate(playerCarObject, Vector3.zero, Quaternion.identity);
            carController = playerCarObjectRef.GetComponentInChildren<PrometeoCarController>();
            carController.SetVehicleSelectorReference(this);
            carController.carEngineSound.Play();
            carController.carSpeedText = playerSpeedText.GetComponentInChildren<Text>();

            for (int i = 0; i < CinemachineCams.Length; i++) {
                CinemachineCams[i].gameObject.SetActive(false);
            }

            playerCarCockpitCamera.Target.TrackingTarget = carController.transform;
            playerCarFollowCamera.Target.TrackingTarget = carController.transform;
            playerCarFollowCamera.gameObject.SetActive(isPlayerCarActive);
            playerSpeedText.SetActive(isPlayerCarActive);
            playerLapTimesParent.SetActive(isPlayerCarActive);
        }

        if (startTimeRecording) {
            currentSectorTime += Time.deltaTime;
            currentLapTime += Time.deltaTime;
            var timeSpan = TimeSpan.FromSeconds(currentLapTime);
            playerCurrentTimeText.text = timeSpan.ToString("mm\\:ss\\.fff");
        }

        if (Keyboard.current.rKey.wasPressedThisFrame && vehicles.Count > 0) {
            SelectNextCar();
        }

        if (currentVehicleIndex >= 0) {
            vehicles[currentVehicleIndex].UpdateUIValues();
        }

        if (isPlayerCarActive) {
            return;
        }

        if (Keyboard.current.cKey.wasPressedThisFrame && vehicles.Count > 0) {
            SelectNextCamera();
        }

        if (Keyboard.current.vKey.wasPressedThisFrame && vehicles.Count > 0) {
            SelectPreviousCamera();
        }
    }

    public void SpawnDespawnPlayerCar() {
        isPlayerCarActive = !isPlayerCarActive;
    }

    public void RemoteChangeCarActiveCamera() {
        if (isPlayerCarActive) {
            playerCarCockpitCamera.gameObject.SetActive(!playerCarCockpitCamera.gameObject.activeSelf);
            playerCarFollowCamera.gameObject.SetActive(!playerCarFollowCamera.gameObject.activeSelf);
        }
    }

    public void SelectNextCar() {
        currentVehicleIndex = (currentVehicleIndex + 1) % vehicles.Count;

        foreach (var cinemachineCam in CinemachineCams) {
            cinemachineCam.Target.TrackingTarget = vehicles[currentVehicleIndex].GetCameraTarget();
        }
    }

    public void SelectNextCamera() {
        SelectNextOrPreviousCamera(true);
    }

    public void SelectPreviousCamera() {
        SelectNextOrPreviousCamera(false);
    }

    public void Acceleration(bool isAccelerating) {
        if (carController != null) {
            carController.Accelerate = isAccelerating;
        }
    }

    public void Brake(bool isBraking) {
        if (carController != null) {
            carController.Brake = isBraking;
        }
    }

    public void Left(bool isSteeringLeft) {
        if (carController != null) {
            carController.Left = isSteeringLeft;
        }
    }

    public void Right(bool isSteeringRight) {
        if (carController != null) {
            carController.Right = isSteeringRight;
        }
    }

    private void SelectNextOrPreviousCamera(bool goToNextCamera) {
        currentCameraIndex += goToNextCamera ? 1 : -1;

        if (currentCameraIndex < 0) {
            currentCameraIndex = CinemachineCams.Length - 1;
        } else {
            currentCameraIndex %= CinemachineCams.Length;
        }

        TelemetryDisplay.CurrentActiveCameraValue.text = CinemachineCamsLabels[currentCameraIndex];

        for (int i = 0; i < CinemachineCams.Length; i++) {
            CinemachineCams[i].gameObject.SetActive(i == currentCameraIndex);
        }
    }

    public void RegisterVehicle(TelemetryVehiclePlayer vehicle) {
        if (!vehicles.Contains(vehicle)) {
            vehicles.Add(vehicle);

            // First registered vehicle becomes active
            if (vehicles.Count == 1) {
                currentVehicleIndex = 0;
            }
        }
    }

    public TelemetryVehiclePlayer GetCurrentSelectedVehicle() {
        return currentVehicleIndex < 0 ? null : vehicles[currentVehicleIndex];
    }

    public string GetCurrentlySelectedVehicleId() {
        return vehicles[currentVehicleIndex].VehicleId;
    }

    public string ExtractCarNumber(string vehicleId) {
        // Match last numeric segment, e.g. GR86-004-78 -> 78
        var match = Regex.Match(vehicleId, @"(\d+)$");
        return match.Success ? match.Value : vehicleId;
    }

    public void StartNewLap() {
        currentLapTime = 0f;
        startTimeRecording = true;
    }

    public void FinishLap() {
        if (currentLapTime > 0 && (currentLapTime < bestLapTime || bestLapTime <= 0f)) {
            bestLapTime = currentLapTime;
        }
        var timeSpan = TimeSpan.FromSeconds(bestLapTime);
        playerBestTimeText.text = timeSpan.ToString("mm\\:ss\\.fff");
    }

    public void FinishSector1() {
        var timeSpan = TimeSpan.FromSeconds(currentSectorTime);
        playerSector1TimeText.text = timeSpan.ToString("mm\\:ss\\.fff");
    }

    public void StartSector() {
        currentSectorTime = 0f;
    }

    public void FinishSector2() {
        var timeSpan = TimeSpan.FromSeconds(currentSectorTime);
        playerSector2TimeText.text = timeSpan.ToString("mm\\:ss\\.fff");
    }

    public void FinishSector3() {
        var timeSpan = TimeSpan.FromSeconds(currentSectorTime);
        playerSector3TimeText.text = timeSpan.ToString("mm\\:ss\\.fff");
    }
}
