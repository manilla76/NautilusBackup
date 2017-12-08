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
        private float maxDelta;
        private float currentDemand;
        private bool isManual;
        private const string SourcesPropertyName = "Sources";
        private ObservableCollection<SourceData> sources = new ObservableCollection<SourceData>();

        public string Oxide { get => oxide; set => Set(ref oxide, value); }
        public int FeederNumber { get => feederNumber; set => Set(ref feederNumber, value); }
        public float MaxDelta { get => maxDelta; set => Set(ref maxDelta, value); }
        public float CurrentDemand { get => currentDemand; set => Set(ref currentDemand, value); }
        public bool IsManual { get => isManual; set => Set(ref isManual, value); }
        public ObservableCollection<SourceData> Sources { get { return sources; } }


        /// <summary>
        /// Initializes a new instance of the FeederViewModel class.
        /// </summary>
        public FeederViewModel()
        {
            Model.ThermoInterface.InitSources(Sources);
        }

        private RelayCommand openCommand;

        /// <summary>
        /// Gets the MyCommand.
        /// </summary>
        public RelayCommand OpenCommand => openCommand ?? (openCommand = new RelayCommand(() =>
        {
            // Open command code  Open a new windows to input feeder config.
            App.AppVM.NewFeeder = this;
            Messenger.Default.Send(new NotificationMessage("Show Dialog"));
            Model.ThermoInterface.SaveConfig();
  
        }));
    }
}