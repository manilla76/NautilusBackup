using System;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;

namespace ManualRamosAddon
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class FeederViewModel : ViewModelBase
    {
        private string oxide;
        private int feederNumber;
        private int feederAggression;
        private float maxDelta;
        private double currentDemand;
        private bool isManual;
        private const string SourcesPropertyName = "Sources";
        private ObservableCollection<SourceData> sources = new ObservableCollection<SourceData>();
        private ObservableCollection<string> oxides = new ObservableCollection<string>() { "Al2O3", "CaO", "Fe2O3", "MgO", "SiO2", "LSF", "C3S", "Alkali" };
        private bool debug;
        private double setpoint;
        private double tolerance;
        private double rolling;
        private bool isLf;
        private double error;
        private double prevError;
        private double errorSum;

        public string Oxide { get => oxide; set => Set(ref oxide, value); }
        public string SourceEstmiateName { get => Sources[FeederNumber].SourceEstimateName; }
        public int FeederNumber { get => feederNumber; set => Set(ref feederNumber, value); }
        public int FeederAggression { get => feederAggression; set => Set(ref feederAggression, value); }
        public float MaxDelta { get => maxDelta; set => Set(ref maxDelta, value); }
        public double CurrentDemand { get => currentDemand; set => Set(ref currentDemand, value); }
        public bool IsManual { get => isManual; set => Set(ref isManual, value); }
        public ObservableCollection<SourceData> Sources { get => sources; } 
        public ObservableCollection<string> Oxides { get => oxides; }
        public double Error { get => error; set => Set(ref error, value); }
        public double PrevError { get => prevError; set => Set(ref prevError, value); }
        public double ErrorSum { get => errorSum; set => Set(ref errorSum, value); }
        public bool Debug { get => debug; set => Set(ref debug, value); }
        public bool IsLf { get => isLf; set => Set(ref isLf, value); }
        public double Tolerance { get => tolerance; set => Set(ref tolerance, value); }
        public double Setpoint { get => setpoint; set => Set(ref setpoint, value); }
        public double Rolling { get => rolling; set => Set(ref rolling, value); }

        /// <summary>
        /// Initializes a new instance of the FeederViewModel class.
        /// </summary>
        public FeederViewModel()
        {
            Model.ThermoInterface.InitSources(Sources);
            Model.ThermoInterface.InitOxides(Oxides);
        }

        private RelayCommand openCommand;

        /// <summary>
        /// Gets the MyCommand.
        /// </summary>
        public RelayCommand OpenCommand => openCommand ?? (openCommand = new RelayCommand(() =>
        {
            // Open command code  Open a new windows to input feeder config.
            App.AppVM.NewFeeder = this;
            Messenger.Default.Send(new NotificationMessage("AddFeeder"));
            Model.ThermoInterface.SaveConfig();
  
        }));

        private RelayCommand deleteCommand;

        /// <summary>
        /// Gets the MyCommand.
        /// </summary>
        public RelayCommand DeleteCommand => deleteCommand ?? (deleteCommand = new RelayCommand(() =>
        {
            // Open command code  Open a new windows to input feeder config.
            App.AppVM.Feeders.Remove(this);
            Model.ThermoInterface.SaveConfig();

        }));
    }
}