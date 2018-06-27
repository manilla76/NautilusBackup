using GalaSoft.MvvmLight.Ioc;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using static ThermoArgonautLibrary.CommonSystem;
using static ThermoArgonautViewerLibrary.CommonSystemViewer;
using BlendingOptimizationEngine;
using System;
using Thermo.Datapool;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Windows;
using System.ServiceProcess;
using System.Security.Principal;
using System.Threading;

namespace ManualRamosAddon.Model
{
    public struct Coeff
    {
        public string Name;
        public string Value;
    }

    public static class ThermoInterface
    {
        static System.Timers.Timer T;
        static bool noRamos = false;
        public static ThermoArgonautViewerLibrary.Argonaut OmniView = new ThermoArgonautViewerLibrary.Argonaut();
        public static BOSCtrl Ramos;
        public static ServerItemUpdate ServerItemUpdate;
        public static Datapool.ITagInfo SwitchTag;
        public static Datapool.ITagInfo StartTimeTag;
        public static Datapool.ITagInfo StopTimeTag;
        public static Datapool.ITagInfo IntervalTag;
        public static Datapool.ITagInfo BusyTag;
        private static Dictionary<string, BlendingOptimizationSystem.Feeders> profile;
        private static BlendingOptimizationSystem.Feeders feeders = new BlendingOptimizationSystem.Feeders();
        private static BlendingOptimizationSystem.BlendRecipe recipe = new BlendingOptimizationSystem.BlendRecipe();
        private static readonly List<Datapool.DPGroupTagValue> tagList = new List<Datapool.DPGroupTagValue>();
        private static readonly List<string> tagExclude = new List<string> { "analysisid", "ProductUniqueId", "DateBegin", "DateEnd", "SubName", "IsCalibration" };

        public static bool AutoSerivceRestart { get; private set; }

        public static void Init()
        {
            T = new System.Timers.Timer
            {
                Interval = 400, // 0.4 sec   0.4*1000ms
                Enabled = !noRamos
            };
            T.Elapsed += new ElapsedEventHandler(OnElapsedEvent);
            
            Ramos = new BOSCtrl(null);
            Ramos.SolveReport.UpdateEvent += SolveReport_UpdateEvent;
            profile = profile ?? new Dictionary<string, BlendingOptimizationSystem.Feeders>();
            try
            {
                var feeders = OmniView.GetRaMOSFeederSetup();
      
            //Ramos.Feeders.UpdateEvent += Feeders_UpdateEvent;
            ServerItemUpdate = new ServerItemUpdate();
            ServerItemUpdate.IsUpdated(ServerItemUpdate.ItemsEnum.UpdateProduct, OmniView.GetItemUpdate());
            
            //feeders = OmniView.GetRaMOSFeederSetup();
            feeders.UpdateEvent += Feeders_UpdateEvent;
            recipe.UpdateEvent += Recipe_UpdateEvent;
            AutoSerivceRestart = true;
            if (AutoSerivceRestart)
                UpdateTag(new Datapool.DPGroupTagName(@"RAMOS.Status", @"Busy", Datapool.dpTypes.BOOL), "Busy");
            }
            catch
            {

            }
        }
        
