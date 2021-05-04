using ManualRamosAddon.Model;
using System.Collections.ObjectModel;

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

        /// <summary>
        /// Initializes a new instance of the ApplicationViewModel class.
        /// </summary>
        public ApplicationViewModel(IDataService dataService)
        {
            App.AppVM = this;
            feeders = new ObservableCollection<FeederViewModel>();
            _dataService = dataService;
            InitFeeders();
            SolveCount = 0;
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