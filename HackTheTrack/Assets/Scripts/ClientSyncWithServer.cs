using NativeWebSocket;
using SimpleWebRTC;
using System.Globalization;
using UnityEngine;

public class ClientSyncWithServer : MonoBehaviour {

    [SerializeField] private WebRTCConnection webRTCConnection;
    [SerializeField] private string cameraSwitchKeyword = "switch";
    [SerializeField] private string carCameraSwitchKeyword = "switchCarCam";
    [SerializeField] private string playPauseKeyword = "playPause";
    [SerializeField] private string prevCameraSwitchKeyword = "switchPrev";
    [SerializeField] private string nextCarKeyword = "nextCar";
    [SerializeField] private string spawnDespawnKeyword = "spawnDespawn";

    [SerializeField] private string accelerationKeyword = "acceleration";
    [SerializeField] private string brakeKeyword = "brake";
    [SerializeField] private string leftKeyword = "left";
    [SerializeField] private string rightKeyword = "right";

    [SerializeField] private string sendMeChartDataKeyword = "chartDataPls";

    [Header("Trigger video streaming on sender")]
    [SerializeField] private bool webRTCStartStopVideoStream = false;
    [Header("Trigger camera switch from receiver")]
    [SerializeField] private bool webRTCPositionSwitch = false;
    [Header("Sending Camera Position to Sender")]
    [SerializeField] private bool syncCameraPosition = false;
    [SerializeField] private float sendingIntervalInSeconds = 0.1f;
    [Header("Trigger lap event data sending on sender")]
    [SerializeField] private bool sendChartDataToClient = false;

    [Header("Telemetry Rush References")]
    [SerializeField] private TelemetryVehicleSelector telemetryVehicleSelector;
    [SerializeField] private TelemetryReceiver telemetryReceiver;
    [SerializeField] private SectionEnduranceReceiver sectionEnduranceReceiver;

    private float sendingIntervalCounter = 0;
    private int connectionCounter = 0;

    // make sure the float decimal separator is converted correctly
    private NumberFormatInfo numberFormatInfo = new NumberFormatInfo { NumberDecimalSeparator = "." };

    private void Start() {
        webRTCConnection.Connect();
    }

    private void Update() {
        // example: use the input system keyboard for starting/stopping the video stream
        //if (Keyboard.current.vKey.wasPressedThisFrame) {
        //    webRTCConnection.StartVideoTransmission();
        //}
        //if (Keyboard.current.bKey.wasPressedThisFrame) {
        //    webRTCConnection.StopVideoTransmission();
        //}

        // example: use the OVR input for sending the camera switch
        //if (OVRInput.Get(OVRInput.Button.Start)) {
        //    webRTCConnection.SendDataChannelMessage(cameraSwitchKeyword);
        //}

        // use the boolean flag for starting/stopping the camera stream
        if (webRTCStartStopVideoStream && webRTCConnection.IsSender && webRTCConnection.IsWebRTCActive) {
            webRTCStartStopVideoStream = false;
            if (webRTCConnection.IsVideoTransmissionActive) {
                webRTCConnection.StopVideoTransmission();
            } else {
                webRTCConnection.StartVideoTransmission(); ;
            }
        }

        if (sendChartDataToClient && webRTCConnection.IsWebRTCActive && webRTCConnection.IsImmersiveSetupActive && webRTCConnection.IsSender && webRTCConnection.ExperimentalSupportFor6DOF) {
            sendChartDataToClient = false;

            var currentVehicleId = sectionEnduranceReceiver.VehicleSelector.GetCurrentlySelectedVehicleId();
            var carNumber = sectionEnduranceReceiver.VehicleSelector.ExtractCarNumber(currentVehicleId);
            var lapEventData = sectionEnduranceReceiver.CarLapData[carNumber];
            var vehicleColor = sectionEnduranceReceiver.CarColor[carNumber];

            string json = JsonUtility.ToJson(new LapDataWrapper<LapEvent> {
                LapDataList = lapEventData,
                VehicleNumber = carNumber,
                VehicleColor = vehicleColor
            });

            webRTCConnection.SendDataChannelMessage(json);
        }

        // use the boolean flag for sending the camera switch
        //if (webRTCPositionSwitch && webRTCConnection.IsWebRTCActive && webRTCConnection.IsReceiver) {
        //    webRTCPositionSwitch = false;
        //    webRTCConnection.SendDataChannelMessage(cameraSwitchKeyword);
        //}
        //if (syncCameraPosition && webRTCConnection.IsWebRTCActive && webRTCConnection.IsImmersiveSetupActive && webRTCConnection.IsReceiver && webRTCConnection.ExperimentalSupportFor6DOF) {
        //    sendingIntervalCounter += Time.deltaTime;
        //    if (sendingIntervalCounter >= sendingIntervalInSeconds) {
        //        sendingIntervalCounter = 0;
        //        webRTCConnection.SendDataChannelMessage($"{webRTCConnection.ExperimentalSpectatorCam6DOF.localPosition.x}||||{webRTCConnection.ExperimentalSpectatorCam6DOF.localPosition.y}||||{webRTCConnection.ExperimentalSpectatorCam6DOF.localPosition.z}");
        //    }
        //}
    }

