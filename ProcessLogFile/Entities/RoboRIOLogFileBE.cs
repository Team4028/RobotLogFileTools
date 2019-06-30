using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessLogFile.Entities
{
    class RoboRIOLogFileBE
    {
        public string FileName { get; set; }

        public string FilePathName { get; set; }

        public DateTime LastModDT { get; set; }
    }
}
