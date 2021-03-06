﻿using BlendingOptimizationEngine;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using static ThermoArgonautLibrary.CommonSystem;
using static ThermoArgonautViewerLibrary.CommonSystemViewer;

namespace ManualRamosAddon.Model
{
    public static class ThermoInterface
    {
        static Timer T;
        public static ThermoArgonautViewerLibrary.Argonaut OmniView = new ThermoArgonautViewerLibrary.Argonaut();
        public static BOSCtrl Ramos;
        public static ServerItemUpdate ServerItemUpdate;
        public static Datapool.ITagInfo SwitchTag;

        public static void Init()
        {
            T = new Timer();
            T.Interval = 5000; // 5 sec   5*1000ms
            T.Enabled = true;
            T.Elapsed += new ElapsedEventHandler(onElapsedEvent);
            Ramos = new BOSCtrl(null);
            Ramos.SolveReport.UpdateEvent += SolveReport_UpdateEvent;
            ServerItemUpdate = new ServerItemUpdate();
            ServerItemUpdate.IsUpdated(ServerItemUpdate.ItemsEnum.UpdateProduct, OmniView.GetItemUpdate());
        }

        internal static void InitOxides(ObservableCollection<string> oxides)
        {
            var tags = Datapool.DatapoolSvr.ReadTagNames();
            if (tags == null) return;
            var group = tags.m_tagNames.Where((f) => f.m_group == "RAMOS.Predicted").ToList();

            var temp = group.Select((f) => f.m_tag).ToList();
            if (temp == null)
                return;
            oxides.Clear();
            foreach (var a in temp)
            {
                oxides.Add(a);
            }
        }

        private static void SolveReport_UpdateEvent(BlendControl.SolveReport e)
        {
            App.AppVM.SolveCount++;
            if (App.AppVM.SolveCount >= App.AppVM.ControlPeriod)
            {
                App.AppVM.SolveCount = 0;
                ProcessSolve();
            }
        }

        private static void ProcessSolve()
        {
            //TODO: Evaluate each feeder and modify the feeder demand as appropriate.
            foreach (FeederViewModel feeder in App.AppVM.Feeders)
            {
                if (feeder.IsManual != true)  // if disabled, skip
                    continue;
                // get target & tolerance for the oxide
                var recipe = Ramos.Recipe.Items.Where((f) => f.QcName == feeder.Oxide).FirstOrDefault();
                if (recipe == null) return;
                double setpoint = recipe.Setpoint;
                double tolerance = recipe.Tolerance;
                bool basis = recipe.FirstBasis;  // true = lf    false = db
                // get rolling average for the oxide

                double rolling;
                Datapool.DatapoolSvr.Read("Rolling.Analysis1." + (basis ? "Loss free" : "Dry basis"), feeder.Oxide, out rolling);

                // get current demand for this feeder
                double demand = Ramos.SolveReport.Feeder[feeder.FeederNumber].Demand * 100;
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
                Datapool.DatapoolSvr.Write("RAMOS.Feeder.Read", "Set fixed rate% #" + (feeder.FeederNumber + 1).ToString(), demand);
            }
        }

        private static void onElapsedEvent(object sender, ElapsedEventArgs e)  // Runs every T (timer) seconds.  See Init() for T value.
        {
            var results = OmniView.GetRaMOSFeederSetup();
            if (results == null) return;
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

            ServerItemUpdate currentUpdate = OmniView.GetItemUpdate();
            if (OmniView.CommunicationUp() == Thermo.Communication.Communication.ClientCom.triState.True)
            {
                if (ServerItemUpdate.IsUpdated(ServerItemUpdate.ItemsEnum.RaMOSSolveReport, currentUpdate))
                    Ramos.SolveReport.Assign(OmniView.GetRaMOSSolveReport());
                if (ServerItemUpdate.IsUpdated(ServerItemUpdate.ItemsEnum.RaMOSRecipe, currentUpdate))
                    Ramos.Recipe.Assign(OmniView.GetRaMOSRecipe());
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

                App.Current.MainWindow.Close();
            }

        }

