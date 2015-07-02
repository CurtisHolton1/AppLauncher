using Curt.Helpers;
using Curt.shared;
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
    class InstallViewModel : ViewModelBase
    {
        private InstallModel model;
        private int installValue;
        public int InstallValue { get { return installValue; } set { installValue = value; model.InstallValue = value; RaisePropertyChanged("InstallValue"); } }
        public string StartupMode { get; set; }
        public InstallViewModel()
        {
            model = Factory.GetInstallModel();
            model.CloseLauncher();

        }

        public async Task<string> InstallWrapper()
        {
            try
            {
                if (!string.IsNullOrEmpty(StartupMode) && StartupMode.Equals("Update"))
                {
                    model.Location = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
                    model.Location = System.IO.Directory.GetParent(model.Location).FullName;                 
                    model.Location = System.IO.Directory.GetParent(model.Location).FullName;  
                    //model.Location = @"C:\Users\Curtis\Desktop\Curt";

                }
                var success = model.Download(StartupMode);
                //////////////////////////////////////
               // success = false;
                ///////////////////////////////////////
                if (success)
                {
                    if (string.IsNullOrEmpty(StartupMode)) //install mode
                    {
                        FileWriteRead fileObject = new FileWriteRead();
                        await fileObject.WriteFile(Startup.GetInitialLocations(), model.Location);
                    }
                }
                else { model.RollBack(); }            
                model.StartLauncher();
                System.Environment.Exit(0);             
            }               
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("error in installwrapper: " + e.Message);
            }
            return "";
        }





    }
}
