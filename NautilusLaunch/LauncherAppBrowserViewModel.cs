using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using BaseLibrary;

namespace OmniLaunch
{
    class LauncherAppBrowserViewModel
    {
        private ICommand dragCommand;
        public ICommand DragCommand => dragCommand ?? (dragCommand = new RelayCommand((p) => 
            {

            }));

        private ICommand dropCommand;
        private ICommand DropCommand => dropCommand ?? (dropCommand = new RelayCommand((p) =>
            {

            }));

        public ObservableCollection<string> AppList { get; }
        
        public LauncherAppBrowserViewModel()
        {
            AppList = new ObservableCollection<string>();
            InitAppList();            
        }

        private void InitAppList()
        {
            string binDir = @"C:\Program Files\Thermo\Nautilus\";
            if (Directory.Exists(@"C:\Program Files (x86)"))
            {
                binDir = @"C:\Program Files (x86)\Thermo\Nautilus\";
            }            
            AddExeFromDirectory(binDir);            
        }

        private void AddExeFromDirectory(string dir)
        {
            foreach(string file in Directory.GetFiles(dir))
            {
                AddExe(file);
            }
            foreach(string d in Directory.GetDirectories(dir))
            {
                AddExeFromDirectory(d);
            }
        }

        private void AddExe(string file)
        {
            if (file.EndsWith(".exe"))
            {
                AppList.Add(file);
            }
        }
    }
}
