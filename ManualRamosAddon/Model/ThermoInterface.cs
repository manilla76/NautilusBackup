using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Thermo.Configuration;
using Thermo.Datapool;


namespace ManualRamosAddon.Model
{
    public static class ThermoInterface
    {
        static Timer T;
        private static ThermoArgonautViewerLibrary.Argonaut omniView = new ThermoArgonautViewerLibrary.Argonaut();
        
        public static void Init()
        {
            T = new Timer();            
            T.Interval = 5000; // 5 sec   5*1000ms
            T.Enabled = true;            
            T.Elapsed += new ElapsedEventHandler(onElapsedEvent);
        }

        private static void onElapsedEvent(object sender, ElapsedEventArgs e)
        {
            var results = omniView.GetRaMOSFeederSetup();
            
            foreach (var feeder in SimpleIoc.Default.GetInstance<ApplicationViewModel>().Feeders)
            {
                feeder.IsManual = results[feeder.FeederNumber].Enabled && results[feeder.FeederNumber].ReadFixedRateFromDatapool && results[feeder.FeederNumber].FixedRateEnabled;

                foreach (var source in feeder.Sources)
                {
                    if (!results.Items.Any(x => x.SourceEstimateName == source.SourceEstimateName))
                        feeder.Sources.Remove(source);
                }
                foreach (var source in results.Items.Select(f => new { f.SourceEstimateName, f.Index }))
                {
                    if (!feeder.Sources.Any(x => x.SourceEstimateName == source.SourceEstimateName))
                        feeder.Sources.Add(new SourceData(source.SourceEstimateName, source.Index));
                }
            }           
        }

        public static void InitSources(ObservableCollection<SourceData> sources)
        {
            var results = omniView.GetRaMOSFeederSetup();
            sources.Clear();
            foreach (var source in results.Items.Select(f => new { f.SourceEstimateName, f.Index }))
                sources.Add(new SourceData(source.SourceEstimateName, source.Index));
        }

        public static void LoadConfig(ApplicationViewModel appVM)
        {
            appVM.NumFeeders = Properties.Settings.Default.NumFeeders;
            if (appVM.NumFeeders < 1)
            {
                try
                {
                    appVM.Feeders.Add(new FeederViewModel() { FeederNumber = 1, MaxDelta = 0.1f, Oxide = "Fe2O3" });
                }
                catch { MessageBox.Show("newError"); }
                return;
            }                
            appVM.Feeders.Add(new FeederViewModel() { FeederNumber = Properties.Settings.Default.F1FeederNum, MaxDelta = Properties.Settings.Default.F1MaxDelta, Oxide = Properties.Settings.Default.F1Oxide });
            if (App.AppVM.NumFeeders < 2)
                return;
            appVM.Feeders.Add(new FeederViewModel() { FeederNumber = Properties.Settings.Default.F2FeederNum, MaxDelta = Properties.Settings.Default.F2MaxDelta, Oxide = Properties.Settings.Default.F2Oxide });
            if (App.AppVM.NumFeeders < 3)
                return;
            appVM.Feeders.Add(new FeederViewModel() { FeederNumber = Properties.Settings.Default.F3FeederNum, MaxDelta = Properties.Settings.Default.F3MaxDelta, Oxide = Properties.Settings.Default.F3Oxide });

        }

        public static void SaveConfig()
        {
            App.AppVM.NumFeeders = Properties.Settings.Default.NumFeeders = App.AppVM.Feeders.Count;
            if (App.AppVM.NumFeeders < 1)
            {
                Properties.Settings.Default.Save();
                return;
            }
            Properties.Settings.Default.F1FeederNum = App.AppVM.Feeders[0].FeederNumber;
            Properties.Settings.Default.F1MaxDelta = App.AppVM.Feeders[0].MaxDelta;
            Properties.Settings.Default.F1Oxide = App.AppVM.Feeders[0].Oxide;
            if (App.AppVM.NumFeeders < 2)
            {
                Properties.Settings.Default.Save();
                return;
            }
            Properties.Settings.Default.F2FeederNum = App.AppVM.Feeders[1].FeederNumber;
            Properties.Settings.Default.F2MaxDelta = App.AppVM.Feeders[1].MaxDelta;
            Properties.Settings.Default.F2Oxide = App.AppVM.Feeders[1].Oxide;
            if (App.AppVM.NumFeeders < 3)
            {
                Properties.Settings.Default.Save();
                return;
            }
            Properties.Settings.Default.F3FeederNum = App.AppVM.Feeders[2].FeederNumber;
            Properties.Settings.Default.F3MaxDelta = App.AppVM.Feeders[2].MaxDelta;
            Properties.Settings.Default.F3Oxide = App.AppVM.Feeders[2].Oxide;
            {
                Properties.Settings.Default.Save();
                return;
            }
        }
    }
}
