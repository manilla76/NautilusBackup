using GalaSoft.MvvmLight.Messaging;
using System.Windows;

namespace ManualRamosAddon
{
    /// <summary>
    /// Interaction logic for AutoInterval.xaml
    /// </summary>
    public partial class AutoInterval : Window
    {
        public AutoInterval()
        {
            InitializeComponent();
        }

        private void ThermoDPTreeWidget_TagSelectionChangeEvent(Thermo.Datapool.Datapool.DPGroupTagName tag)
        {
            Messenger.Default.Send(new NotificationMessage<Thermo.Datapool.Datapool.DPGroupTagName>(this, tag, "NewTag"));
        }
    }
}
