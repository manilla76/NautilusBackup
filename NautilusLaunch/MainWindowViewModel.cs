using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Thermo.Configuration;
using BaseLibrary;

namespace OmniLaunch
{
    class MainWindowViewModel : ViewModelBase
    {
        private Branch nautilusCfg;
        private LauncherAppBrowserViewModel launcher = new LauncherAppBrowserViewModel();
        private int selectedProgram;
        public int SelectedProgram { get => selectedProgram; set => Set(ref selectedProgram, value); }
        public ProgramModel SelectedItem { get => StartList[SelectedIndex]; }
        private int selectedIndex;
        public int SelectedIndex { get => selectedIndex; set => Set(ref selectedIndex, value); }
        private ObservableCollection<ProgramModel> startList;
        private ICommand dragCommand;
        public ICommand DragCommand => dragCommand ?? (dragCommand = new RelayCommand((p)=> 
            {

            }));

        private ICommand appendCommand;
        public ICommand AppendCommand => appendCommand ?? (appendCommand = new RelayCommand((p) =>
        {
            StartList.Add(new ProgramModel { Path = AppList[SelectedProgram], Parameters = string.Empty, StartType = StartType.none });            
        }));
        private ICommand insertCommand;
        public ICommand InsertCommand => insertCommand ?? (insertCommand = new RelayCommand((p) =>
        {
            StartList.Insert(SelectedIndex, new ProgramModel { Path = AppList[SelectedProgram], Parameters = string.Empty, StartType = StartType.none });
        }));
        private ICommand deleteCommand;
        public ICommand DeleteCommand => deleteCommand ?? (deleteCommand = new RelayCommand((p) =>
        {
            StartList.RemoveAt(SelectedIndex);               
            SelectedIndex = 0;
        }));

        public ObservableCollection<string> AppList { get; private set; }
        public ObservableCollection<ProgramModel> StartList { get => startList; set => Set(ref startList, value); }
        public MainWindowViewModel()
        {
            try
            {
                
                var temp = OPC.Common.ComApi.EnumComputers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            startList = new ObservableCollection<ProgramModel>
            {
                new ProgramModel { Parameters = string.Empty, Path = string.Empty, StartType = StartType.none }
            };
            StartList.CollectionChanged += StartList_CollectionChanged;
            SelectedIndex = 0;
            try
            {
                InitConfiguraiton();  // Attempt to load the configuraiton.
                SetupTestData();
            }
            catch (Exception)  // If fails, most likely due to Thermo Config service not running
            {
                MessageBox.Show("CfgService not running. \nStart the Thermo Configuration Service and run OmniLaunch again.");
                Application.Current.Shutdown();
            }
            AppList = launcher.AppList;

        }

        private void StartList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnProptertyChanged("StartList");            
        }

        private void SetupTestData()
        {
            startList.Clear();
            startList.Add(new ProgramModel { Path = @"C:\Program Files (x86)\Thermo\Nautilus\Omni\Viewer\OmniView.exe", Parameters = "/administrator", StartType = StartType.none });
            startList.Add(new ProgramModel { Path = @"C:\Program Files (x86)\Thermo\Nautilus\SpecAnalysis.exe", Parameters = string.Empty, StartType = StartType.none });
            startList.Add(new ProgramModel { Path = @"C:\Program Files (x86)\Thermo\Nautilus\Car.exe", Parameters = string.Empty, StartType = StartType.none });
            startList.Add(new ProgramModel { Path = @"C:\Program Files (x86)\Thermo\Nautilus\Opc\ThermoOpcClient\ThermoOpcClient.exe", Parameters = string.Empty, StartType = StartType.none });
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
