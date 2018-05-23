using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ManualRamosAddon
{
    /// <summary>
    /// Interaction logic for AddDpDialog.xaml
    /// </summary>
    public partial class AddDpDialog : Window
    {
        private string destination = string.Empty;

        public AddDpDialog()
        {
            InitializeComponent();
            Messenger.Default.Register<NotificationMessage>(this, WhichTag);
        }

        private void WhichTag(NotificationMessage msg)
        {
            destination = (msg.Notification);
        }

        private void ThermoDPTreeWidget_MouseDoubleClickSelectionEvent(Thermo.Datapool.Datapool.DPGroupTagName tag)
        {
            Messenger.Default.Send(new NotificationMessage<Thermo.Datapool.Datapool.DPGroupTagName>(this, tag, destination));
            this.Close();
        }
    }
}
