using System;
using System.Diagnostics;
using System.IO;

namespace LogFileCleaner
{
    class Program
    {
        static void Main(string[] args)
        {
            string DateString = DateTime.Now.ToString().Replace("/", "-").Replace(":", ".");
            string WorkingDir = @"C:\Users\Public\Documents\FileJanitor";
            string SettingsFilePath = @"C:\Users\Public\Documents\FileJanitor\FileJanitor.ini";
            string LogFilePath = @"C:\Users\Public\Documents\FileJanitor\" + $"Cleanup Log {DateString}.txt";

            LoadSettings();
            CheckFileSize();         

            
            void LoadSettings()
            {
                if (File.Exists(SettingsFilePath))
                {
                    //Settings File Exists, load setup
                    using (StreamReader SettingsFile = new StreamReader(SettingsFilePath))
                    {
                        String singleSetting;
                        int SettingsLoaded = 0;
                        while ((singleSetting = SettingsFile.ReadLine()) != null)
                        {
                            SettingsLoaded += 1;
                            switch (SettingsLoaded)
                            {
                                case 1:
                                    _settings.TargetFilePath = singleSetting;
                                    break;
                                case 2:
                                    _settings.MaxFileSize = Convert.ToInt32(singleSetting);
                                    break;
                                case 3:
                                    _settings.ArchiveDir = singleSetting;
                                    break;
                                case 4:
                                    _settings.ServiceName = singleSetting;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    //Settings File does not exist.Ask for settings and save to file
                    bool RunSetup = true;

                    string fTargetFilePath = "";
                    string fMaxSize = "";
                    string fArchivePath = "";
                    string fServiceName = "#n/a#";

                    while (RunSetup)
                    {
                        Console.Clear();

                        Console.WriteLine(@"           ");
                        Console.WriteLine(@"            _____ _____ _____ _   _______ ");
                        Console.WriteLine(@"           /  ___|  ___|_   _| | | | ___ \");
                        Console.WriteLine(@"           \ `--.| |__   | | | | | | |_/ /");
                        Console.WriteLine(@"            `--. \  __|  | | | | | |  __/ ");
                        Console.WriteLine(@"           /\__/ / |___  | | | |_| | |    ");
                        Console.WriteLine(@"           \____/\____/  \_/  \___/\_|    ");
                        Console.WriteLine(@"                                          ");

                        logging.WriteLine("======================WARNING=======================");
                        logging.WriteLine("   THIS WIZARD DOES NOT DO ANY INPUT VALIDATION");

                        logging.WriteLine("\nEnter the full path to the targeted file:");
                        fTargetFilePath = Console.ReadLine();


                        logging.WriteLine("\nEnter the max size the file can be (in MB):");
                        fMaxSize = Console.ReadLine();

                        logging.WriteLine("\nEnter the full path to the archive directory or use #delete# to delete over-sized files:");
                        fArchivePath = Console.ReadLine();

                        fServiceName = "#n/a#";


                        logging.WriteLine("\nWould you like to start/stop a service during processing? [Y/N]");
                        if (Console.ReadLine().ToUpper() == "Y")
                        {
                            logging.WriteLine("\nPlease note: In order to use this function the cleanup must be run with admin rights");
                            logging.WriteLine("\nEnter the name of the service to start/stop during processing:");
                            fServiceName = Console.ReadLine();
                        }

                        logging.WriteLine("\nSave settings, N to restart setup? [Y/N]");
                        var RestartSetup = Console.ReadLine().ToUpper() != "Y" ? RunSetup = true : RunSetup = false;

                    }

                    if (!Directory.Exists(WorkingDir))
                    {
                        Directory.CreateDirectory(WorkingDir);
                    }

                    using (StreamWriter SettingsFile = new StreamWriter(SettingsFilePath))
                    {
                        SettingsFile.WriteLine(fTargetFilePath);
                        SettingsFile.WriteLine(fMaxSize);
                        SettingsFile.WriteLine(fArchivePath);
                        SettingsFile.WriteLine(fServiceName);
                    }

                    logging.WriteLine($"\nSettings saved to {SettingsFilePath}");
                    logging.WriteLine("To run setup again, simply delete the settings file.");

                    logging.WriteLine($"\nTo run cleanup, simply call me at {Environment.GetCommandLineArgs()[0].Replace(".dll",".exe")}");
                    logging.WriteLine("\nPress any key to exit...");
                    Console.ReadLine();
                    Environment.Exit(0);
                    
                }
            }

            void CheckFileSize()
            {
                logging.WriteLine("\nChecking file size");
                //Check if the file actually exists
                if (File.Exists(_settings.TargetFilePath))
                {
                    //The file exists, we gather the file info
                    //and check the size of the file.
                    //If the file size is greater than the set max size we rename it
                    //and move it to the archive
                    FileInfo LogFile_FileInfo = new FileInfo(_settings.TargetFilePath);

                    if (LogFile_FileInfo.Length / (1048576) > _settings.MaxFileSize)
                    {
                        logging.WriteLine($"File is larger than {_settings.MaxFileSize}MB.");
                        if (_settings.ArchiveDir.ToLower() == "#delete#")
                        {
                            logging.WriteLine("Deleting file...\n");
                        }
                        else
                        {
                            logging.WriteLine($"Moving file to {_settings.ArchiveDir}\n");
                        }

                        if (_settings.ServiceName != "#n/a#")
                        {
                            //Stop the target Service
                            ToggleService(_settings.ServiceName, "STOP");
                        }
                        

                       
                        string NewFileName = Path.Combine(_settings.ArchiveDir, DateString + " - " + _settings.TargetFilePath.Substring(_settings.TargetFilePath.LastIndexOf("\\") + 1));

                        if (_settings.ArchiveDir != "#delete#")
                        {
                            File.Copy(_settings.TargetFilePath, NewFileName);
                            File.Delete(_settings.TargetFilePath);
                            logging.WriteLine($"File moved to {NewFileName}");
                        }
                        else
                        {
                            File.Delete(_settings.TargetFilePath);
                            logging.WriteLine($"File deleted");
                        }

                        if (_settings.ServiceName != "#n/a#")
                        {
                            //Start the Service again
                            ToggleService(_settings.ServiceName, "start");
                        }

                    }
                    logging.WriteLine("File size stable");
                }

                logging.SaveLog(false, LogFilePath);

                Console.WriteLine("\n\nAuto closing in 15 seconds");
                Console.ReadLine();
                System.Threading.Thread.Sleep(15000);
            }

            void ToggleService(string ServiceName, string ExplicitToggle = "toggle")
            {
                Process ServiceProcess = new Process();
                ServiceProcess.StartInfo.FileName = "cmd.exe";
                ServiceProcess.StartInfo.RedirectStandardInput = true;
                ServiceProcess.StartInfo.RedirectStandardOutput = true;
                ServiceProcess.StartInfo.CreateNoWindow = true;
                ServiceProcess.StartInfo.UseShellExecute = false;
                ServiceProcess.Start();

                ServiceProcess.StandardInput.WriteLine($"sc {ExplicitToggle} {ServiceName}");
                ServiceProcess.StandardInput.Flush();
                ServiceProcess.StandardInput.Close();
                ServiceProcess.WaitForExit();
                logging.WriteLine(ServiceProcess.StandardOutput.ReadToEnd());
            }


        }


    }
}
