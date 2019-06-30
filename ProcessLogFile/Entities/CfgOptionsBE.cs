﻿using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace ProcessLogFile.Entities
{
    /// <summary>
    /// These classes represent the Config of the Graphs to generate stored in a JSON file
    /// </summary>
    class CfgOptionsBE
    {
        [JsonProperty(PropertyName = "roboRio")]
        public RoboRioBE RoboRio { get; set; }

        [JsonProperty(PropertyName = "localWorkingFolder")]
        public string LocalWorkingFolder { get; set; }

        [JsonProperty(PropertyName = "logFileExtension")]
        public string LogFileExtension { get; set; }

        [JsonProperty(PropertyName = "graphSets")]
        public List<GraphSetBE> GraphSets { get; set; }
    }
}

public class RoboRioBE
{
    [JsonProperty(PropertyName = "ipv4Address")]
    public string Ipv4Address { get; set; }

    [JsonProperty(PropertyName = "username")]
    public string Username { get; set; }

    [JsonProperty(PropertyName = "password")]
    public string Password { get; set; }

    [JsonProperty(PropertyName = "logFileFolder")]
    public string LogFileFolder { get; set; }
}

public class GraphSetBE
{
    [JsonProperty(PropertyName = "setName")]
    public string SetName { get; set; }

    [JsonProperty(PropertyName = "graphs")]
    public List<GraphBE> Graphs { get; set; }
}

public class GraphBE
{
    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; }

    [JsonProperty(PropertyName = "xAxis")]
    public XAxisBE XAxis { get; set; }

    [JsonProperty(PropertyName = "yAxis")]
    public YAxisBE YAxis { get; set; }

    [JsonProperty(PropertyName = "gains")]
    public GainsBE Gains { get; set; }

    [JsonProperty(PropertyName = "chartType")]
    public string ChartType { get; set; }

    [JsonProperty(PropertyName = "calcAreaDelta")]
    public CalcAreaDeltaBE CalcAreaDelta { get; set; }
}

public class XAxisBE
{
    [JsonProperty(PropertyName = "axisTitle")]
    public string AxisTitle { get; set; }

    [JsonProperty(PropertyName = "fromColumnName")]
    public string FromColumnName { get; set; }
}

public class YAxisBE
{
    [JsonProperty(PropertyName = "axisTitle")]
    public string AxisTitle { get; set; }

    [JsonProperty(PropertyName = "fromColumnNames")]
    public List<string> FromColumnNames { get; set; }
}

public class GainsBE
{
    [JsonProperty(PropertyName = "pidGains")]
    public string PIDGains { get; set; }

    [JsonProperty(PropertyName = "followerGains")]
    public string FollowerGains { get; set; }

    [JsonProperty(PropertyName = "controlMode")]
    public string ControlMode { get; set; }
}

public class CalcAreaDeltaBE
{
    [JsonProperty(PropertyName = "elaspedTime")]
    public string ElapsedTimeInMS { get; set; }

    [JsonProperty(PropertyName = "target")]
    public string TargetColumnName { get; set; }

    [JsonProperty(PropertyName = "actual")]
    public string ActualColumnName { get; set; }
}
