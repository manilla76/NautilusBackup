﻿using System.Windows;
using SandBox.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Threading;

namespace SandBox
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