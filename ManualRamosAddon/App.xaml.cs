using System.Windows;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Ioc;

namespace ManualRamosAddon
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ApplicationViewModel AppVM;
        static App()
        {
            DispatcherHelper.Initialize();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Model.ThermoInterface.Init();
        }
    }
}
