using System;
using System.Collections.Generic;
using System.Text;

using CommandLine;

namespace ProcessLogFile.Entities
{
    /// <summary>
    /// https://github.com/commandlineparser/commandline
    /// </summary>
    class CmdLineOptionsBE
    {
        [Option('d', "download", Required = false, HelpText = "Grab latest file from RoboRio.")]
        public bool IsPullLatestFromRoboRIO { get; set; }

        [Option('f', "file", Required = false, HelpText = "CSV Filename to process.")]
        public string CSVFileName { get; set; }
    }
}
