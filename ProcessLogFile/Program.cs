﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using CommandLine;
using Newtonsoft.Json;

using ProcessLogFile.Entities;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace ProcessLogFile
{
    class Program
    {
        // this is the name of config file, it shoudl always be in the folder with the executable
        const string CONFIG_FILENAME = @"CfgOptions.json";

        static int Main(string[] args)
        {
            // write out build version
            var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var fileVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;

            System.Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"FRC Team 4028 Log File Graphing Tool   [v{fileVersion}]");
            System.Console.ResetColor();

            try
            {
                // load config from file
                var config = LoadConfig(CONFIG_FILENAME);
                if (config == null)
                {
                    throw new ApplicationException($"Cannot locate the config file: [{CONFIG_FILENAME}], it should be in the folder with: [{assemblyLocation}]");
                }

                string logFilePathName = string.Empty;

                // parse command line options
                Parser.Default.ParseArguments<CmdLineOptionsBE>(args)
                        .WithParsed<CmdLineOptionsBE>(o =>
                        {
                            if (string.IsNullOrEmpty(o.GraphSetName))
                                {
                                    throw new ApplicationException($"You must supply a graph set name!");
                                }
                            // the file will be pulled from roborio using SFTP
                            else if (o.IsPullLatestFromRoboRIO)
                            {
                                if (!IsServerAvailable(config.RoboRio.Ipv4Address, 22))
                                {
                                    throw new ApplicationException($"Cannot connect to RoboRio at: [{config.RoboRio.Ipv4Address}]");
                                }

                                logFilePathName = CopyLatestFileFromRoboRio(config);
                            }
                            // the file will come from a local folder
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
                            string xlsFilePathName = GraphBuilder.ProcessLogFile(logFilePathName, o.GraphSetName, config);

                            System.Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine();
                            Console.WriteLine($"Log Data + Graphs in: [{xlsFilePathName}]");
                            System.Console.ResetColor();
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
        private static GraphConfigsBE LoadConfig(string configFileName)
        {
            GraphConfigsBE config = null;

            try
            {
                // deserialze from JSON
                config = JsonConvert.DeserializeObject<GraphConfigsBE>(File.ReadAllText(configFileName));
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
        private static string CopyLatestFileFromRoboRio(GraphConfigsBE config)
        {
            string targetLogFilePathName = string.Empty;

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

                    Dictionary<DateTime, RoboRIOLogFileBE> logFiles = new Dictionary<DateTime, RoboRIOLogFileBE>();

                    // loop thru each log file
                    foreach (SftpFile file in remoteFiles)
                    {
                        // skip directories
                        if (file.IsDirectory) continue;

                        // skip empty files
                        if (file.Length == 0) continue;

                        // skip files with the wrong extension
                        if (Path.GetExtension(file.Name).ToLower() != config.LogFileExtension.ToLower()) continue;

                        // add file to dictionary
                        logFiles.Add(file.LastWriteTime, new RoboRIOLogFileBE()
                                                                {
                                                                    FileName = file.Name,
                                                                    FilePathName = file.FullName,
                                                                    LastModDT = file.LastWriteTime
                                                                });
                    }

                    // sort the list in descending order and pink the newest filename
                    RoboRIOLogFileBE latestLogFile = logFiles.OrderByDescending(f => f.Key).First().Value;

                    // build target file path name
                    targetLogFilePathName = System.IO.Path.Combine(config.LocalWorkingFolder, latestLogFile.FileName);

                    //see if we already have this file
                    if (!System.IO.File.Exists(targetLogFilePathName))
                    {
                        // download the most recent file
                        using (Stream fileStream = File.Create(targetLogFilePathName))
                        {
                            System.Console.WriteLine();
                            System.Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"... Now downloading latest log file: [{targetLogFilePathName}]");
                            System.Console.ResetColor();

                            sftpClient.DownloadFile(latestLogFile.FilePathName, fileStream);
                        }
                    }
                    else
                    {
                        System.Console.WriteLine();
                        System.Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"... Latest log file: [{targetLogFilePathName}] already downloaded");
                        System.Console.ResetColor();
                    }
                }
            }
            catch(Exception ex)
            {
                System.Console.WriteLine($"Error downloading latest file from RoboRIO: {ex}");
            }

            return targetLogFilePathName;
        }

        /// <summary>
        /// Utility to check if a remote address is reachable
        /// </summary>
        /// <param name="server"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private static bool IsServerAvailable(string server, int port)
        {
            using (TcpClient client = new TcpClient())
            {
                try
                {
                    client.Connect(server, port);
                }
                catch (SocketException)
                {
                    return false;
                }
                client.Close();
                return true;
            }
        }
    }
}