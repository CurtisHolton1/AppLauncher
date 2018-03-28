using Curt.shared;
using Curt.shared.Models;
using System;
using System.IO;
using CurtInstaller.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            DatabaseManager.SetDBLocation(model.Location + "\\AppLauncher\\FilesData.sqlite");
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
                //                var success = await Task.Run(() => model.Download(StartupMode));

                await Task.Run(() => model.InstallFiles());
                //////////////////////////////////////
                // success = false;
                ///////////////////////////////////////
                //                if (success)
                //                {
                if (string.IsNullOrEmpty(StartupMode)) //install mode
                    {
                        DatabaseManager.CreateDB();

                        using (StreamReader sr = new StreamReader("Resources\\WhiteListTmp.txt"))
                        {
                            var toWrite = new List<Extension>();
                            var all = sr.ReadToEnd();
                            var tmp = all.Split('\t').ToList();
                            foreach (var t in tmp)
                            {
                                toWrite.Add(new Extension { Type = t, IsChecked = true });
                            }
                            await Task.Run(() => DatabaseManager.CreateFilesTable(progressIndicator));
                            await Task.Run(() =>DatabaseManager.CreateWhiteListTable(toWrite));
                            var FilesFound = await Task.Run(() => FileSearch.FileSpider("c:\\", "*"));
                            await Task.Run(()=>DatabaseManager.InsertIntoFilesTable(FilesFound.ToList()));
                            await Task.Run(() => DatabaseManager.CreateCommandsTable());                                                                                                                      
                        }
                        ///////////////////////////
                        //if(File.Exists("WhiteListTmp.txt"))
                        //File.Delete("WhiteListTmp.txt");
                        /////////////////////////////
                    }
//                }
//                else
//                { 
//                    model.RollBack();
//                }
//                    model.StartLauncher();
//                    System.Environment.Exit(0);
                }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("error in installwrapper: " + e.Message);
            }
            return "";
        }
    }
}
