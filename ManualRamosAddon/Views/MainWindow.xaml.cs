using System.Windows;
using ManualRamosAddon;
using GalaSoft.MvvmLight.Messaging;
using System;

namespace ManualRamosAddon
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
            Closing += (s, e) => ViewModel.ViewModelLocator.Cleanup();
            Messenger.Default.Register<NotificationMessage>(this, ShowDialogReceived);
        }

        private void ShowDialogReceived(NotificationMessage msg)
        {
            
            if (msg.Notification == "AddFeeder")
            {
                var dialog = new AddFeeder();
                dialog.ShowDialog();
            }
                
            if (msg.Notification == "AddInterval")
            {
                var dialog = new AutoInterval();
                dialog.ShowDialog();
            }
                
            if (msg.Notification.Contains("AddDp"))
            {
                var dialog = new AddDpDialog();
                Messenger.Default.Send(new NotificationMessage(msg.Notification.Substring(0, msg.Notification.IndexOf(' '))));
                dialog.ShowDialog();
            }
                
        }
    }
}