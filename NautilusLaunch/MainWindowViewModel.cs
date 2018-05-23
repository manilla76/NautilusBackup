using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Thermo.Configuration;

namespace OmniLaunch
{
    class MainWindowViewModel
    {
        private Branch nautilusCfg;
        private LauncherAppBrowserViewModel launcher = new LauncherAppBrowserViewModel();
        private ObservableCollection<string> appList;
        private ObservableCollection<string> startList;
        public ObservableCollection<string> AppList => appList;
        public ObservableCollection<string> StartList { get => startList; }
        public string SelectedItem { get; set; }
        public MainWindowViewModel()
        {
            startList = new ObservableCollection<string>();
            startList.Add(string.Empty);
            SelectedItem = string.Empty;
            try
            {
                InitConfiguraiton();  // Attempt to load the configuraiton.
            }
            catch (Exception)  // If fails, most likely due to Thermo Config service not running
            {
                MessageBox.Show("CfgService not running. \nStart the Thermo Configuration Service and run OmniLaunch again.");
                Application.Current.Shutdown();
            }
            appList = launcher.AppList;

        }

        private void InitConfiguraiton()
        {
            nautilusCfg = Configuration.GetBranch("nautilus");

            if (!nautilusCfg.Children.Exists((b) => b.Path == "Launcher")) // check for Launcher in CfgService, if missing, read nautiluslaunch.ini
            {
                ReadIni();
                // Configuration.CreatePath(nautilusCfg.Path + @"\Launcher");
                // MessageBox.Show("Launcher Created");
            }
            else  // Exists
            {
                // MessageBox.Show("Launcher Already Exists");
            }
        }

        private void ReadIni()
        {
            StreamReader reader = new StreamReader(Environment.GetEnvironmentVariable("GM_CFG") + @"\NautilusLaunch.ini");
            string currentLine;
            int i = 0;
            while ((currentLine = reader.ReadLine()) != null)
            {
                if (currentLine.Contains(@"<TEXT>")) continue;
                if (currentLine.Contains(@"<CMD>"))
                {
                    Configuration.CreatePath(@"Nautilus\Launcher\Item" + i);
                    int firstPos = currentLine.IndexOf("<CMD>", 0) + 5;
                    int secondPos = currentLine.IndexOf("</CMD>");
                    Configuration.SetValue(@"Nautilus\Launcher\Item" + i, "CMD", currentLine.Substring(firstPos, secondPos - firstPos));
                    firstPos = currentLine.IndexOf("<PARAM>", 0) + 7;
                    secondPos = currentLine.IndexOf("</PARAM>");
                    Configuration.SetValue(@"Nautilus\Launcher\Item" + i, "PARAM", currentLine.Substring(firstPos, secondPos - firstPos));
                    firstPos = currentLine.IndexOf("<START>", 0) + 7;
                    secondPos = currentLine.IndexOf("</START>");
                    Configuration.SetValue(@"Nautilus\Launcher\Item" + i, "START", currentLine.Substring(firstPos, secondPos - firstPos));
                    i++;
                }
            }
        }
    }
}
