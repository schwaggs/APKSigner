using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Autozip
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            while (!Initialization())
            {
                Console.WriteLine("Error, retry?  <Y/N>:  ");
                string retry = Console.ReadLine();

                if (retry != "Y" || retry != "y")
                {
                    Environment.Exit(1);
                }
            }

            string apk_unsigned = "";
            string apk_signed = "";
            string keystore = "";

            string zipalign = Properties.Settings.Default.ToolsPath + @"\zipalign.exe";
            string apksigner = Properties.Settings.Default.ToolsPath + @"\apksigner.bat";

            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = false,
                Title = "Choose location of unsigned apk",
                DefaultExtension = ".apk",
                InitialDirectory = @"C:\"
            };

            dialog.Filters.Add(new CommonFileDialogFilter("APK Files", "*.apk"));

            CommonFileDialogResult res = dialog.ShowDialog();

            if (res == CommonFileDialogResult.Ok)
            {
                if (!string.IsNullOrEmpty(dialog.FileName) && !string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    apk_unsigned = dialog.FileName;
                }
            }
            else
            {
                Environment.Exit(1);
            }

            dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = false,
                DefaultExtension = ".keystore",
                Title = "Choose location of keystore file.",
                InitialDirectory = @"C:\"
            };

            dialog.Filters.Add(new CommonFileDialogFilter("KEYSTORE Files", "*.keystore"));

            res = dialog.ShowDialog();

            if (res == CommonFileDialogResult.Ok)
            {
                if (!string.IsNullOrEmpty(dialog.FileName) && !string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    keystore = dialog.FileName;
                }
            }
            else
            {
                Environment.Exit(1);
            }

            dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = false,
                Title = "Choose where to save signed apk.",
                DefaultExtension = ".apk",
                InitialDirectory = @"C:\"
            };

            dialog.Filters.Add(new CommonFileDialogFilter("APK Files", "*.apk"));

            res = dialog.ShowDialog();

            if (res == CommonFileDialogResult.Ok)
            {
                if (!string.IsNullOrEmpty(dialog.FileName) && !string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    apk_signed = dialog.FileName;
                }
            }
            else
            {
                Environment.Exit(1);
            }

            string zipArgs = "-f -v 4 " + apk_unsigned + " " + apk_signed;
            string signArgs = "sign --ks " + keystore + " " + apk_signed;

            System.Diagnostics.Process.Start(zipalign, zipArgs).WaitForExit();
            System.Diagnostics.Process.Start(apksigner, signArgs).WaitForExit();
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
                    Title = "Choose location of android tools",
                    InitialDirectory = initialDir
                };

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
