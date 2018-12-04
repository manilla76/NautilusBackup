using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using ManualRamosAddon.Model;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using Thermo.Datapool;

namespace ManualRamosAddon
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class ApplicationViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        private const string NewFeederPropertyName = "NewFeeder";
        private const string FeedersPropertyName = "Feeders";
        public ObservableCollection<FeederViewModel> feeders;
        private FeederViewModel newFeeder;
        private int numFeeders;
        private int controlPeriod;
        private int solveCount;
        internal float MaxError = 10;
        private bool saveProfile = false;

        public int ControlPeriod { get => (controlPeriod < 1) ? 1 : controlPeriod; set => Set(ref controlPeriod, value); }
        public ObservableCollection<FeederViewModel> Feeders { get { return feeders; } set { feeders = value; RaisePropertyChanged("Feeders"); } }
        public FeederViewModel NewFeeder { get => newFeeder; set => Set(ref newFeeder, value); }
        public int NumFeeders { get => numFeeders; set => Set(ref numFeeders, value); }
        public int SolveCount { get => solveCount; set => Set(ref solveCount, value); }
        public float Kp { get; set; }
        public float Ki { get; set; }
        public float Kd { get; set; }
        public string RecipeSwitchGroup { get; set; }
        public string RecipeSwitchTag { get; set; }
        public string ErrorCode { get; set; }
        public bool SaveProfile { get => saveProfile; set => Set(ref saveProfile, value); }

        private void SelectedTagUpdate(NotificationMessage<Datapool.DPGroupTagName> msg)
        {
            if (msg.Notification == "NewTag")
                SelectedTag = msg.Content;
            else
            {
                switch (msg.Notification)
                {
                    case "Start":
                        StartTimeTag = msg.Content;
                        break;
                    case "Stop":
                        StopTimeTag = msg.Content;
                        break;
                    case "Calculate":
                        CalculateTag = msg.Content;
                        break;
                    case "Switch":
                        SwitchTag = msg.Content;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// The <see cref="SelectedTag" /> property's name.
        /// </summary>
        public const string SelectedTagPropertyName = "SelectedTag";

        private Datapool.DPGroupTagName selectedTag;

        /// <summary>
        /// Sets and gets the "SelectedTagproperty.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Datapool.DPGroupTagName SelectedTag
        {
            get
            {
                return selectedTag;
            }

            set
            {
                if (selectedTag == value)
                {
                    return;
                }

                selectedTag = value;
                RaisePropertyChanged(() => SelectedTag);
            }
        }
        public const string SwitchTagPropertyName = "SwitchTag";

        private Datapool.DPGroupTagName switchTag;

        /// <summary>
        /// Sets and gets the "switchTagProperty.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Datapool.DPGroupTagName SwitchTag
        {
            get
            {
                return switchTag;
            }

            set
            {
                if (switchTag == value)
                {
                    return;
                }

                switchTag = value;
                Model.ThermoInterface.UpdateTag(value, "Switch");
                RaisePropertyChanged(() => SwitchTag);
            }
        }
        /// <summary>
        /// The <see cref="StartTimeTag" /> property's name.
        /// </summary>
        public const string StartTimeTagPropertyName = "StartTimeTag";

        private Datapool.DPGroupTagName startTimeTag;

        /// <summary>
        /// Sets and gets the "StartTimeTagproperty.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Datapool.DPGroupTagName StartTimeTag
        {
            get
            {
                return startTimeTag;
            }

            set
            {
                if (startTimeTag == value)
                {
                    return;
                }

                startTimeTag = value;
                Model.ThermoInterface.UpdateTag(value, "StartTime");
                RaisePropertyChanged(() => StartTimeTag);
            }
        }
        /// <summary>
        /// The <see cref="StopTimeTag" /> property's name.
        /// </summary>
        public const string StopTimeTagPropertyName = "StopTimeTag";

        private Datapool.DPGroupTagName stopTimeTag;

        /// <summary>
        /// Sets and gets the "StopTimeTagproperty.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Datapool.DPGroupTagName StopTimeTag
        {
            get
            {
                return stopTimeTag;
            }

            set
            {
                if (stopTimeTag == value)
                {
                    return;
                }

                stopTimeTag = value;
                Model.ThermoInterface.UpdateTag(value, "StopTime");
                RaisePropertyChanged(() => StopTimeTag);
            }
        }
        /// <summary>
        /// The <see cref="CalculateTag" /> property's name.
        /// </summary>
        public const string CalculateTagPropertyName = "CalculateTag";

        private Datapool.DPGroupTagName calculateTag;

        /// <summary>
        /// Sets and gets the CalculateTag property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Datapool.DPGroupTagName CalculateTag
        {
            get
            {
                return calculateTag;
            }

            set
            {
                if (calculateTag == value)
                {
                    return;
                }

                calculateTag = value;
                Model.ThermoInterface.UpdateTag(value, "Calculate");
                RaisePropertyChanged(() => CalculateTag);
            }
        }
        private RelayCommand<MouseEventArgs> mouseDoubleClickCommand;

        /// <summary>
        /// Gets the MouseDoubleClickCommand.
        /// </summary>
        public RelayCommand<MouseEventArgs> MouseDoubleClickCommand
        {
            get
            {
                return mouseDoubleClickCommand
                    ?? (mouseDoubleClickCommand = new RelayCommand<MouseEventArgs>(
                    (p) =>
                    {
                        string name = ((TextBox)p.Source).Name;
                        Messenger.Default.Send(new NotificationMessage(name + " AddDp"));
                        AddDpDialogViewModel dpDialog = new AddDpDialogViewModel();
                    }));
            }
        }

        /// <summary>
        /// Initializes a new instance of the ApplicationViewModel class.
        /// </summary>
        public ApplicationViewModel(IDataService dataService)
        {
            App.AppVM = this;
            feeders = new ObservableCollection<FeederViewModel>();
            _dataService = dataService;
            SolveCount = 0;
            //ThermoArgonautViewerLibrary.CommonSystemViewer.System = new ThermoArgonautViewerLibrary.CommonSystemViewer(new System.Windows.Controls.ContentControl());
            //ThermoArgonautViewerLibrary.CommonSystemViewer.System.ConnectToComputer();
            InitFeeders();
            Messenger.Default.Register<NotificationMessage<Datapool.DPGroupTagName>>(this, SelectedTagUpdate);
        }

        private void InitFeeders()
        {
            // read in Feeder data from save
            // create and load feederViewModel for each feeder to be controlled.
            // Feeders.Add(new FeederViewModel { FeederNumber = 1, MaxDelta = 0.1f, Oxide = "Fe2O3" });
            Model.ThermoInterface.LoadConfig();
        }
    }
}