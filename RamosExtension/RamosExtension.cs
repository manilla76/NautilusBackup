using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Thermo.Datapool;
using ThermoArgonautViewerLibrary;
using Topshelf.Logging;
using static Thermo.Communication.Communication.ClientCom;
using static Thermo.Datapool.Datapool;
using static ThermoArgonautLibrary.CommonSystem;
using static ThermoArgonautViewerLibrary.CommonSystemViewer;

namespace RamosExtension
{
    public class RamosExtension
    {
        private readonly LogWriter logger;
        private CommonSystemViewer omni = new CommonSystemViewer(null);
        private Argonaut omniView = new Argonaut();
        private List<DPGroupTagValue> tags = new List<DPGroupTagValue>();
        //private BlendConfig ramos;
        private BOSCtrl ramos = new BOSCtrl(null);
        private string recipeName = string.Empty;
        private bool updateDatapool;
        private ServerItemUpdate recipeUpdate = null;
        private Timer timer;
        private string datapoolGroupName = "Ramos.Recipe.Setpoint.Custom";
        private bool datapoolIsConnected = false;
        private bool isUpdating = false;
        private static Datapool datapool;
        private readonly int timerDelayTime = 1000;  // wait 1 seconds before timer starts.
        private readonly int timerPeriod = 500;     // repeat timer every 0.5 seconds.  Call TimerElapsed(null) each period.

        public RamosExtension()
        {
            logger = new Log4NetLogWriterFactory().Get("RamosExtension");
        }

        private static bool CheckDatapoolConnectionStatus()
        {
            return !string.IsNullOrEmpty(datapool.Read("SYSTEM", "system terminate"));
        }

        private void DatapoolSvr_ConnectionChange(bool isConnected)
        {
            if (datapool.IsConnected == datapoolIsConnected) return;
            logger.Info($"Datapool Server connection changed.  Connected? = {datapool.IsConnected}");
            datapoolIsConnected = datapool.IsConnected;
            if (datapoolIsConnected)
            {
                if (tags.Count == 0)
                {
                    updateDatapool = true;    // Load Recipe targets into datapool
                    AddQCsToTagsList();
                }
            }
            else
            {
                ClearQcsAndUnsubscribe();     // Clear tags when datapool closes
            }
        }

        private void AddQCsToTagsList()
        {
            if (tags.Count > 0)
            {
                logger.Warn($"qCModels should be cleared before adding new QCs.  qCModels.Count = {tags.Count}");
                return;
            }
            if (datapool.IsConnected == false)
            {
                logger.Warn("Nautilus Datapool is not running.  Tags cannot be added.");
                return;
            }
            
            isUpdating = true;            
            ramos.Recipe.Items.ForEach(q =>
            {
                var tag = new DPGroupTagValue(datapoolGroupName, q.QcName, q.Setpoint);
                datapool.Create(datapoolGroupName, q.QcName, dpTypes.FLOAT);
                datapool.Write(datapoolGroupName, q.QcName, q.Setpoint);
                tags.Add(tag);
            });                         // Add all recipe targets to the datapool.

            
            //isUpdating = false;
            updateDatapool = false;    // datapool updated, do not do it again.   Is updating will be reset in timerElapsed.
        }

        private void TimerElapsed(object state)
        {
            //logger.Debug("Timer Elapsed.");
            DatapoolSvr_ConnectionChange(CheckDatapoolConnectionStatus());
            if (datapool.IsConnected == false) return;
            if (RecipeChanged() & isUpdating == false)     // check for recipe change if not already in the middle of an update.  Checks for new Recipe (not a target change).
            {
                logger.Debug("Recipe Changed, call clearing QC List and rebuilding.");
                if (tags.Count > 0) ClearQcsAndUnsubscribe();
                AddQCsToTagsList();
            }
            // Check for data changes
            else    // target change if updateDatapool == true and isUpdating == false
            {
                if (updateDatapool & isUpdating == false)                             // recipe target changed
                {
                    foreach (var tag in tags)
                    {
                        tag.m_double = ramos.Recipe.Items.FirstOrDefault(t => t.QcName == tag.m_tag).Setpoint;
                        datapool.Write(datapoolGroupName, tag.m_tag, tag.m_double);
                        logger.Info($"Writing recipe target to datapool.  tag: {tag.m_tag}, value: {tag.m_double}.");
                    }
                    updateDatapool = false;
                }
            }
            var oldValues = tags.Select(t=>t.m_double).ToList();        // set oldValues to the current values.
            datapool.Read(ref tags);                                    // datapool read current values.

            if (isUpdating == false & oldValues.Count > 0)
            {
                for (int i = 0; i < tags.Count; i++)
                {
                    if (oldValues[i] != tags[i].m_double)
                    {
                        logger.Info($"Datapool tag updated: {tags[i].m_tag}.  OldValue = {oldValues[i]}, NewValue = {tags[i].m_double} ");
                        isUpdating = true;
                        UpdateRecipe(tags[i].m_tag, tags[i].m_double);
                    }
                }
                return;
            }
            
            if (isUpdating)
            {
                isUpdating = false;
                return;
            }
        }

        private void UpdateRecipe(string qcName, double setpoint)
        {
            ramos.Recipe = omniView.GetRaMOSRecipe();
            ramos.Recipe.Items.FirstOrDefault(q => q.QcName == qcName).Setpoint = setpoint;
            omniView.SetRecipe(ramos.Recipe);
            logger.Info("Ramos Targets Updated Remotely.");
        }

        private bool RecipeChanged()   // return false if no change in recipe name.  
        {
            if (isUpdating) return false;

            if (recipeUpdate == null)
            {
                recipeUpdate = new ServerItemUpdate();
                recipeUpdate.IsUpdated(ServerItemUpdate.ItemsEnum.RaMOSRecipe, omniView.GetItemUpdate());
            }

            // Get which items in the server has updated...
            ServerItemUpdate currentItemUpdate = omniView.GetItemUpdate();

            if (omniView.CommunicationUp() == triState.True && recipeUpdate.IsUpdated(ServerItemUpdate.ItemsEnum.RaMOSRecipe, currentItemUpdate))
            {
                ramos.Recipe = omniView.GetRaMOSRecipe();
                if (string.IsNullOrEmpty(recipeName))
                {
                    recipeName = ramos.Recipe.RecipeName;
                }
                if (recipeName != ramos.Recipe.RecipeName)
                {
                    recipeName = ramos.Recipe.RecipeName;
                    updateDatapool = true;
                    logger.Debug($"Recipe updated, different recipe selected.");
                    return true;
                }
                updateDatapool = true;
            }
            return false;
        }

        public void Start()
        {
            logger.Info("Start");
            datapool = DatapoolSvr;

            logger.Debug("Constructor Started");
            timer = new Timer(TimerElapsed, null, timerDelayTime, timerPeriod);
            ramos.Recipe = omniView.GetRaMOSRecipe();
            recipeName = ramos.Recipe.RecipeName;
            //datapool.ConnectionChange += DatapoolSvr_ConnectionChange;

            DatapoolSvr_ConnectionChange(CheckDatapoolConnectionStatus());
        }

        public void Stop()
        {
            logger.Info("Stop");
            timer.Dispose();
            //datapool.ConnectionChange -= DatapoolSvr_ConnectionChange;
           
            ClearQcsAndUnsubscribe();
            logger.Info("Cleared.");
            datapool.Dispose();
            //logger.Debug("Closing.");
        }

        private void ClearQcsAndUnsubscribe()
        {
            if (tags.Count == 0) return;

            tags.Clear();
        }
    }
}
