using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Autozip
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (!Properties.Settings.Default.HasTools)
            {
                Console.WriteLine("Do you have a keystore file downloaded and the android sdk build tools installed? <Y/N>:  ");
                string response = Console.ReadLine();
                
                if (response == "Y" || response == "y")
                {
                    Properties.Settings.Default.HasTools = true;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    Console.WriteLine(response != "y");
                    Console.WriteLine("Manually signing an apk requires these, please correct and re-run. Press enter to continue.");
                    Console.ReadLine();
                    Environment.Exit(1);
                }
            }

            string inputFile = "";
            string outputFile = "";

            if (args.Length > 0)
            {
                FileInfo apkInfo = new FileInfo(args[0]);

                inputFile = apkInfo.FullName;
                outputFile = apkInfo.DirectoryName + @"\" + apkInfo.Name.Substring(0, apkInfo.Name.Length - 4) + @"_signed.apk";

                if (File.Exists(outputFile))
                {
                    try
                    {
                        File.Delete(outputFile);
                    }
                    catch
                    {
                        Console.WriteLine("Unable to delete previous signed apk, please delete and re-run.");
                        Environment.Exit(1);
                    }
                }

                while (!Initialization())
                {
                    Console.WriteLine("Error, retry?  <Y/N>:  ");
                    string retry = Console.ReadLine();

                    if (retry != "Y" || retry != "y")
                    {
                        Environment.Exit(1);
                    }
                }

                string zipalign = Properties.Settings.Default.ToolsPath + @"\zipalign.exe";
                string apksigner = Properties.Settings.Default.ToolsPath + @"\apksigner.bat";

                if (Properties.Settings.Default.KeystorePath == "")
                {
                    CommonOpenFileDialog dialog = new CommonOpenFileDialog
                    {
                        IsFolderPicker = false,
                        DefaultExtension = ".keystore",
                        Title = "Choose location of keystore file.",
                        InitialDirectory = @"C:\"
                    };

                    dialog.Filters.Add(new CommonFileDialogFilter("KEYSTORE Files", "*.keystore"));

                    CommonFileDialogResult res = dialog.ShowDialog();

                    if (res == CommonFileDialogResult.Ok)
                    {
                        if (!string.IsNullOrEmpty(dialog.FileName) && !string.IsNullOrWhiteSpace(dialog.FileName))
                        {
                            if(File.Exists(dialog.FileName))
                            {
                                Properties.Settings.Default.KeystorePath = dialog.FileName;
                                Properties.Settings.Default.Save();
                            }
                            else
                            {
                                Console.WriteLine("Invalid keystore location, please re-run. Press enter to continue.");
                                Console.ReadLine();
                            }
                        }
                    }
                    else
                    {
                        Environment.Exit(1);
                    }
                }
                
                string zipArgs = "-f -v 4 " + inputFile + " " + outputFile;
                string signArgs = "sign --ks-pass pass:Password@001 --ks \"" + Properties.Settings.Default.KeystorePath + "\" \"" + outputFile + "\"";
                
                Process proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = zipalign,
                        Arguments = zipArgs,
                        ErrorDialog = true,
                        WindowStyle = ProcessWindowStyle.Normal
                    }
                };
                
                proc.Start();
                proc.WaitForExit();

                proc.StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    FileName = apksigner,
                    Arguments = signArgs,
                    ErrorDialog = true,
                    RedirectStandardError = true,
                    WindowStyle = ProcessWindowStyle.Normal
                };
                
                proc.Start();
                proc.WaitForExit();
            }
        }

        private static bool Initialization()
        {
            string defaultDir = @"C:\Program Files (x86)\Android\android-sdk\build-tools";
            string initialDir = "";

            if (Directory.Exists(defaultDir))
            {
                initialDir = defaultDir;
            }
            else
            {
                initialDir = @"C:\";
            }

            if(Properties.Settings.Default.ToolsPath == "" || !ContainsTools())
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog
                {
                    IsFolderPicker = true,
                    Title = "Choose location of android sdk build-tools",
                    InitialDirectory = initialDir
                };

                Console.WriteLine("The build tools will be under build-tools.\nFrom there you can use any version of the sdk with a minimum of 26.");
                Console.WriteLine("Just select that version's folder and click ok.");

                CommonFileDialogResult res = dialog.ShowDialog();

                if (res == CommonFileDialogResult.Ok)
                {
                    if(!string.IsNullOrEmpty(dialog.FileName) && !string.IsNullOrWhiteSpace(dialog.FileName))
                    {
                        Properties.Settings.Default.ToolsPath = dialog.FileName;

                        if(!ContainsTools())
                        {
                            Properties.Settings.Default.ToolsPath = "";
                            return false;
                        }
                        else
                        {
                            Properties.Settings.Default.Save();
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        private static bool ContainsTools()
        {
            string zipalign = @"\zipalign.exe";
            string apksign = @"\apksigner.bat";

            if(File.Exists(Properties.Settings.Default.ToolsPath + zipalign) && File.Exists(Properties.Settings.Default.ToolsPath + apksign))
            {
                return true;
            }

            return false;
        }
    }
}
