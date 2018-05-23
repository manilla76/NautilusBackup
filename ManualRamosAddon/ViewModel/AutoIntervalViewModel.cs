using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Controls;
using System.Windows.Input;
using Thermo.Datapool;

namespace ManualRamosAddon
{
    public class AutoIntervalViewModel : ViewModelBase
    {
        public AutoIntervalViewModel()
        {
            Messenger.Default.Register<NotificationMessage<Datapool.DPGroupTagName>>(this, SelectedTagUpdate);
        }

        private void SelectedTagUpdate(NotificationMessage<Datapool.DPGroupTagName> msg)
        {
            if (msg.Notification == "NewTag")
                SelectedTag = msg.Content;
            else
            {
                switch (msg.Notification)
                {
                    case "Start":
                        StartTimeTag = msg.Content;
                        break;
                    case "Stop":
                        StopTimeTag = msg.Content;
                        break;
                    case "Calculate":
                        CalculateTag = msg.Content;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// The <see cref="SelectedTag" /> property's name.
        /// </summary>
        public const string SelectedTagPropertyName = "SelectedTag";

        private Datapool.DPGroupTagName selectedTag;

        /// <summary>
        /// Sets and gets the "SelectedTagproperty.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Datapool.DPGroupTagName SelectedTag
        {
            get
            {
                return selectedTag;
            }

            set
            {
                if (selectedTag == value)
                {
                    return;
                }

                selectedTag = value;
                RaisePropertyChanged(() => SelectedTag);
            }
        }
        /// <summary>
        /// The <see cref="SelectedTag" /> property's name.
        /// </summary>
        public const string StartTimeTagPropertyName = "StartTimeTag";

        private Datapool.DPGroupTagName startTimeTag;

        /// <summary>
        /// Sets and gets the "StartTimeTagproperty.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Datapool.DPGroupTagName StartTimeTag
        {
            get
            {
                return startTimeTag;
            }

            set
            {
                if (startTimeTag == value)
                {
                    return;
                }

                startTimeTag = value;
                Model.ThermoInterface.UpdateTag(value, "StartTime");
                RaisePropertyChanged(() => StartTimeTag);
            }
        }
        /// <summary>
        /// The <see cref="StopTimeTag" /> property's name.
        /// </summary>
        public const string StopTimeTagPropertyName = "StopTimeTag";

        private Datapool.DPGroupTagName stopTimeTag;

        /// <summary>
        /// Sets and gets the "StopTimeTagproperty.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Datapool.DPGroupTagName StopTimeTag
        {
            get
            {
                return stopTimeTag;
            }

            set
            {
                if (stopTimeTag == value)
                {
                    return;
                }

                stopTimeTag = value;
                Model.ThermoInterface.UpdateTag(value, "StopTime");
                RaisePropertyChanged(() => StopTimeTag);
            }
        }
        /// <summary>
        /// The <see cref="CalculateTag" /> property's name.
        /// </summary>
        public const string CalculateTagPropertyName = "CalculateTag";

        private Datapool.DPGroupTagName calculateTag;

        /// <summary>
        /// Sets and gets the CalculateTag property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Datapool.DPGroupTagName CalculateTag
        {
            get
            {
                return calculateTag;
            }

            set
            {
                if (calculateTag == value)
                {
                    return;
                }

                calculateTag = value;
                Model.ThermoInterface.UpdateTag(value, "Calculate");
                RaisePropertyChanged(() => CalculateTag);
            }
        }
        private RelayCommand<MouseEventArgs> mouseDoubleClickCommand;

        /// <summary>
        /// Gets the MouseDoubleClickCommand.
        /// </summary>
        public RelayCommand<MouseEventArgs> MouseDoubleClickCommand
        {
            get
            {
                return mouseDoubleClickCommand
                    ?? (mouseDoubleClickCommand = new RelayCommand<MouseEventArgs>(
                    (p) =>
                    {
                        string name = ((TextBox)p.Source).Name;
                        Messenger.Default.Send(new NotificationMessage(name + " AddDp"));
                        AddDpDialogViewModel dpDialog = new AddDpDialogViewModel();
                    }));
            }
        }
    }
}
