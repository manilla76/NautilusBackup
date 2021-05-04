using SandBox.Model;
using System.Windows;

namespace Sandbox.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// The <see cref="TerminateTag" /> property's name.
        /// </summary>
        public const string TerminateTagPropertyName = "TerminateTag";

        private Datapool.ITagInfo terminateTag = Datapool.DatapoolSvr.CreateTagInfo("SYSTEM", "Test", Datapool.dpTypes.BOOL);

        private void TerminateTag_UpdateValueEvent(Datapool.ITagInfo e)
        {
            if (e.AsBoolean == true)
            {
                Cleanup();
                Messenger.Default.Send<string>("shutdown");
            }
        }
        /// <summary>
        /// The <see cref="DpConnected" /> property's name.
        /// </summary>
        public const string DpConnectedPropertyName = "DpConnected";

        private bool dpConnected = Datapool.DatapoolSvr.IsConnected;

        /// <summary>
        /// Sets and gets the DpConnected property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool DpConnected
        {
            get
            {
                return dpConnected;
            }

            set
            {
                if (dpConnected == value)
                {
                    return;
                }

                dpConnected = value;
                RaisePropertyChanged(DpConnectedPropertyName);
            }
        }
        /// <summary>
        /// The <see cref="TestS" /> property's name.
        /// </summary>
        public const string TestSPropertyName = "TestS";

        private string testS = SandBox.Properties.Settings.Default.TestS;

        /// <summary>
        /// Sets and gets the TestS property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string TestS
        {
            get
            {
                return testS;
            }

            set
            {
                if (testS == value)
                {
                    return;
                }

                testS = value;
                RaisePropertyChanged(() => TestS);
            }
        }

        private readonly IDataService _dataService;

        /// <summary>
        /// The <see cref="WelcomeTitle" /> property's name.
        /// </summary>
        public const string WelcomeTitlePropertyName = "WelcomeTitle";

        private string _welcomeTitle = string.Empty;

        /// <summary>
        /// Gets the WelcomeTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WelcomeTitle
        {
            get
            {
                return _welcomeTitle;
            }
            set
            {
                Set(ref _welcomeTitle, value);
            }
        }

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

                    WelcomeTitle = item.Title;
                });
            terminateTag.UpdateValueEvent += TerminateTag_UpdateValueEvent;
            Datapool.DatapoolSvr.ConnectionChange += DatapoolSvr_ConnectionChange;
            SandBox.ViewModel.NotifyIconViewModel.OnExiting += NotifyIconViewModel_OnExiting;
        }

        private void NotifyIconViewModel_OnExiting(object sender, System.EventArgs e)
        {
            Cleanup();
            Messenger.Default.Send<string>("shutdown");
        }

        private void DatapoolSvr_ConnectionChange(bool isConnected)
        {
            DpConnected = isConnected;
        }

        private RelayCommand testCommand;

        /// <summary>
        /// Gets the MyCommand.
        /// </summary>
        public RelayCommand TestCommand
        {
            get
            {
                return testCommand
                    ?? (testCommand = new RelayCommand(
                    () =>
                    {
                        TestS = SandBox.Properties.Settings.Default.TestS;
                        MessageBox.Show("Button Worked: " + TestS);
                    }));
            }
        }

        private RelayCommand writeCommand;

        /// <summary>
        /// Gets the MyCommand.
        /// </summary>
        public RelayCommand WriteCommand
        {
            get
            {
                return writeCommand
                    ?? (writeCommand = new RelayCommand(
                    () =>
                    {
                        SandBox.Properties.Settings.Default.TestF = 5.432f;
                        SandBox.Properties.Settings.Default.Save();

                    }));
            }
        }

        private RelayCommand readCommand;

        /// <summary>
        /// Gets the MyCommand.
        /// </summary>
        public RelayCommand ReadCommand
        {
            get
            {
                return readCommand
                    ?? (readCommand = new RelayCommand(
                    () =>
                    {
                        MessageBox.Show("Float: " + SandBox.Properties.Settings.Default.TestF.ToString());

                    }));
            }
        }

        public override void Cleanup()
        {
            // Clean up if needed
            terminateTag.Dispose();
            Datapool.DatapoolSvr.Dispose();
            base.Cleanup();
        }
    }
}