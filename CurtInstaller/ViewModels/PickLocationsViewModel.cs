using CurtInstaller.Helpers;
using CurtInstaller.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CurtInstaller.ViewModels
{
    class PickLocationsViewModel:ViewModelBase
    {
        private string location;
        public string Location { get { return location; } set { location = value; model.Location = value; RaisePropertyChanged("Location"); } }
        private InstallModel model;
        public PickLocationsViewModel()
        {
            NextButtonCommand = new RelayCommand(NextButton);
            BrowseButtonCommand = new RelayCommand(BrowseButton);
            model=  Factory.GetInstallModel();
        }
        public RelayCommand NextButtonCommand { get; set; }
        public RelayCommand BrowseButtonCommand { get; set; }
        void NextButton(object parameter)
        {
            if (parameter == null) return;
            if (Directory.Exists(parameter.ToString()))
            {             
                var wnd = new InstallView(null);
                wnd.Show();
                CloseWindow();
            }
            else if (!Directory.Exists(parameter.ToString()) && !string.IsNullOrEmpty(parameter.ToString()))
            {                  
             Location = Directory.CreateDirectory(parameter.ToString()).FullName;
             var wnd = new InstallView(null);
             wnd.Show();
             CloseWindow();
            }        
        }

        void BrowseButton(object parameter)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            if (result.ToString().Equals("OK"))
                Location = fbd.SelectedPath;
        }
    }
}
