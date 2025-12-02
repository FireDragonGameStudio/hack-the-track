using Dreamteck.Splines;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TelemetryVehiclePlayer : MonoBehaviour {
    public string VehicleId;
    public SplinePositioner Positioner;

    public bool IsInterpolationActive;
    public bool IsPaused;

    public TelemetryUI TelemetryDisplay;

    // Sanity check tolerance (meters)
    public float DistanceTolerance = 10f;

    public LineRenderer TrackRenderer;
    public SplineProjector TelemetryCar;
    public GameObject CarGeometry;

    [Header("Acceleration Sensors")]
    public Transform XForceSensor;
    public Transform YForceSensor;

    [Header("Movement")]
    public Transform LeftFrontWheelSpin;
    public Transform RightFrontWheelSpin;
    public Transform LeftRearWheelSpin;
    public Transform RightRearWheelSpin;

    [Header("Steering-CarGeometry")]
    public Transform SteeringWheel;
    public Transform LeftFrontWheel;
    public Transform RightFrontWheel;

    [Header("Steering-TelemetryCar")]
    public Transform TelemetrySteeringWheel;
    public Transform TelemetryLeftFrontWheel;
    public Transform TelemetryRightFrontWheel;

    [Header("Physics Settings")]
    public float maxAcceleration = 5f;   // m/s² at full throttle
    public float maxBraking = 10f;       // m/s² at full brake
    public float maxSpeed = 100f;        // m/s (360 km/h)

    private List<Vector3> points = new List<Vector3>();
    private Vector3 newPosition = Vector3.zero;
    private Vector3 oldPosition = Vector3.zero;

    private float currentSpeed;          // m/s
    private float integratedDistance;    // meters along spline (our continuous integration)
    private float splineLength;
    private int currentLapNumber = 2;

    private float currentLapDistance;
    //private float lastLapDistance;

    private float currentRPM;
    private float currentThrottle;
    private float currentBrakeFront;
    private float currentBrakeRear;
    private float currentSteeringAngle;
    private int currentGear;

    private float currentAccelerationXForce;
    private float currentAccelerationYForce;

    private DateTime currentDateTime;

    private SectionEnduranceReceiver sectionEnduranceReceiver;

    private void Start() {
        var receiver = FindFirstObjectByType<TelemetryReceiver>();
        if (receiver != null) receiver.Register(this);

        var vehicleSelector = FindFirstObjectByType<TelemetryVehicleSelector>();
        if (vehicleSelector != null) vehicleSelector.RegisterVehicle(this);

        sectionEnduranceReceiver = FindFirstObjectByType<SectionEnduranceReceiver>();
        if (sectionEnduranceReceiver != null) sectionEnduranceReceiver.RegisterVehicle(this);

        splineLength = Positioner.CalculateLength();
        Debug.Log($"Spline Length: {splineLength}");

        Positioner.enabled = IsInterpolationActive;
        TelemetryCar.enabled = !IsInterpolationActive;
        TelemetryCar.gameObject.SetActive(!IsInterpolationActive);

        XForceSensor.localScale = Vector3.zero;
        YForceSensor.localScale = Vector3.zero;

        TelemetryDisplay.ShowGhostCars.onValueChanged.AddListener(v => {
            CarGeometry.SetActive(v);
        });
    }

    private void Update() {
        if (!IsPaused && IsInterpolationActive) {
            // --- Simulate acceleration and braking ---
            float throttleInput = Mathf.Clamp01(currentThrottle / 100f); // 0 to 1
            float brakeInput = Mathf.Clamp01((currentBrakeFront + currentBrakeRear) / 20f); // normalize ~0–1 (assuming 10 bar max per axle)

            float acceleration = throttleInput * maxAcceleration; // m/s²
            float braking = brakeInput * maxBraking;              // m/s²

            float netAccel = acceleration - braking;

            currentSpeed += netAccel * Time.deltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);

            // Advance integrated distance
            integratedDistance += currentSpeed * Time.deltaTime;

            // Wrap around spline
            if (integratedDistance > splineLength) {
                integratedDistance -= splineLength;
                //currentLapNumber++;
            }

            Positioner.SetDistance(integratedDistance);

            // Update estimated speed display
            TelemetryDisplay.CurrentSpeed.text = (currentSpeed * 3.6f).ToString("F2"); // km/h
            CarGeometry.transform.SetPositionAndRotation(transform.position, transform.rotation);

            // assuming 45cm wheel radius
            float angularVelocity = (currentSpeed / (2f * Mathf.PI * 0.45f)) * 360f * Time.deltaTime * (-1f);
            LeftFrontWheelSpin.Rotate(Vector3.right, angularVelocity, Space.Self);
            RightFrontWheelSpin.Rotate(Vector3.right, angularVelocity, Space.Self);
            LeftRearWheelSpin.Rotate(Vector3.right, angularVelocity, Space.Self);
            RightRearWheelSpin.Rotate(Vector3.right, angularVelocity, Space.Self);
        }
    }

    private void LateUpdate() {
        TelemetryCar.transform.rotation = CarGeometry.transform.rotation;
    }

    private void OnDestroy() {
        points.Clear();
    }

    public void ApplyTelemetry(Dictionary<string, object> samples, string timestamp) {
        if (!samples.ContainsKey("Laptrigger_lapdist_dls")) return;
        if (!DateTime.TryParse(timestamp, out DateTime dt)) return;

        currentDateTime = dt;

        if (samples.ContainsKey("nmot")) {
            currentRPM = Convert.ToSingle(samples["nmot"]);
        }

        if (samples.ContainsKey("Steering_Angle")) {
            currentSteeringAngle = Convert.ToSingle(samples["Steering_Angle"]);
            SteeringWheel.localRotation = Quaternion.Euler(0, 0, currentSteeringAngle);
            TelemetrySteeringWheel.localRotation = Quaternion.Euler(0, 0, currentSteeringAngle);
            ApplySteeringToWheels(currentSteeringAngle);
        }

        if (samples.ContainsKey("aps")) {
            currentThrottle = Convert.ToSingle(samples["aps"]);
        }

        if (samples.ContainsKey("pbrake_f")) {
            currentBrakeFront = Convert.ToSingle(samples["pbrake_f"]);
        }
        if (samples.ContainsKey("pbrake_r")) {
            currentBrakeRear = Convert.ToSingle(samples["pbrake_r"]);
        }

        if (samples.ContainsKey("gear")) {
            currentGear = Convert.ToInt16(samples["gear"]);
        }

        if (samples.ContainsKey("lap")) {
            currentLapNumber = Convert.ToInt16(samples["lap"]);
        }

        if (samples.ContainsKey("accx_can")) {
            currentAccelerationXForce = Convert.ToSingle(samples["accx_can"]);
            XForceSensor.localScale = new Vector3(currentAccelerationXForce, 1f, 1f);
        }
        if (samples.ContainsKey("accy_can")) {
            currentAccelerationYForce = Convert.ToSingle(samples["accy_can"]) * (-1f);
            YForceSensor.localScale = new Vector3(1f, 1f, currentAccelerationYForce);
        }

        // Speed is usually in km/h, convert to m/s if needed
        if (samples.ContainsKey("speed")) {
            float speedKph = Convert.ToSingle(samples["speed"]);
            currentSpeed = speedKph / 3.6f;
        }

        currentLapDistance = (float)Convert.ToDouble(samples["Laptrigger_lapdist_dls"]);

        // First sample: initialise
        if (integratedDistance <= 0f) {
            integratedDistance = currentLapDistance;
            Positioner.SetDistance(integratedDistance);
            return;
        }

        //// Detect lap wrap
        //if (lastLapDistance > 3000 && currentLapDistance < lastLapDistance) {
        //    integratedDistance -= splineLength;
        //    currentLapNumber++;
        //}
        //lastLapDistance = currentLapDistance;

        // Sanity check: if telemetry lap distance differs too much from our integrated distance, snap
        if (IsInterpolationActive && Mathf.Abs(currentLapDistance - integratedDistance) > DistanceTolerance) {
            integratedDistance = currentLapDistance;
            Positioner.SetDistance(integratedDistance);
            //currentSpeed -= 10f;
        }

        // render real postion of race car to have a value to compare
        if (samples.ContainsKey("VBOX_Lat_Min") && samples.ContainsKey("VBOX_Long_Minutes")) {
            double lat = Convert.ToDouble(samples["VBOX_Lat_Min"]);
            double lon = Convert.ToDouble(samples["VBOX_Long_Minutes"]);
            newPosition = GPSUtils.GeoToUnity((float)lat, (float)lon);

            TelemetryCar.transform.position = newPosition;
        }

        // update meta data
        if (!oldPosition.Equals(newPosition)) {
            oldPosition = newPosition;

            points.Add(newPosition + new Vector3(0f, 0.25f, 0f));

            TrackRenderer.positionCount = points.Count;
            TrackRenderer.SetPositions(points.ToArray());
        }
    }

    private string InferAccelerationClass() {
        float absX = Mathf.Abs(currentAccelerationXForce);
        float absY = Mathf.Abs(currentAccelerationYForce);
        float load = Mathf.Sqrt(currentAccelerationXForce * currentAccelerationXForce + currentAccelerationYForce * currentAccelerationYForce);

        string phase = "Trail Zone - cornering braking/throttle";
        if (absX > absY * 1.5f) {
            phase = currentAccelerationXForce > 0 ? "Acceleration Phase" : "Braking Phase";
        } else if (absY > absX * 1.5f) {
            phase = "Cornering Phase";
        }

        // Combined total load classification
        string accelerationClass = "Limit Zone --- car near traction limit";
        if (load < 0.1f) accelerationClass = "Cruise Zone --- car stable, minimal load";
        if (load < 0.25f) accelerationClass = "Light Dynamics --- gentle motion";
        if (load < 0.45f) accelerationClass = "Push Zone --- moderate throttle or braking";
        if (load < 0.65f) accelerationClass = "Attack Zone --- strong dynamic load";

        return $"{accelerationClass}\n{phase} --- load {load:F4}g";
    }

    private void ApplySteeringToWheels(float steeringAngleDegrees, float maxSteeringAngle = 30f) {
        // Clamp the input angle to the realistic steering range
        float clampedAngle = Mathf.Clamp(steeringAngleDegrees, -maxSteeringAngle, maxSteeringAngle) * (-1f);

        // Apply local rotation to simulate wheel steering turning
        LeftFrontWheel.localRotation = Quaternion.Euler(0f, clampedAngle, 0f);
        RightFrontWheel.localRotation = Quaternion.Euler(0f, clampedAngle, 0f);
        TelemetryLeftFrontWheel.localRotation = Quaternion.Euler(0f, clampedAngle, 0f);
        TelemetryRightFrontWheel.localRotation = Quaternion.Euler(0f, clampedAngle, 0f);
    }


    public void UpdateUIValues() {
        TelemetryDisplay.VehicleId.text = VehicleId;

        TelemetryDisplay.CurrentRPM.text = currentRPM.ToString("F0");
        TelemetryDisplay.CurrentRPMSlider.value = currentRPM;

        TelemetryDisplay.CurrentSteering.text = currentSteeringAngle.ToString("F2");
        TelemetryDisplay.SteeringWheelImage.localRotation = Quaternion.Euler(Vector3.forward * currentSteeringAngle);

        TelemetryDisplay.CurrentThrottle.text = currentThrottle.ToString("F2");
        TelemetryDisplay.CurrentThrottleSlider.value = currentThrottle;

        TelemetryDisplay.CurrentBrakeFront.text = currentBrakeFront.ToString("F2");
        TelemetryDisplay.CurrentBrakeFrontSlider.value = currentBrakeFront;

        TelemetryDisplay.CurrentBrakeRear.text = currentBrakeRear.ToString("F2");
        TelemetryDisplay.CurrentBrakeRearSlider.value = currentBrakeRear;

        TelemetryDisplay.CurrentGear.text = currentGear.ToString();

        TelemetryDisplay.CurrentSpeed.text = (currentSpeed * 3.6f).ToString("F2"); // km/h

        TelemetryDisplay.CurrentLapDistance.text = currentLapDistance.ToString("F2");

        TelemetryDisplay.CurrentLap.text = currentLapNumber.ToString();

        TelemetryDisplay.CurrentTime.text = $"{currentDateTime:dd/MM/yyyy H:mm:ss}";

        TelemetryDisplay.AccelerationXForce.text = currentAccelerationXForce.ToString("F4");
        TelemetryDisplay.AccelerationYForce.text = currentAccelerationYForce.ToString("F4");
        TelemetryDisplay.AccelerationForceClass.text = InferAccelerationClass();

        TelemetryDisplay.CurrentColorImage.color = TrackRenderer.material.color;
        TelemetryDisplay.CurrentFlagColorImage.color = GetFlagColorByLapNumber(currentLapNumber - 1, VehicleId);
    }

    public Transform GetCameraTarget() {
        return IsInterpolationActive ? CarGeometry.transform : TelemetryCar.transform;
    }

    public void OnTelemetryStreamEnded(string timestamp) {
        Debug.Log($"{VehicleId} telemetry stream ended at {timestamp}");
    }

    public void OnTelemetryStreamPaused(bool isPause) {
        Debug.Log($"{VehicleId} telemetry stream paused: {isPause}");
        IsPaused = isPause;
    }

    private Color GetFlagColorByLapNumber(int lapNumber, string vehicleId) {
        var carNumber = sectionEnduranceReceiver.VehicleSelector.ExtractCarNumber(vehicleId);
        if (!sectionEnduranceReceiver.CarLapData.ContainsKey(carNumber)) {
            return FlagToColor("");
        }

        var laps = sectionEnduranceReceiver.CarLapData[carNumber];
        var validLap = laps.FirstOrDefault(l => l.lap == lapNumber);
        if (validLap != null) {
            return FlagToColor(validLap.flag);
        }
        return FlagToColor("");
    }

    private Color FlagToColor(string flag) {
        return flag switch {
            "GF" => Color.green,
            "FCY" => Color.yellow,
            "FF" => Color.black,
            _ => Color.gray,
        };
    }
}
