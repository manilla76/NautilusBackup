/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:ManualRamosAddon.ViewModel"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using ManualRamosAddon.Model;
using CommonServiceLocator;

namespace ManualRamosAddon.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {

        }

        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();

        /// <summary>
        /// Gets the AddDpDialog property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public AddDpDialogViewModel AddDpDialog => ServiceLocator.Current.GetInstance<AddDpDialogViewModel>();

        /// <summary>
        /// Gets the Feeder property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public FeederViewModel Feeder => ServiceLocator.Current.GetInstance<FeederViewModel>();                  

        /// <summary>
        /// Gets the AppVM property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public ApplicationViewModel AppVM => ServiceLocator.Current.GetInstance<ApplicationViewModel>();

        /// <summary>
        /// Gets the AddFeederVM property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public AddFeederViewModel AddFeederVM => ServiceLocator.Current.GetInstance<AddFeederViewModel>();

        /// <summary>
        /// Gets the AddFeederVM property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public AutoIntervalViewModel AutoIntervalVM => ServiceLocator.Current.GetInstance<AutoIntervalViewModel>();

        /// <summary>
        /// Cleans up all the resources.
        /// </summary>
        public static void Cleanup()
        {
        }

        internal static void Init()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (ViewModelBase.IsInDesignModeStatic)
            {
                SimpleIoc.Default.Register<IDataService, Design.DesignDataService>();
            }
            else
            {
                SimpleIoc.Default.Register<IDataService, DataService>();
            }
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<FeederViewModel>();
            SimpleIoc.Default.Register<ApplicationViewModel>(true);
            SimpleIoc.Default.Register<AddFeederViewModel>();
            SimpleIoc.Default.Register<AutoIntervalViewModel>();
            SimpleIoc.Default.Register<AddDpDialogViewModel>();            
        }
    }
}