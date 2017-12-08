using GalaSoft.MvvmLight;
using ManualRamosAddon.Model;
using System.Collections.ObjectModel;
using System;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;

namespace ManualRamosAddon
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {        
        private readonly IDataService _dataService;
        public ApplicationViewModel AppVM = SimpleIoc.Default.GetInstance<ApplicationViewModel>();
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;
            _dataService.GetData(
                (item, error) =>
                {
                    if (error != null)
                    {
                        // Report error here
                        return;
                    }
                });
        }

        private RelayCommand openCommand;

        /// <summary>
        /// Gets the OpenCommand.
        /// </summary>
        public RelayCommand OpenCommand =>  openCommand ?? (openCommand = new RelayCommand(() =>
                    {
                        // Open command code  Open a new windows to input feeder config.
                        FeederViewModel newFeederVM = new FeederViewModel { FeederNumber = 1, IsManual = false, Oxide = "Fe2O3", MaxDelta = 0.1f };
                        AppVM.Feeders.Add(newFeederVM);
                        AppVM.NewFeeder = newFeederVM;
                        Messenger.Default.Send(new NotificationMessage("Show Dialog"));
                    }));                  

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}