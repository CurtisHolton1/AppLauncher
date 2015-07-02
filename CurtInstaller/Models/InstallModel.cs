using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;

namespace CurtInstaller.Models
{
   public class InstallModel
    {
       public InstallModel Model { get; set; }
        private int installValue;
        public int InstallValue { get { return installValue; } set { installValue = value; } }
        private string location;
        public string Location { get { return location; } set { location = value; } }
        private List<string> filesInZip;
       WebClient client;
       public InstallModel()
       {      
          client = new WebClient();
          filesInZip = new List<string>();
       }

       public void CloseLauncher()
       {
           Process[] processlist = Process.GetProcesses();
           foreach (Process theprocess in processlist)
           {
               if (theprocess.ProcessName.Equals("AppLauncher"))
                   theprocess.Kill();
           }
       }

       public bool Download(string startupMode)
       {
           
           try
           {
           //    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
           //    client.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
           //    client.DownloadFileAsync(new Uri("http://squints.io/Curt/AppLauncher.zip"), Location+"\\AppLauncher.zip");
               CloseLauncher();
               client.DownloadFile(new Uri("http://squints.io/Curt/AppLauncher.zip"), Location + Properties.Settings.Default.zipName);
               if (!string.IsNullOrEmpty(startupMode) && startupMode.Equals("Update"))
                   return Update();
               else
             return  InstallFiles();
           }
           catch (Exception e)
           {
               System.Windows.MessageBox.Show("Error downloading file: " + e.Message);
               return false;

           }
       }

       private void Completed(object sender, AsyncCompletedEventArgs e)
       {
            //used for async download
           //InstallFiles();
       }

       private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
       {
           //used for async download
         //for install bar

       }

       private bool InstallFiles()
       {          
           try
           {
               

             
              //foreach (var e in entries)
              //{
              //    filesInZip.Add(e.FullName);
              //}

              //if (!System.IO.Directory.Exists(Location + "\\tmp"))
              //{
              //    System.IO.Directory.CreateDirectory(Location + "\\tmp\\CurtInstaller");
                  
              //}
              //foreach (var f in filesInZip)
              //{
              //    if (File.Exists(Location + "\\" + f))
              //    {
              //        if (Directory.Exists(Location + "\\tmp\\" + f))
              //        {
              //            File.Move(Location + "\\" + f, Location + "\\tmp\\" + f);
              //        }
              //        else
              //        {
              //            Directory.CreateDirectory(Location + "\\tmp\\" + f);
              //            File.Move(Location + "\\" + f, Location + "\\tmp\\" + f);
              //        }
                          
              //    }
              //}
              //zip.Dispose();
               var z = ZipFile.OpenRead(Location + Properties.Settings.Default.zipName);
               z.ExtractToDirectory(Location);
               z.Dispose();
               File.Delete(Location + Properties.Settings.Default.zipName);
              return true;               
           }
           catch (Exception exc)
           {
               System.Windows.MessageBox.Show("error in Installfiles:" + exc.Message);
               return false;
           }
       }

       private bool Update()
       {
           try
           {
               if (!Directory.Exists(Location + "\\tmp\\AppLauncher"))
               {
                   Directory.CreateDirectory(Location + "\\tmp");
               }
               //Directory.Move(Location + "\\AppLauncher\\AppLauncher", Location + "\\tmp\\AppLauncher");     
               foreach (var f in Directory.GetFiles(Location + "\\AppLauncher\\AppLauncher"))
               {
                   File.Move(f, Location + "\\tmp\\AppLauncher" + f);
               }
               var b = InstallFiles();
               File.Move(Location + "\\tmp\\AppLauncher\\InstalledSoftware.bin", Location + "\\AppLauncher\\AppLauncher\\InstalledSoftware.bin");
               return b;
           }
           catch (Exception ex)
           {
               System.Windows.MessageBox.Show("Error in Update Method:" + ex.Message);
               return false;
           }
       }

       public void RollBack()
       {
           System.Windows.MessageBox.Show("Something failed, rolling back changes, please wait.");
           //CloseLauncher();
           //for (int i = 0; i < filesInZip.Count; i++)
           //{
           //    File.Delete(filesInZip[i]);
           //}
           //var dir = Directory.GetFiles(Location + "\\tmp","*");
           //foreach (var f in dir)
           //{
           //    File.Move(Location+"\\tmp\\" + f,f);
           //}       
       }

       public void StartLauncher()
       {
           Process p = new Process();
           p.StartInfo.WorkingDirectory = Location + "\\AppLauncher\\AppLauncher";
           p.StartInfo.FileName = Location + "\\AppLauncher\\AppLauncher\\AppLauncher.exe";
           p.Start();
       }

       void RaisePropertyChanged(string prop)
       {
           if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }
       }
       public event PropertyChangedEventHandler PropertyChanged;

    }
}