        private static void Recipe_UpdateEvent(BlendingOptimizationSystem.BlendRecipe e)
        {            
            if (profile != null && profile.ContainsKey(e.RecipeName))
            {
                // get the profile for the new recipe (if exists), otherwise do nothing then update Ramos
                OmniView.SetRaMOSFeederSetup(profile[e.RecipeName]);
            }
        }
        private static void Feeders_UpdateEvent(BlendingOptimizationSystem.Feeders e)
        {
            // When the Feeder Configuration updates, update the dictionary.
            var tempFeeder = JsonConvert.SerializeObject(e);
            if (profile != null && profile.ContainsKey(recipe.RecipeName))
            {
                // Update existing profile
                profile[recipe.RecipeName] = JsonConvert.DeserializeObject<BlendingOptimizationSystem.Feeders>(tempFeeder);
            }
            else
            {
                // Add New Profile
                profile.Add(recipe.RecipeName, JsonConvert.DeserializeObject<BlendingOptimizationSystem.Feeders>(tempFeeder));
            }
            
            Properties.Settings.Default.FeederDictionary = JsonConvert.SerializeObject(profile);  // Save profile
            Properties.Settings.Default.Save();
        }
        internal static void InitOxides(ObservableCollection<string> oxides)
        {
            var tags = Datapool.DatapoolSvr.ReadTagNames();
            if (tags == null) return;
            var group = tags.m_tagNames.Where((f) => f.m_group=="RAMOS.Predicted").ToList();

            var temp = group.Select((f) => f.m_tag).ToList();
            if (temp == null)
                return;
            oxides.Clear();
            foreach (var a in temp)
            {
                oxides.Add(a);
            }
        }
        private static void ProcessSolve()
        {
            //TODO: Evaluate each feeder and modify the feeder demand as appropriate.
            foreach (FeederViewModel feeder in App.AppVM.Feeders)
            {
                if (feeder.IsManual != true)  // if disabled, skip
                    continue;
                // get target & tolerance for the oxide for the current recipe
                recipe = OmniView.GetRaMOSRecipe();
                var feed = OmniView.GetRaMOSFeederSetup();
                
                var curRecipe = recipe.Items.Where((r) => r.QcName == feeder.Oxide).FirstOrDefault();
                //recipe = Ramos.Recipe.Items.Where((f) => f.QcName == feeder.Oxide).FirstOrDefault();
                if (curRecipe == null) return;
                double setpoint = feeder.Setpoint = curRecipe.Setpoint;
                double tolerance = feeder.Tolerance = curRecipe.Tolerance;
                bool basis = feeder.IsLf = curRecipe.FirstBasis;  // true = lf    false = db
                // get rolling average for the oxide

                Datapool.DatapoolSvr.Read("Rolling.Analysis1." + (basis ? "Loss free" : "Dry basis"), feeder.Oxide, out double rolling);
                feeder.Rolling = rolling;
                // Check for starvation/plugged feeder
                feeder.IsPlugged = Ramos.SolveReport.Feeder[feeder.FeederNumber].Plugged;
                if (feeder.IsPlugged)
                {
                    return;
                }
                // get current demand for this feeder
                double demand = feeder.CurrentDemand = Ramos.SolveReport.Feeder[feeder.FeederNumber].Demand * 100;
                
                // compare
                // calculate new demand
                 
                if (setpoint + tolerance < rolling)
                {
                    feeder.Error = (float)(setpoint - rolling); // above target (error < 0)
                }
                else if (setpoint - tolerance > rolling)
                {
                    feeder.Error = (float)(setpoint - rolling); // below target (error > 0)
                }
                else
                {
                    feeder.Error = (float)(setpoint - rolling);  // inside the target window                    
                }
                feeder.ErrorSum += feeder.Error;
                feeder.ErrorSum = (feeder.ErrorSum < -(App.AppVM.MaxError)) ? -App.AppVM.MaxError : (feeder.ErrorSum > App.AppVM.MaxError) ? App.AppVM.MaxError : feeder.ErrorSum;
                double demandOffset = 0f;
                demandOffset += feeder.Error * App.AppVM.Kp * App.AppVM.ControlPeriod;  // p term
                demandOffset += (feeder.Error - feeder.PrevError) * App.AppVM.Kd; // d term
                demandOffset += feeder.ErrorSum * App.AppVM.Ki; // i term
                demandOffset = (demandOffset < -(feeder.MaxDelta)) ? -feeder.MaxDelta : (demandOffset > feeder.MaxDelta) ? feeder.MaxDelta : demandOffset;  // restrict to Max Delta
                demand += demandOffset;
                demand = (demand < 0) ? 0 : (demand > 100) ? 100 : demand;
                feeder.PrevError = feeder.Error;  // update previous error
                //double offset = diff * feeder.FeederAggression / 1000;
                //demand += (offset >= 0) ? ((offset > feeder.MaxDelta) ? feeder.MaxDelta : offset) : ((offset < -(feeder.MaxDelta)) ? -(feeder.MaxDelta) : offset);
                //// write demand
                Datapool.DatapoolSvr.Write("RAMOS.Feeder.Read", "Set fixed rate% #" + (feeder.FeederNumber+1).ToString(), demand);
            }
        }
        private static void OnElapsedEvent(object sender, ElapsedEventArgs e)  // Runs every T (timer) seconds.  See Init() for T value.
        {
            var results = OmniView.GetRaMOSFeederSetup();
            if (results == null) return;
            foreach (var feeder in SimpleIoc.Default.GetInstance<ApplicationViewModel>().Feeders)
            {
                feeder.IsManual = results[feeder.FeederNumber].Enabled && results[feeder.FeederNumber].ReadFixedRateFromDatapool && results[feeder.FeederNumber].FixedRateEnabled;


                try
                {
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
                catch (Exception)
                {

                }
            }

            ServerItemUpdate currentUpdate = OmniView.GetItemUpdate();
            if (OmniView.CommunicationUp() == Thermo.Communication.Communication.ClientCom.triState.True)
            {
                if (ServerItemUpdate.IsUpdated(ServerItemUpdate.ItemsEnum.RaMOSSolveReport, currentUpdate))
                    Ramos.SolveReport.Assign(OmniView.GetRaMOSSolveReport());
                if (ServerItemUpdate.IsUpdated(ServerItemUpdate.ItemsEnum.RaMOSRecipe, currentUpdate))
                    recipe.Assign(OmniView.GetRaMOSRecipe());
                if (ServerItemUpdate.IsUpdated(ServerItemUpdate.ItemsEnum.RaMOSFeeders, currentUpdate))
                    feeders.Assign(OmniView.GetRaMOSFeederSetup());
            }
        }
        public static void UpdateTag(Datapool.DPGroupTagName tag, string dest)
        {
            if (tag == null)
                return;
            switch (dest)
            {
                case "Switch":
                    if (SwitchTag != null)
                        SwitchTag.Dispose();
                    SwitchTag = Datapool.DatapoolSvr.CreateTagInfo(tag.m_group, tag.m_tag, tag.m_type);
                    SwitchTag.UpdateValueEvent += SwitchTag_UpdateValueEvent;
                    SwitchTag_UpdateValueEvent(SwitchTag);
                    break;
                case "StartTime":
                    if (StartTimeTag != null)
                        StartTimeTag.Dispose();
                    StartTimeTag = Datapool.DatapoolSvr.CreateTagInfo(tag.m_group, tag.m_tag, tag.m_type);
                    StartTimeTag.UpdateValueEvent += StartTimeTag_UpdateValueEvent;
                    StartTimeTag_UpdateValueEvent(StartTimeTag);
                    break;
                case "StopTime":
                    if (StopTimeTag != null)
                        StopTimeTag.Dispose();
                    StopTimeTag = Datapool.DatapoolSvr.CreateTagInfo(tag.m_group, tag.m_tag, tag.m_type);
                    StopTimeTag.UpdateValueEvent += StopTimeTag_UpdateValueEvent;
                    StopTimeTag_UpdateValueEvent(StopTimeTag);
                    break;
                case "Calculate":
                    if (IntervalTag != null)
                        IntervalTag.Dispose();
                    IntervalTag = Datapool.DatapoolSvr.CreateTagInfo(tag.m_group, tag.m_tag, tag.m_type);
                    IntervalTag.UpdateValueEvent += IntervalTag_UpdateValueEvent;
                    IntervalTag_UpdateValueEvent(IntervalTag);
                    break;
                case "Busy":
                    if (BusyTag != null)
                        BusyTag.Dispose();
                    BusyTag = Datapool.DatapoolSvr.CreateTagInfo(tag.m_group, tag.m_tag, tag.m_type);
                    BusyTag.UpdateValueEvent += BusyTag_UpdateValueEvent;
                    BusyTag_UpdateValueEvent(BusyTag);
                    break;
                default:
                    break;
            }
        }

        private static void IntervalTag_UpdateValueEvent(Datapool.ITagInfo e)
        {
            if (IntervalTag.AsBoolean == false)
                return;
            DateTime start, stop;
            start = DateTime.Now;
            stop = DateTime.Now;
            if (StartTimeTag != null) {
                try
                {
                    start = DateTime.Parse(StartTimeTag.AsString);
                }
                catch
                {
                    start = DateTime.Now;
                }
            }
            if (StopTimeTag != null)
            {
                try
                {
                    stop = DateTime.Parse(StopTimeTag.AsString);
                }
                catch
                {
                    stop = DateTime.Now;
                }
            }
            CreateInterval(start, stop);
            IntervalTag.Write(false);
        }

        private static void StopTimeTag_UpdateValueEvent(Datapool.ITagInfo e)
        {
            /* Do nothing */
        }

        private static void StartTimeTag_UpdateValueEvent(Datapool.ITagInfo e)
        {
            /* Do nothing */
        }

        private static void SolveReport_UpdateEvent(BlendControl.SolveReport e)  // Runs after Ramos Solve  
        {
            App.AppVM.SolveCount++;
            if (App.AppVM.SolveCount >= App.AppVM.ControlPeriod)
            {
                App.AppVM.SolveCount = 0;
                ProcessSolve();
            }
        }
        private static void SwitchTag_UpdateValueEvent(Datapool.ITagInfo e)  // type Switch tag value changed
        {
            try
            {
                BlendControl.BlendConfig blendControl = OmniView.GetRaMOSConfiguration();
                blendControl.RecipeName = e.AsString;
                OmniView.SetRaMOSConfiguration(blendControl);     // Set Recipe in OmniView
                if (profile.ContainsKey(blendControl.RecipeName))
                {
                    OmniView.SetRaMOSFeederSetup(profile[blendControl.RecipeName]);   // if feeder profile exists, set it in OmniView.
                }
                App.AppVM.ErrorCode = string.Empty;
            }
            catch (Exception)
            {
                App.AppVM.ErrorCode = "Recipe in switch tag not valid. No change to current recipe.";
            }
        }

        private static void BusyTag_UpdateValueEvent(Datapool.ITagInfo e)
        {
            if (BusyTag.AsBoolean)
            {
                Thread.Sleep(5000);
                Datapool.DatapoolSvr.Read("RAMOS.Status", "Busy", out bool tempRead);
                if (tempRead && (new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator))
                    RebootService("ThermoScientificOmniViewService");
            }
        }

        public static void InitSources(ObservableCollection<SourceData> sources)
        {
            try
            {
                var results = OmniView.GetRaMOSFeederSetup();
                sources.Clear();
                foreach (var source in results.Items.Select(f => new { f.SourceEstimateName, f.Index }))
                    sources.Add(new SourceData(source.SourceEstimateName, source.Index));
            }
            catch (Exception)
            {
                return;         //  Ramos wasn't running...sources don't get initialized.  Program should message and close but not sure how to do that.
                
            }            

        }
        public static void LoadConfig()
        {
            App.AppVM.NumFeeders = Properties.Settings.Default.NumFeeders;
            App.AppVM.ControlPeriod = Properties.Settings.Default.ControlPeriod;
            App.AppVM.Kp = Properties.Settings.Default.Kp;
            App.AppVM.Ki = Properties.Settings.Default.Ki;
            App.AppVM.Kd = Properties.Settings.Default.Kd;
            App.AppVM.SwitchTag = JsonConvert.DeserializeObject<Datapool.DPGroupTagName>(Properties.Settings.Default.SwitchTag);
            App.AppVM.StartTimeTag = JsonConvert.DeserializeObject<Datapool.DPGroupTagName>(Properties.Settings.Default.StartTimeTag);
            App.AppVM.StopTimeTag = JsonConvert.DeserializeObject<Datapool.DPGroupTagName>(Properties.Settings.Default.StopTimeTag);
            App.AppVM.CalculateTag = JsonConvert.DeserializeObject<Datapool.DPGroupTagName>(Properties.Settings.Default.CalculateTag);
            profile = JsonConvert.DeserializeObject<Dictionary<string, BlendingOptimizationSystem.Feeders>>(Properties.Settings.Default.FeederDictionary);
            if (profile != null)
            {
                try
                {
                    recipe = OmniView.GetRaMOSRecipe();
                    if (profile.ContainsKey(recipe.RecipeName))
                    {
                        OmniView.SetRaMOSFeederSetup(profile[recipe.RecipeName]);   // if feeder profile exists, set it in OmniView.
                    }
                    else
                        profile.Add(recipe.RecipeName, OmniView.GetRaMOSFeederSetup());
                }
                catch
                {
                    noRamos = true;
                }
            }

            if (App.AppVM.NumFeeders < 1)
            {
                //try
                //{
                //    App.AppVM.Feeders.Add(new FeederViewModel() { FeederNumber = 1, MaxDelta = 0.1f, Oxide = "Fe2O3" });
                //}
                //catch { MessageBox.Show("newError"); }
                return;
            }
            App.AppVM.Feeders.Add(new FeederViewModel() { FeederNumber = Properties.Settings.Default.F1FeederNum, MaxDelta = Properties.Settings.Default.F1MaxDelta, Oxide = Properties.Settings.Default.F1Oxide, FeederAggression=Properties.Settings.Default.F1Agg });
            if (App.AppVM.NumFeeders < 2)
                return;
            App.AppVM.Feeders.Add(new FeederViewModel() { FeederNumber = Properties.Settings.Default.F2FeederNum, MaxDelta = Properties.Settings.Default.F2MaxDelta, Oxide = Properties.Settings.Default.F2Oxide, FeederAggression = Properties.Settings.Default.F2Agg });
            if (App.AppVM.NumFeeders < 3)
                return;
            App.AppVM.Feeders.Add(new FeederViewModel() { FeederNumber = Properties.Settings.Default.F3FeederNum, MaxDelta = Properties.Settings.Default.F3MaxDelta, Oxide = Properties.Settings.Default.F3Oxide, FeederAggression = Properties.Settings.Default.F3Agg });

        }
        public static void SaveConfig()
        {
            Properties.Settings.Default.ControlPeriod = App.AppVM.ControlPeriod;
            Properties.Settings.Default.Kp = App.AppVM.Kp;
            Properties.Settings.Default.Ki = App.AppVM.Ki;
            Properties.Settings.Default.Kd = App.AppVM.Kd;
            Properties.Settings.Default.SwitchTag = JsonConvert.SerializeObject(App.AppVM.SwitchTag);
            Properties.Settings.Default.StartTimeTag = JsonConvert.SerializeObject(App.AppVM.StartTimeTag);
            Properties.Settings.Default.StopTimeTag = JsonConvert.SerializeObject(App.AppVM.StopTimeTag);
            Properties.Settings.Default.CalculateTag = JsonConvert.SerializeObject(App.AppVM.CalculateTag);
            Properties.Settings.Default.FeederDictionary = JsonConvert.SerializeObject(profile);
            App.AppVM.NumFeeders = Properties.Settings.Default.NumFeeders = App.AppVM.Feeders.Count;
            if (App.AppVM.NumFeeders < 1)
            {
                Properties.Settings.Default.Save();
                return;
            }
            Properties.Settings.Default.F1FeederNum = App.AppVM.Feeders[0].FeederNumber;
            Properties.Settings.Default.F1MaxDelta = App.AppVM.Feeders[0].MaxDelta;
            Properties.Settings.Default.F1Oxide = App.AppVM.Feeders[0].Oxide;
            //Properties.Settings.Default.F1Agg = App.AppVM.Feeders[0].FeederAggression;
            if (App.AppVM.NumFeeders < 2)
            {
                Properties.Settings.Default.Save();
                return;
            }
            Properties.Settings.Default.F2FeederNum = App.AppVM.Feeders[1].FeederNumber;
            Properties.Settings.Default.F2MaxDelta = App.AppVM.Feeders[1].MaxDelta;
            Properties.Settings.Default.F2Oxide = App.AppVM.Feeders[1].Oxide;
            //Properties.Settings.Default.F2Agg = App.AppVM.Feeders[1].FeederAggression;
            if (App.AppVM.NumFeeders < 3)
            {
                Properties.Settings.Default.Save();
                return;
            }
            Properties.Settings.Default.F3FeederNum = App.AppVM.Feeders[2].FeederNumber;
            Properties.Settings.Default.F3MaxDelta = App.AppVM.Feeders[2].MaxDelta;
            Properties.Settings.Default.F3Oxide = App.AppVM.Feeders[2].Oxide;
            //Properties.Settings.Default.F3Agg = App.AppVM.Feeders[2].FeederAggression;

            Properties.Settings.Default.Save();
            return;
            
        }

        public static void CloseAppAsync()
        {
            SaveConfig();
            feeders.UpdateEvent -= Feeders_UpdateEvent;
            if (SwitchTag != null)
            {
                SwitchTag.UpdateValueEvent -= SwitchTag_UpdateValueEvent;
                SwitchTag.Dispose();                
            }
            if (StartTimeTag != null)
            {
                StartTimeTag.UpdateValueEvent -= StartTimeTag_UpdateValueEvent;
                StartTimeTag.Dispose();
            }
            if (StopTimeTag != null)
            {
                StopTimeTag.UpdateValueEvent -= StopTimeTag_UpdateValueEvent;
                StopTimeTag.Dispose();
            }
            if (IntervalTag != null)
            {
                IntervalTag.UpdateValueEvent -= IntervalTag_UpdateValueEvent;
                IntervalTag.Dispose();
            }
            if (BusyTag != null)
            {
                BusyTag.UpdateValueEvent -= BusyTag_UpdateValueEvent;
                BusyTag.Dispose();
            }
            Datapool.DatapoolSvr.Dispose();
        }
        public static void CreateInterval(DateTime startTime, DateTime stopTime, string dpGroup = "AutoInterval")
        {
            var a = OmniView.GetAnalysisListAverage(startTime, stopTime);
            a.WriteAnalysisToDatapool(dpGroup);
        }

        private static void StopService(string serviceName)
        {
            ServiceController sc = new ServiceController() { ServiceName = serviceName };

            if (sc.Status == ServiceControllerStatus.Running)
            {
                try
                {
                    TimeSpan timeout = TimeSpan.FromMilliseconds(20000);
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

                }
                catch(InvalidOperationException)
                {
                    MessageBox.Show("Service Timeout Reached.  Manually Restart OmniView Service.");
                }
            }
        }

        private static void StartService(string serviceName)
        {
            ServiceController sc = new ServiceController() { ServiceName = serviceName };
            if (sc.Status == ServiceControllerStatus.Stopped)
            {
                try
                {
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                }
                catch (InvalidOperationException)
                {
                    MessageBox.Show("Could not start OmniView Service.  Try starting manually.");
                }
            }
        }

        private static bool ServiceIsRunning(string serviceName)
        {
            ServiceController sc = new ServiceController() { ServiceName = serviceName };
            if (sc.Status == ServiceControllerStatus.Running)            
                return true;            
            else
                return false;
        }

        private static bool ServiceExists(string serviceName)
        {
            return ServiceController.GetServices().Any(s => s.ServiceName.Equals(serviceName));
        }

        public static void RebootService(string serviceName)
        {
            if (ServiceExists(serviceName))
            {
                if (ServiceIsRunning(serviceName))
                {
                    StopService(serviceName);
                    StartService(serviceName);
                }             
                else
                    StartService(serviceName);
            }
        }
    }
}
