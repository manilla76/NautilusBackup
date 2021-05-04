using System.Windows;
using ThermoDpSQLReader.ViewModel;

namespace ThermoDpSQLReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();
            Messenger.Default.Register<string>(this, CloseApp);
        }

        private void CloseApp(string msg)
        {
            this.Dispatcher.Invoke(() =>
            {
                App.Current.Shutdown();
            });
        }
    }
}