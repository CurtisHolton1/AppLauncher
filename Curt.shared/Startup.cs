using Curt.shared.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;


namespace Curt.shared
{
   public static class Startup
    {

       public static bool  SetStartup(){
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (rk.GetValue("AppLauncher") == null)
            {
                rk.SetValue("AppLauncher",Directory.GetCurrentDirectory() + "\\AppLauncher.exe");
                return true;
            }
            else return false;
       }

       public static void RemoveStartup()
       {
           RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
           if (rk.GetValue("AppLauncher") != null)
               rk.DeleteValue("AppLauncher");
           File.Delete("InstalledSoftware.bin");
           
       }


    }
}
