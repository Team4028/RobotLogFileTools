using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using Newtonsoft.Json;

using ProcessLogFile.Entities;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace ProcessLogFile
{
    class Program
    {
        const string CONFIG_FILENAME = @"CfgOptions.json";

        static int Main(string[] args)
        {
            // write out build version
            var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var fileVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;

            System.Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"FRC Team 4028 Log File Graphing Tool   [v{fileVersion}]");
            System.Console.ResetColor();

            // load config
            var config = LoadConfig(CONFIG_FILENAME);
            if (config == null) return -1;

            string logFilePathName = string.Empty;

            try
            {
                // parse command line options
                Parser.Default.ParseArguments<CmdLineOptionsBE>(args)
                        .WithParsed<CmdLineOptionsBE>(o =>
                        {
                            // pull file from roborio
                            if (o.IsPullLatestFromRoboRIO)
                            {
                                logFilePathName = CopyLatestFileFromRoboRio(config);
                            }
                            else if (!string.IsNullOrEmpty(o.CSVFileName))
                            {
                                string fileExtension = System.IO.Path.GetExtension(o.CSVFileName).ToLower();
                                if (fileExtension != config.LogFileExtension)
                                {
                                    throw new ApplicationException($"The file extension [{fileExtension}] must be {config.LogFileExtension}.");
                                }

                                // use the filename by itself
                                if (System.IO.File.Exists(o.CSVFileName))
                                {
                                    logFilePathName = o.CSVFileName;
                                }
                                else
                                {
                                    // try looking for that name in the target folder
                                    logFilePathName = System.IO.Path.Combine(config.LocalWorkingFolder, o.CSVFileName);

                                    if (!System.IO.File.Exists(logFilePathName))
                                    { 
                                        throw new ApplicationException($"File [{o.CSVFileName}] does not exist.");
                                    }
                                }

                            }
                            else
                            {
                                throw new ApplicationException($"You must supply a local log file or download one from the RoboRio");
                            }

                            // process file
                            GraphBuilder.ProcessLogFile(logFilePathName, config);
                        });

                return 0;
            }
            catch(Exception ex)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine($"Error: [{ex.Message}]");
                System.Console.ResetColor();
                return -1;
            }

        }

        // this method loads the config file from a external json file
        private static CfgOptionsBE LoadConfig(string configFileName)
        {
            CfgOptionsBE config = null;

            try
            {
                config = JsonConvert.DeserializeObject<CfgOptionsBE>(File.ReadAllText(configFileName));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error loading config: {ex}");
            }

            return config;
        }

        /// <summary>
        /// this method retrieves the latest log file from the RoboRio (using SFTP)
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns>System.String.</returns>
        /// <see cref="https://github.com/sshnet/SSH.NET/"/>
        private static string CopyLatestFileFromRoboRio(CfgOptionsBE config)
        {
            string lastestLogFilePathName = string.Empty;

            try
            {
                // config SFTP connection
                var connectionInfo = new ConnectionInfo(config.RoboRio.Ipv4Address,
                                                        config.RoboRio.Username,
                                                        new PasswordAuthenticationMethod(config.RoboRio.Username, config.RoboRio.Password),
                                                        new PrivateKeyAuthenticationMethod("rsa.key"));

                // create a sftp client using the connection params
                using (var sftpClient = new SftpClient(connectionInfo))
                {
                    // connect
                    sftpClient.Connect();

                    // get a list of the remote files
                    var remoteFiles = sftpClient.ListDirectory(config.RoboRio.LogFileFolder);

                    Dictionary<DateTime, string> logFiles = new Dictionary<DateTime, string>();

                    // loop thru each log file
                    foreach (SftpFile file in remoteFiles)
                    {
                        // skip directories
                        if (file.IsDirectory) continue;

                        // skip empty files
                        if (file.Length == 0) continue;

                        // skip files with the wrong extension
                        if (Path.GetExtension(file.Name).ToLower() != config.LogFileExtension.ToLower()) continue;

                        // add file to dicionary
                        logFiles.Add(file.LastWriteTime, file.Name);
                    }

                    // sort the list in descending order and pink the newest filename
                    string latestLogFileName = logFiles.OrderByDescending(f => f.Key).First().Value;

                    // build target file path name
                    lastestLogFilePathName = System.IO.Path.Combine(config.LocalWorkingFolder, latestLogFileName);

                    //see if we already have this file
                    if (!System.IO.File.Exists(lastestLogFilePathName))
                    {
                        // download the most recent file
                        using (Stream fileStream = File.Create(lastestLogFilePathName))
                        {
                            System.Console.WriteLine();
                            System.Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"... Now downloading latest log file: [{latestLogFileName}]");
                            System.Console.ResetColor();

                            sftpClient.DownloadFile(latestLogFileName, fileStream);
                        }
                    }
                    else
                    {
                        System.Console.WriteLine();
                        System.Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"... Latest log file: [{latestLogFileName}] already downloaded");
                        System.Console.ResetColor();
                    }
                }
            }
            catch(Exception ex)
            {
                System.Console.WriteLine($"Error downloading latest file from RoboRIO: {ex}");
            }

            return lastestLogFilePathName;
        }

    }
}