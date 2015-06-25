using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Windows;
using System.ComponentModel;
using System.IO.Compression;
using System.IO;
using System.Diagnostics;
namespace AppLauncher.Services
{
   public class VersionCheck
    {
       
       static WebClient client;
       public static void CompareCurrent(){
           try
           {
               client = new WebClient();
               string version = client.DownloadString("http://squints.io/Curt/Version.txt");
               double versionNumber = Convert.ToDouble(version);

               if(versionNumber > Convert.ToDouble(ConfigurationManager.AppSettings["Version"])){
                   var w = WindowWatcher.GetWindowOfType<MainWindow>();
                   w.IsEnabled = false;
                   string sMessageBoxText = "A new version is available, would you like to update?";
                   string sCaption = "Curt";
                   MessageBoxButton btnMessageBox = MessageBoxButton.YesNo;
                   MessageBoxImage icnMessageBox = MessageBoxImage.Warning;
                   MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
                   switch (rsltMessageBox)
                   {
                       case MessageBoxResult.Yes:
                           {
                               //Process.Start("CurtInstaller.exe");
                               w.Close();
                               
                               Console.WriteLine("Starting download");
                              
                               break;
                           }
                       case MessageBoxResult.No:
                           {
                               w.IsEnabled = true;
                               w.Show();
                               Console.WriteLine("Okay jeez...");
                               break;
                           }
                   }

               }
           }
           catch (Exception)
           {
            
           }                    
       }

       

    }
}
