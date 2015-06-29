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
   public static class VersionCheck
    {     
       static WebClient client;
       public static bool windowOpen = false;
       public static async Task<bool> CompareCurrent(){
           try
           {
               client = new WebClient();
               string version = client.DownloadString("http://squints.io/Curt/Version.txt");
               double versionNumber = Convert.ToDouble(version);
               if(versionNumber > Convert.ToDouble(ConfigurationManager.AppSettings["Version"])){               
                   return true;
               }
               return false;
           }
           catch (Exception e)
           {
               return false;
           }                    
       }

       public static bool AskForUpdate()
       {
           string sMessageBoxText = "A new version is available, would you like to update?";
           string sCaption = "Curt";
           windowOpen = true;
           MessageBoxButton btnMessageBox = MessageBoxButton.YesNo;
           MessageBoxImage icnMessageBox = MessageBoxImage.Warning;
           MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

           switch (rsltMessageBox)
           {
                  
               case MessageBoxResult.Yes:
                   {
                       //Process.Start(@"CurtInstaller\CurtInstaller.exe");
                       Process p = new Process();
                       p.StartInfo.Arguments = "Update";
                       p.StartInfo.FileName = @"CurtInstaller\CurtInstaller.exe";
                       p.Start();
                       windowOpen = false;
                       System.Environment.Exit(0);
                       return true;
                   }
               case MessageBoxResult.No:
                   {
                       windowOpen = false;
                       return false;
                   }
           }
           return false;
       }

    }
}
