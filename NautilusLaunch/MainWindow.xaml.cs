using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Linq.Expressions;

namespace OmniLaunch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();            
        }
        private void StartList_Drop(object sender, DragEventArgs e)
        {
            if (((MainWindowViewModel)DataContext).StartList.Count == 1 && ((MainWindowViewModel)DataContext).StartList[0] == string.Empty)
            {
                ((MainWindowViewModel)DataContext).StartList.Clear();
            }
            ((MainWindowViewModel)DataContext).StartList.Add((string)e.Data.GetData(DataFormats.StringFormat));            
        }

        private void AppList_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                //Start Drag...Copy selected value
                DataObject data = new DataObject();
                data.SetData(DataFormats.StringFormat, ((ListView)sender).SelectedItem);                
                DragDrop.DoDragDrop(this, data, DragDropEffects.Copy);
            }
        }

        private void StartList_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete || e.Key == System.Windows.Input.Key.Back)
            {
                foreach (string item in ((ListView)sender).SelectedItems)
                {

                }
                var temp = ((MainWindowViewModel)DataContext).StartList.Where(s => !(((ListView)sender).SelectedItems).Contains(s));
                
                //foreach (string item in ((ListView)sender).SelectedItems)
                //{
                //    ((MainWindowViewModel)DataContext).StartList.Remove(item);
                //}                
            }            
        }
    }
}
