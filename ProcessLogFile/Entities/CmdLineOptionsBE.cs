using System;
using System.Collections.Generic;
using System.Text;

using CommandLine;

namespace ProcessLogFile.Entities
{
    /// <summary>
    /// This class represents command line options
    /// </summary>
    /// <see cref="https://github.com/commandlineparser/commandline"/>
    class CmdLineOptionsBE
    {
        [Option('d', "download", Required = false, HelpText = "Grab latest file from RoboRio.")]
        public bool IsPullLatestFromRoboRIO { get; set; }

        [Option('f', "file", Required = false, HelpText = "CSV Filename to process.")]
        public string CSVFileName { get; set; }

        [Option('g', "gsm", Required = false, HelpText = "Graph Set Name.")]
        public string GraphSetName { get; set; }
    }
}
