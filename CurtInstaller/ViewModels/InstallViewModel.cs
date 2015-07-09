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
        public string StartupMode { get; set; }
        public InstallViewModel()
        {
            model = Factory.GetInstallModel();
            SharedHelper.KillProcess("AppLauncher");

        }

        public async Task<string> InstallWrapper(System.IProgress<double> progressIndicator)
        {
            try
            {
                if (!string.IsNullOrEmpty(StartupMode) && StartupMode.Equals("Update"))
                {
                    model.Location = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
                    model.Location = System.IO.Directory.GetParent(model.Location).FullName;
                    model.Location = System.IO.Directory.GetParent(model.Location).FullName;  
                }
                else if (!string.IsNullOrEmpty(StartupMode) && StartupMode.Equals("WriteFile"))
                {
                    model.Location = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
                    model.Location = System.IO.Directory.GetParent(model.Location).FullName;
                    model.Location = System.IO.Directory.GetParent(model.Location).FullName;  
                    FileWriteRead fileObject = new FileWriteRead();
                    await fileObject.WriteFile(Startup.GetInitialLocations(),model.Location, progressIndicator);
                    model.StartLauncher();
                    System.Environment.Exit(0); 
                    return "";
                }
                var success = await Task.Run(()=> model.Download(StartupMode));
                //////////////////////////////////////
               // success = false;
                ///////////////////////////////////////
                if (success)
                {
                    if (string.IsNullOrEmpty(StartupMode)) //install mode
                    {
                        FileWriteRead fileObject = new FileWriteRead();
                        await fileObject.WriteFile(Startup.GetInitialLocations(), model.Location, progressIndicator);
                     await Task.Run(()=> FileSearch.CreateFilesDatabase(model.Location + "\\AppLauncher\\AppLauncher","FilesDatabase.sqlite",true));
                      
                    }           
                }
                else
                {
                    model.RollBack();                
                }
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
