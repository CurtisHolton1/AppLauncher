using CurtInstaller.ViewModels;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CurtInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class InstallView : Window
    {
        InstallViewModel viewModel;
        public  InstallView(string startupMode)
        {
    
            InitializeComponent();
            viewModel = new InstallViewModel();
            this.DataContext = viewModel;
            viewModel.StartupMode = startupMode;
            InstallBar.Maximum = 100;
            //Installer installer = new Installer();
            //install files
            //search for all files(need background worker(update progress bar) and threading)
        }

        private async void Window_ContentRendered(object sender, EventArgs e)
        {
            var progressIndicator = new Progress<double>(ReportProgress);
            await viewModel.InstallWrapper(progressIndicator);
        }

        void ReportProgress(double value)
        {
            InstallBar.Value = value*100;
        }


        
        


    }
}
