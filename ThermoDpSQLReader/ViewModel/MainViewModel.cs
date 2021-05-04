using System.ComponentModel;
using System.Windows.Input;
using Thermo.Datapool;
using ThermoDpSQLReader.Model;

namespace ThermoDpSQLReader.ViewModel
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
        /// The <see cref="SqlServer" /> property's name.
        /// </summary>
        public const string SqlServerPropertyName = "MyProperty";

        private string sqlServer = Properties.Settings.Default.SqlServerAddress;

        /// <summary>
        /// Sets and gets the MyProperty property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SqlServer
        {
            get => sqlServer;

            set
            {
                if (sqlServer == value)
                {
                    return;
                }

                sqlServer = value;
                RaisePropertyChanged(SqlServerPropertyName);
            }
        }
        private readonly IDataService dataService;

        /// <summary>
        /// The <see cref="ReadGroup" /> property's name.
        /// </summary>
        public const string ReadGroupPropertyName = "ReadGroup";

        private string readGroup = Properties.Settings.Default.ReadGroup;

        /// <summary>
        /// Sets and gets the ReadGroup property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ReadGroup
        {
            get => readGroup;

            set
            {
                if (readGroup == value)
                {
                    return;
                }

                readGroup = value;
                RaisePropertyChanged(ReadGroupPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ReadTag" /> property's name.
        /// </summary>
        public const string ReadTagPropertyName = "ReadGroup";

        private string readTag = Properties.Settings.Default.ReadTag;

        /// <summary>
        /// Sets and gets the ReadTag property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ReadTag
        {
            get => readTag;

            set
            {
                if (readTag == value)
                {
                    return;
                }

                readTag = value;
                RaisePropertyChanged(ReadTagPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="WriteGroup" /> property's name.
        /// </summary>
        public const string WriteGroupPropertyName = "WriteGroup";

        private string writeGroup = Properties.Settings.Default.WriteGroup;

        /// <summary>
        /// Sets and gets the WriteGroup property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WriteGroup
        {
            get => writeGroup;

            set
            {
                if (writeGroup == value)
                {
                    return;
                }

                writeGroup = value;
                RaisePropertyChanged(WriteGroupPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="WriteTag" /> property's name.
        /// </summary>
        public const string WriteTagPropertyName = "WriteTag";

        private string writeTag = Properties.Settings.Default.WriteTag;

        /// <summary>
        /// Sets and gets the WriteTag property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WriteTag
        {
            get => writeTag;

            set
            {
                if (writeTag == value)
                {
                    return;
                }

                writeTag = value;
                RaisePropertyChanged(WriteTagPropertyName);
            }
        }

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

        private RelayCommand saveCommand;

        /// <summary>
        /// Gets the SaveCommand.
        /// </summary>
        public RelayCommand SaveCommand
        {
            get
            {
                return saveCommand
                    ?? (saveCommand = new RelayCommand(
                    () =>
                    {
                        Properties.Settings.Default.SqlServerAddress = SqlServer;
                        Properties.Settings.Default.ReadGroup = ReadGroup;
                        Properties.Settings.Default.ReadTag = ReadTag;
                        Properties.Settings.Default.WriteGroup = WriteGroup;
                        Properties.Settings.Default.WriteTag = WriteTag;
                        Properties.Settings.Default.Save();
                        DataManager.TagInfoUpdate();
                    }));
            }
        }



        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            this.dataService = dataService;
            WelcomeTitle = @"Sql Dp Interface";
            Datapool.DatapoolSvr.IpAddress = @"127.0.0.1";
            Datapool.DatapoolSvr.MonitorPeriodMilliSeconds = 5000;
            Datapool.DatapoolSvr.ConnectionChange += DatapoolSvr_ConnectionChange;
        }

        public ICommand WindowClosing => new RelayCommand<CancelEventArgs>((args) =>
        {
            Cleanup();
        });

        private void DatapoolSvr_ConnectionChange(bool isConnected)
        {
            DpConnected = isConnected;
        }

        public override void Cleanup()
        {
            // Clean up if needed
            base.Cleanup();
        }
    }
}