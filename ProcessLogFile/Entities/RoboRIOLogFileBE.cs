using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessLogFile.Entities
{
    /// <summary>
    /// This class represents a single log file read from the RoboRIO
    /// This is used to build a list then sort by lastmoddt descending to find the most recent (ie: newest) file
    /// </summary>
    class RoboRIOLogFileBE
    {
        public string FileName { get; set; }

        public string FilePathName { get; set; }

        public DateTime LastModDT { get; set; }
    }
}
