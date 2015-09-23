using CurtInstaller.ViewModels;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using CurtInstaller.Properties;

namespace CurtInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class InstallView : Window , INotifyPropertyChanged
    {
        InstallViewModel viewModel;
        
        public  InstallView(string startupMode)
        {
            
            InitializeComponent();
            LoadingImg.Source = new Uri("Resources/Loading.gif", UriKind.Relative);
            LoadingImg.Play();
            viewModel = new InstallViewModel();
            this.DataContext = this;          
            viewModel.StartupMode = startupMode;  
            
    
        }

        private async void Window_ContentRendered(object sender, EventArgs e)
        {
            var progressIndicator = new Progress<double>(ReportProgress);
            await viewModel.InstallWrapper(progressIndicator);
        }

        void ReportProgress(double value)
        {
            //InstallBar.Value = value*100;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }




        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void LoadingImg_MediaEnded(object sender, RoutedEventArgs e)
        {
            LoadingImg.Position = new TimeSpan(0,0,1);
            LoadingImg.Play();
        }
    }
}
