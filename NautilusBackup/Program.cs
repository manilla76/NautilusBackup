using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace NautilusBackup
{
    class Program
    {
        static int verbosity;

        static void Main(string[] args)
        {
            bool show_help = false;
            bool zipBackup = false;
            bool quietMode = false;
            string registryPath = @"HKCU\Software\Nautilus";
            var configPath = Environment.GetEnvironmentVariable(@"GM_CFG");
            string xmlPath = @"C:\ProgramData\Thermo\ThermoCfgService";
            string filePath = @"C:\Backup\Nautilus\";
            string datetime = DateTime.Now.Year + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00");
            var p = new OptionSet(){
                { "p|path=","the {Path} to save backup files.",
                    v =>filePath = v},
                { "v","increase debug message verbosity",
                    v =>{if(v!=null)++verbosity;}},
                { "q|quiet","skips confirmation",
                    v =>quietMode=true},
                { "z|zip","zips the backup files",
                    v =>zipBackup=true},
                { "h|help","show this message and exit",
                    v =>show_help=v!=null},
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("NautilusBackup: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `NautilusBackup --help' for more information.");
                return;
            }

            if (show_help)
            {
                ShowHelp(p);
                return;
            }
            if (filePath[filePath.Length - 1] != '\\') filePath = filePath + @"\";
            Directory.CreateDirectory(filePath + datetime + @"\Config");
            ExportRegistry(registryPath, filePath + datetime + @"\nautilus.reg");   // move registry into the backup directory.
            DirectoryCopy(configPath, filePath + datetime + @"\Config", true);
            File.Copy(xmlPath + @"\ThermoCfgService.xml", filePath + datetime + @"\ThermoCfgService.xml", true);
            filePath = filePath + datetime;
            if (zipBackup)
            {
                if (File.Exists(filePath + ".zip")) File.Delete(filePath + ".zip");
                ZipFile.CreateFromDirectory(filePath + @"\", filePath + ".zip");
                Directory.Delete(filePath + @"\", true);
                filePath = filePath + ".zip";
            }
            Console.WriteLine("Backup files stored in: " + filePath);
            if (quietMode) return;
            Console.WriteLine("Press Any Key to Exit");
            Console.ReadKey();

        }

        static void ExportRegistry(string strKey, string filepath)
        {
            try
            {
                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = "reg.exe";
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.RedirectStandardError = true;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.Arguments = "export \"" + strKey + "\" \"" + filepath + "\" /y";
                    proc.Start();
                    string stdout = proc.StandardOutput.ReadToEnd();
                    string stderr = proc.StandardError.ReadToEnd();
                    proc.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                // handle exception
            }
        }

        private static void DirectoryCopy(
        string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);
                // Copy the file.
                file.CopyTo(temppath, true);
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    // Copy the subdirectories.
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: greet [OPTIONS]+ message");
            Console.WriteLine("Greet a list of individuals with an optional message.");
            Console.WriteLine("If no message is specified, a generic greeting is used.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        static void Debug(string format, params object[] args)
        {
            if (verbosity > 0)
            {
                Console.Write("# ");
                Console.WriteLine(format, args);
            }
        }

    }
}
