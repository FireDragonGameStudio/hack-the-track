using System;

[Serializable]
public class LapEvent {
    public string type;
    public string vehicle_id;   // e.g. "78"
    public int lap;
    public string lap_time;
    public float[] sector_times;
    public float top_speed;
    public string flag;
    public bool pit;
    public string timestamp;
}