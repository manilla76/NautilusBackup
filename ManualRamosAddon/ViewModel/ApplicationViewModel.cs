using GalaSoft.MvvmLight;
using ManualRamosAddon.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System;

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

        public ObservableCollection<FeederViewModel> Feeders { get { return feeders; } set { feeders = value; RaisePropertyChanged("Feeders"); } }
        public FeederViewModel NewFeeder { get => newFeeder; set => Set(ref newFeeder, value); }
        public int NumFeeders { get => numFeeders; set => Set(ref numFeeders, value); }

        /// <summary>
        /// Initializes a new instance of the ApplicationViewModel class.
        /// </summary>
        public ApplicationViewModel(IDataService dataService)
        {
            App.AppVM = this;
            feeders = new ObservableCollection<FeederViewModel>();
            _dataService = dataService;
            InitFeeders();
        }

        private void InitFeeders()
        {
            // read in Feeder data from save
            // create and load feederViewModel for each feeder to be controlled.
            // Feeders.Add(new FeederViewModel { FeederNumber = 1, MaxDelta = 0.1f, Oxide = "Fe2O3" });
            Model.ThermoInterface.LoadConfig(this);
        }
    }
}