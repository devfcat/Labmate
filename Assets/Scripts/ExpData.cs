using System;
using UnityEngine;

[Serializable]
public class ExpData
{
    public float voltage;
    public float current;
    public float power;
    public float resistance;
    public float error;
    public string timestamp;
    public string experimentType;

    public ExpData(float voltage, float current, float power, float resistance, float error, string experimentType)
    {
        this.voltage = voltage;
        this.current = current;
        this.power = power;
        this.resistance = resistance;
        this.error = error;
        this.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        this.experimentType = experimentType;
    }
} 