    private void OnDestroy() {
        webRTCConnection.Disconnect();
    }

    public void OnMessageReceived(string message) {
        if (webRTCConnection.IsImmersiveSetupActive && webRTCConnection.IsSender) {
            string[] trylocalPosition = message.Split("||||");
            bool isPositionMessage = trylocalPosition.Length == 3;
            if (webRTCConnection.ExperimentalSupportFor6DOF && isPositionMessage) {
                //Debug.Log($"x = {trylocalPosition[0]} = {float.Parse(trylocalPosition[0], numberFormatInfo)}, y = {trylocalPosition[1]} = {float.Parse(trylocalPosition[1], numberFormatInfo)}, z = {trylocalPosition[2]} = {float.Parse(trylocalPosition[2], numberFormatInfo)}");

                // sanity check for , as decimal separator
                if (trylocalPosition[0].Contains(",") || trylocalPosition[1].Contains(",") || trylocalPosition[2].Contains(",")) {
                    trylocalPosition[0] = trylocalPosition[0].Replace(",", ".");
                    trylocalPosition[1] = trylocalPosition[1].Replace(",", ".");
                    trylocalPosition[2] = trylocalPosition[2].Replace(",", ".");
                }

                webRTCConnection.VideoStreamingCamera.transform.localPosition = new Vector3(
                    float.Parse(trylocalPosition[0], numberFormatInfo),
                    float.Parse(trylocalPosition[1], numberFormatInfo),
                    float.Parse(trylocalPosition[2], numberFormatInfo));

            } else if (message.ToLower().Equals(cameraSwitchKeyword.ToLower())) {
                telemetryVehicleSelector.SelectNextCamera();
            } else if (message.ToLower().Equals(prevCameraSwitchKeyword.ToLower())) {
                telemetryVehicleSelector.SelectPreviousCamera();
            } else if (message.ToLower().Equals(nextCarKeyword.ToLower())) {
                telemetryVehicleSelector.SelectNextCar();
            } else if (message.ToLower().Equals(spawnDespawnKeyword.ToLower())) {
                telemetryVehicleSelector.SpawnDespawnPlayerCar();
            } else if (message.ToLower().Equals(carCameraSwitchKeyword.ToLower())) {
                telemetryVehicleSelector.RemoteChangeCarActiveCamera();
            } else if (message.ToLower().Equals(sendMeChartDataKeyword.ToLower())) {
                sendChartDataToClient = true;
            } else if (message.ToLower().Equals(playPauseKeyword.ToLower())) {
                var currentlySelectedVehicle = telemetryVehicleSelector.GetCurrentSelectedVehicle();

                if (currentlySelectedVehicle == null) return;

                if (currentlySelectedVehicle.IsPaused) {
                    telemetryReceiver.Play();
                } else {
                    telemetryReceiver.Pause();
                }
            } else {
                // car controls
                if (message.ToLower().Contains(accelerationKeyword.ToLower())) {
                    string[] carControlInput = message.Split("||||");
                    telemetryVehicleSelector.Acceleration(carControlInput[1].Equals("1"));
                }
                if (message.ToLower().Contains(brakeKeyword.ToLower())) {
                    string[] carControlInput = message.Split("||||");
                    telemetryVehicleSelector.Brake(carControlInput[1].Equals("1"));
                }
                if (message.ToLower().Contains(leftKeyword.ToLower())) {
                    string[] carControlInput = message.Split("||||");
                    telemetryVehicleSelector.Left(carControlInput[1].Equals("1"));
                }
                if (message.ToLower().Contains(rightKeyword.ToLower())) {
                    string[] carControlInput = message.Split("||||");
                    telemetryVehicleSelector.Right(carControlInput[1].Equals("1"));
                }
            }
        }
    }

    public void CheckForRace(WebSocketState wsState) {
        if (wsState == WebSocketState.Closed) {
            connectionCounter++;

            if (connectionCounter >= 4) {
                webRTCConnection.gameObject.SetActive(false);
                connectionCounter = 0;
            }
        }
    }
}