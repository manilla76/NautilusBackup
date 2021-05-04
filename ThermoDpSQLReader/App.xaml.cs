using System;
using System.Windows;

namespace ThermoDpSQLReader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static TaskbarIcon notifyIcon;
        static App()
        {
            DispatcherHelper.Initialize();
            Model.DataManager.Initialize();
            ViewModel.NotifyIconViewModel.OnExiting += NotifyIconViewModel_OnExiting;
        }

        private static void TerminateNow(string msg)
        {
            Cleanup();
        }

        private static void NotifyIconViewModel_OnExiting(object sender, EventArgs e)
        {
            Cleanup();
            Current.Dispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Normal);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //create the notifyicon (it's a resource declared in NotifyIconResources.xaml
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (Current.MainWindow != null)
                Current.MainWindow.Close();
            Cleanup();
            notifyIcon.Visibility = Visibility.Hidden;
            notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner            
            base.OnExit(e);
        }

        public static void Cleanup()
        {
            Model.DataManager.TerminateTag.Dispose();
            Model.DataManager.TagInfo.Dispose();
            Thermo.Datapool.Datapool.DatapoolSvr.Dispose();
        }
    }
}
