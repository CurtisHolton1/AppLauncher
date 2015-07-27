using System;
using System.Net;
using System.Threading.Tasks;
using System.Configuration;
using System.Windows;
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
           try
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
                           StartInstaller("Update");
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
           catch (Exception e)
           {
               System.Windows.MessageBox.Show(e.Message);
               return false;
           }
       }


       public static void StartInstaller(string arg)
       {
           Process p = new Process();
           if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\tmp"))
           {
               Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\tmp");
           }
           Directory.Move(AppDomain.CurrentDomain.BaseDirectory + "..\\CurtInstaller", AppDomain.CurrentDomain.BaseDirectory + "..\\..\\tmp\\CurtInstaller");
           p.StartInfo.Arguments = arg;
           p.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\tmp\\CurtInstaller\\CurtInstaller.exe";
           p.Start();
       }
    }
}
