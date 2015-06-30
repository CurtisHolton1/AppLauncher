using CurtInstaller;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Curtinstaller

{
   public class Installer 
    {
       WebClient client;
       public Installer()
       {
           client = new WebClient();
       }

       private  void Download()
       {
           try
           {
               client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
               client.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
               client.DownloadFileAsync(new Uri("http://squints.io/Curt/AppLauncher.exe.zip"), "../../tmp/AppLauncher.exe.zip");
           }
           catch (Exception e)
           {
               System.Windows.MessageBox.Show("Error downloading file: " + e.Message);
           }
       }

       private  void Completed(object sender, AsyncCompletedEventArgs e)
       {
           Install();
       }

       private  void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
       {
           
       }

       private void Install()
       {
           try
           {
               System.IO.Compression.ZipFile.ExtractToDirectory("../../tmp", "AppLauncher.exe.zip");
           }

           catch (Exception exc)
           {
               Console.Write("fdsaf");
           }
       }

    }
}