        public static void LoadConfig()
        {
            App.AppVM.NumFeeders = Properties.Settings.Default.NumFeeders;
            App.AppVM.ControlPeriod = Properties.Settings.Default.ControlPeriod;
            App.AppVM.Kp = Properties.Settings.Default.Kp;
            App.AppVM.Ki = Properties.Settings.Default.Ki;
            App.AppVM.Kd = Properties.Settings.Default.Kd;
            App.AppVM.RecipeSwitchGroup = Properties.Settings.Default.RecipeGroup;
            App.AppVM.RecipeSwitchTag = Properties.Settings.Default.RecipeTag;
            UpdateSwitchTag();

            if (App.AppVM.NumFeeders < 1)
            {
                //try
                //{
                //    App.AppVM.Feeders.Add(new FeederViewModel() { FeederNumber = 1, MaxDelta = 0.1f, Oxide = "Fe2O3" });
                //}
                //catch { MessageBox.Show("newError"); }
                return;
            }
            App.AppVM.Feeders.Add(new FeederViewModel() { FeederNumber = Properties.Settings.Default.F1FeederNum, MaxDelta = Properties.Settings.Default.F1MaxDelta, Oxide = Properties.Settings.Default.F1Oxide, FeederAggression = Properties.Settings.Default.F1Agg });
            if (App.AppVM.NumFeeders < 2)
                return;
            App.AppVM.Feeders.Add(new FeederViewModel() { FeederNumber = Properties.Settings.Default.F2FeederNum, MaxDelta = Properties.Settings.Default.F2MaxDelta, Oxide = Properties.Settings.Default.F2Oxide, FeederAggression = Properties.Settings.Default.F2Agg });
            if (App.AppVM.NumFeeders < 3)
                return;
            App.AppVM.Feeders.Add(new FeederViewModel() { FeederNumber = Properties.Settings.Default.F3FeederNum, MaxDelta = Properties.Settings.Default.F3MaxDelta, Oxide = Properties.Settings.Default.F3Oxide, FeederAggression = Properties.Settings.Default.F3Agg });

        }

        private static void UpdateSwitchTag()
        {
            if (App.AppVM.RecipeSwitchGroup == string.Empty || App.AppVM.RecipeSwitchTag == string.Empty)
            {
                return;
            }
            if (SwitchTag != null)
            {
                SwitchTag.UpdateValueEvent -= SwitchTag_UpdateValueEvent;
                SwitchTag.Dispose();
            }
            SwitchTag = Datapool.DatapoolSvr.CreateTagInfo(App.AppVM.RecipeSwitchGroup, App.AppVM.RecipeSwitchTag, Datapool.dpTypes.STRING);
            SwitchTag.UpdateValueEvent += SwitchTag_UpdateValueEvent;
        }

        private static void SwitchTag_UpdateValueEvent(Datapool.ITagInfo e)
        {
            try
            {
                var blendControl = OmniView.GetRaMOSConfiguration();
                blendControl.RecipeName = e.AsString;
                OmniView.SetRaMOSConfiguration(blendControl);
                App.AppVM.ErrorCode = string.Empty;
            }
            catch (Exception)
            {
                App.AppVM.ErrorCode = "Recipe in switch tag not valid. No change to current recipe.";
            }
        }

        public static void SaveConfig()
        {
            Properties.Settings.Default.ControlPeriod = App.AppVM.ControlPeriod;
            Properties.Settings.Default.Kp = App.AppVM.Kp;
            Properties.Settings.Default.Ki = App.AppVM.Ki;
            Properties.Settings.Default.Kd = App.AppVM.Kd;
            Properties.Settings.Default.RecipeGroup = App.AppVM.RecipeSwitchGroup;
            Properties.Settings.Default.RecipeTag = App.AppVM.RecipeSwitchTag;

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
        public static void CloseApp()
        {
            if (SwitchTag != null)
            {
                SwitchTag.Dispose();
            }
        }
    }
}
