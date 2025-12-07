using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LapDataWrapper<T> {
    public List<T> LapDataList;
    public string VehicleNumber;
    public Color VehicleColor;
}
