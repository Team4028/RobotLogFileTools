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
        public Graphset[] graphSets { get; set; }
    }

    public class Roborio
    {
        public string ipv4Address { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string logFileFolder { get; set; }
    }

    public class Graphset
    {
        public string setName { get; set; }
        public Angleconversion[] angleConversion { get; set; }
        public Linegraph[] lineGraphs { get; set; }
        public Xygraph[] xyGraphs { get; set; }
    }

    public class Angleconversion
    {
        public string radians { get; set; }
        public string boundedDegrees { get; set; }
    }

    public class Linegraph
    {
        public string name { get; set; }
        public Xaxis xAxis { get; set; }
        public Yaxis yAxis { get; set; }
        public Gains gains { get; set; }
        public Calcareadelta calcAreaDelta { get; set; }
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
        public string pidGains { get; set; }
        public string controlMode { get; set; }
        public string followerGains { get; set; }
    }

    public class Calcareadelta
    {
        public string elaspedTime { get; set; }
        public string target { get; set; }
        public string actual { get; set; }
    }

    public class Xygraph
    {
        public string name { get; set; }
        public Series[] series { get; set; }
        public object gains { get; set; }
        public object calcAreaDelta { get; set; }
    }

    public class Series
    {
        public string name { get; set; }
        public string xAxisCoumnName { get; set; }
        public string yAxisColumnName { get; set; }
    }


}
