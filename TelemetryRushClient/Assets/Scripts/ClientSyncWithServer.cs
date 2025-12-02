using SimpleWebRTC;
using System.Globalization;
using UnityEngine;

public class ClientSyncWithServer : MonoBehaviour {

    [SerializeField] private WebRTCConnection webRTCConnection;
    //[Header("Trigger video streaming on sender")]
    //[SerializeField] private bool webRTCStartStopVideoStream = false;
    [Header("Trigger camera switch from receiver")]
    [SerializeField] private bool webRTCPositionSwitch = false;
    [SerializeField] private string cameraSwitchKeyword = "switch";
    [Header("Trigger prev camera switch from receiver")]
    [SerializeField] private bool webRTCPrevPositionSwitch = false;
    [SerializeField] private string prevCameraSwitchKeyword = "switchPrev";
    [Header("Trigger next car from receiver")]
    [SerializeField] private bool webRTCNextCarSwitch = false;
    [SerializeField] private string nextCarKeyword = "nextCar";
    [Header("Trigger play/pause from receiver")]
    [SerializeField] private bool webRTCPlayPauseSwitch = false;
    [SerializeField] private string playPauseKeyword = "playpause";
    [Header("Trigger spawn/despawn car from receiver")]
    [SerializeField] private bool webRTCSpawnDespawnSwitch = false;
    [SerializeField] private string spawnDespawnKeyword = "spawndespawn";
    [Header("Trigger car camera switch from receiver")]
    [SerializeField] private bool webRTCCarCameraSwitch = false;
    [SerializeField] private string carCameraSwitchKeyword = "switchCarCam";

    [Header("Trigger car controls from receiver")]
    [SerializeField] private bool webRTCAccelerationSwitch = false;
    [SerializeField] private string accelerationKeyword = "acceleration";
    [SerializeField] private bool webRTCBrakeSwitch = false;
    [SerializeField] private string brakeKeyword = "brake";
    [SerializeField] private bool webRTCLeftSwitch = false;
    [SerializeField] private string leftKeyword = "left";
    [SerializeField] private bool webRTCRightSwitch = false;
    [SerializeField] private string rightKeyword = "right";

    [Header("Sending Camera Position to Sender")]
    [SerializeField] private bool syncCameraPosition = false;
    [SerializeField] private float sendingIntervalInSeconds = 0.1f;

    private float sendingIntervalCounter = 0;
    private bool isPlayerCarSpawned = false;

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
        //if (webRTCStartStopVideoStream && webRTCConnection.IsSender && webRTCConnection.IsWebRTCActive) {
        //    webRTCStartStopVideoStream = false;
        //    if (webRTCConnection.IsVideoTransmissionActive) {
        //        webRTCConnection.StopVideoTransmission();
        //    } else {
        //        webRTCConnection.StartVideoTransmission();
        //    }
        //}

        // use the boolean flag for sending the camera switch
        if (webRTCPositionSwitch && webRTCConnection.IsWebRTCActive && webRTCConnection.IsReceiver) {
            webRTCPositionSwitch = false;
            webRTCConnection.SendDataChannelMessage(cameraSwitchKeyword);
        }
        if (webRTCPrevPositionSwitch && webRTCConnection.IsWebRTCActive && webRTCConnection.IsReceiver) {
            webRTCPrevPositionSwitch = false;
            webRTCConnection.SendDataChannelMessage(prevCameraSwitchKeyword);
        }
        // use the boolean flag for play pause telemetry streaming
        if (webRTCPlayPauseSwitch && webRTCConnection.IsWebRTCActive && webRTCConnection.IsReceiver) {
            webRTCPlayPauseSwitch = false;
            webRTCConnection.SendDataChannelMessage(playPauseKeyword);
        }
        if (webRTCNextCarSwitch && webRTCConnection.IsWebRTCActive && webRTCConnection.IsReceiver) {
            webRTCNextCarSwitch = false;
            webRTCConnection.SendDataChannelMessage(nextCarKeyword);
        }
        if (webRTCSpawnDespawnSwitch && webRTCConnection.IsWebRTCActive && webRTCConnection.IsReceiver) {
            webRTCSpawnDespawnSwitch = false;
            webRTCConnection.SendDataChannelMessage(spawnDespawnKeyword);
        }
        if (webRTCCarCameraSwitch && webRTCConnection.IsWebRTCActive && webRTCConnection.IsReceiver) {
            webRTCCarCameraSwitch = false;
            webRTCConnection.SendDataChannelMessage(carCameraSwitchKeyword);
        }

        if (isPlayerCarSpawned) {
            ControlCar();
        }

        if (syncCameraPosition && webRTCConnection.IsWebRTCActive && webRTCConnection.IsImmersiveSetupActive && webRTCConnection.IsReceiver && webRTCConnection.ExperimentalSupportFor6DOF) {
            sendingIntervalCounter += Time.deltaTime;
            if (sendingIntervalCounter >= sendingIntervalInSeconds) {
                sendingIntervalCounter = 0;
                webRTCConnection.SendDataChannelMessage($"{webRTCConnection.ExperimentalSpectatorCam6DOF.localPosition.x}||||{webRTCConnection.ExperimentalSpectatorCam6DOF.localPosition.y}||||{webRTCConnection.ExperimentalSpectatorCam6DOF.localPosition.z}");
            }
        }
    }

    private void OnDestroy() {
        webRTCConnection.Disconnect();
    }

    private void ControlCar() {
        if (webRTCConnection.IsWebRTCActive && webRTCConnection.IsReceiver) {
            var isAccelerating = webRTCAccelerationSwitch ? "||||1" : "||||0";
            webRTCConnection.SendDataChannelMessage(accelerationKeyword + isAccelerating);

            var isBraking = webRTCBrakeSwitch ? "||||1" : "||||0";
            webRTCConnection.SendDataChannelMessage(brakeKeyword + isBraking);

            var isSteeringLeft = webRTCLeftSwitch ? "||||1" : "||||0";
            webRTCConnection.SendDataChannelMessage(leftKeyword + isSteeringLeft);

            var isSteeringRight = webRTCRightSwitch ? "||||1" : "||||0";
            webRTCConnection.SendDataChannelMessage(rightKeyword + isSteeringRight);
        }
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
                // trigger camera switch on server (C key)
            }
        }
    }

    public void SpawnDespawnPlayerCar() {
        webRTCSpawnDespawnSwitch = true;
        isPlayerCarSpawned = !isPlayerCarSpawned;
    }
}