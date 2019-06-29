using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessLogFile.Entities
{
    class Scratch
    {
    }



    public class Rootobject
    {
        public Roborio roboRio { get; set; }
        public string localWorkingFolder { get; set; }
        public string logFileExtension { get; set; }
        public Graph[] graphs { get; set; }
    }

    public class Roborio
    {
        public string ipv4Address { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string logFileFolder { get; set; }
    }

    public class Graph
    {
        public string name { get; set; }
        public Xaxis xAxis { get; set; }
        public Yaxis yAxis { get; set; }
        public Gains gains { get; set; }
        public string chartType { get; set; }
    }

    public class Xaxis
    {
        public string axisTitle { get; set; }
        public string fromColumnName { get; set; }
    }

    public class Yaxis
    {
        public string axisTitle { get; set; }
        public string[] fromColumnNames { get; set; }
    }

    public class Gains
    {
        public string PIDGains { get; set; }
        public string FollowerGains { get; set; }
    }



}
