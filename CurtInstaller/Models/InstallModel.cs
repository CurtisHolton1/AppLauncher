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
using Curt.shared;
namespace CurtInstaller.Models
{
   public class InstallModel
    {
       public InstallModel Model { get; set; }
        private int installValue;
        public int InstallValue { get { return installValue; } set { installValue = value; } }
        private string location;
        public string Location { get { return location; } set { location = value; } }
        
       WebClient client;
       public InstallModel()
       {      
          client = new WebClient();
       }

     

       public bool Download(string startupMode)
       {
           
           try
           {
           //    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
           //    client.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
           //    client.DownloadFileAsync(new Uri("http://squints.io/Curt/AppLauncher.zip"), Location+"\\AppLauncher.zip");
                
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
           SharedHelper.DeleteDirectory(Location + "\\AppLauncher\\Content");
           try
           {
               
               if (!Directory.Exists(Location + "\\tmp\\AppLauncher"))
               {
                   Directory.CreateDirectory(Location + "\\tmp");
               }
              
               if (!Directory.Exists(Location + "\\tmp\\AppLauncher\\"))
                   Directory.CreateDirectory(Location + "\\tmp\\AppLauncher\\");

               MoveFiles(Location + "\\AppLauncher\\AppLauncher", Location + "\\tmp\\AppLauncher");
               File.Move(Location + "\\tmp\\AppLauncher\\InstalledSoftware.bin", Location + "\\AppLauncher\\AppLauncher\\InstalledSoftware.bin");
               var b = InstallFiles();
                
               return b;
           }
           catch (Exception ex)
           {
               System.Windows.MessageBox.Show("Error in Update Method:" + ex.Message);
               return false;
           }
       }

       private void MoveFiles(string toMove, string dest)
       {
           var files = Directory.GetFiles(toMove);
           if (!Directory.Exists(dest))
               Directory.CreateDirectory(dest);
           foreach (var aFile in files.ToList<string>())
           {
               var fileName = aFile.Split('\\').Last();
               File.Move(aFile, dest + "\\" + fileName);
           }
       }
      

       public void RollBack()
       {
           System.Windows.MessageBox.Show("Something failed, rolling back changes, please wait.");
           //CloseLauncher();
           SharedHelper.DeleteDirectory(Location + "\\AppLauncher\\AppLauncher", new List<string> {"InstalledSoftware.bin"});
           SharedHelper.DeleteDirectory(Location + "\\AppLauncher\\CurtInstaller");
           MoveFiles(Location + "\\tmp\\AppLauncher", Location + "\\AppLauncher\\AppLauncher");
           MoveFiles(Location + "\\tmp\\CurtInstaller", Location + "\\AppLauncher\\CurtInstaller");
 
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
