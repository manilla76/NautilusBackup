using System.Windows;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Ioc;
using Hardcodet.Wpf.TaskbarNotification;

namespace ManualRamosAddon
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static TaskbarIcon notifyIcon;
        public static ApplicationViewModel AppVM;
        static App()
        {
            DispatcherHelper.Initialize();
            ViewModel.NotifyIconViewModel.OnExiting += NotifyIconViewModel_OnExiting;

            ViewModel.ViewModelLocator.Init();
            AppVM = SimpleIoc.Default.GetInstance<ApplicationViewModel>();
        }

        private static void NotifyIconViewModel_OnExiting(object sender, System.EventArgs e)
        {
            Model.ThermoInterface.SaveConfig();
            Current.Dispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Normal);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //create the notifyicon (it's a resource declared in NotifyIconResources.xaml
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
            Model.ThermoInterface.Init();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (Current.MainWindow != null)
                Current.MainWindow.Close();
            notifyIcon.Visibility = Visibility.Hidden;
            notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner            
            base.OnExit(e);
        }
    }
}
