using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AppLauncher.Models
{
   public static class Startup
    {

       public static bool  SetStartup(){
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (rk.GetValue("AppLauncher") == null)
            {
                rk.SetValue("AppLauncher", System.Reflection.Assembly.GetExecutingAssembly().Location);
                return true;
            }
            else return false;
       }

       public static void RemoveStartup()
       {
           RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
           if (rk.GetValue("AppLauncher") != null)
               rk.DeleteValue("AppLauncher");
       }


       private static List<DirSearchItem> FillList()
       {
           List<DirSearchItem> locations = new List<DirSearchItem>();
           // locations.Add(new DirSearchItem { Path = "C://Windows", Levels = 1 });          
           //locations.Add(new DirSearchItem { Path = "C://Program Files (x86)", Levels = 1000 });
           // locations.Add(new DirSearchItem { Path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), Levels = 100000 });
           locations.Add(new DirSearchItem { Path = Environment.GetFolderPath(Environment.SpecialFolder.Windows), Levels = 1 });
           locations.Add(new DirSearchItem { Path = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), Levels = 1000 });
           locations.Add(new DirSearchItem { Path = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), Levels = 1000 });
           locations.Add(new DirSearchItem { Path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Levels = 1000 });
           locations.Add(new DirSearchItem { Path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Levels = 1000 });
           locations.Add(new DirSearchItem { Path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), Levels = 1000 });
           locations.Add(new DirSearchItem { Path = "C://Program Files", Levels = 1000 });
           //locations.Add(new DirSearchItem { Path = Environment.GetFolderPath(Environment.SpecialFolder.Startup), Levels = 1000 });
           return locations;
       }

       public static async Task<List<Executable>> GetInstalledSoftware()
       {
           List<DirSearchItem> locations = FillList();
           int index;
           List<Executable> software = new List<Executable>();
           #region comments
           //gets some executables
           //string SoftwareKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\App Paths";
           //using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(SoftwareKey))
           //{
           //    foreach (string skName in rk.GetSubKeyNames())
           //    {
           //        using (RegistryKey sk = rk.OpenSubKey(skName))
           //        {
           //            try
           //            {
           //                if (sk.GetValue("") != null)
           //                {
           //                    string name = sk.Name;
           //                    if (name.Contains(".exe"))
           //                    {
           //                        name = name.Substring(83, name.Length - 87);
           //                    }
           //                    else
           //                    {
           //                        name = name.Substring(83, name.Length - 83);
           //                    }                               
           //                    string loc = (string)sk.GetValue("");
           //                    if (loc.Contains("system32"))
           //                    {
           //                        loc = loc.Replace("system32", "sysnative");
           //                    }
           //                    if (loc.Contains("Program Files (x86)") && loc.Contains("Microsoft Shared"))
           //                    {
           //                        loc = loc.Replace("Program Files (x86)", "Program Files");
           //                    }
           //                    try
           //                    {
           //                        Icon ico = System.Drawing.Icon.ExtractAssociatedIcon((string)sk.GetValue(""));
           //                        ImageSource source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(ico.Handle, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
           //                        software.Add(new Executable { Name = name + "*", Location = loc, ImgSrc = source });
           //                    }
           //                    catch (Exception)
           //                    {
           //                        software.Add(new Executable { Name = name + "*", Location = loc });
           //                    }
           //                }
           //            }
           //            catch (Exception)
           //            {
           //            }
           //        }
           //    }
           //}


           //gets paths to uninstall locations
           //SoftwareKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
           //using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(SoftwareKey))
           //{
           //    foreach (string skName in rk.GetSubKeyNames())
           //    {
           //        using (RegistryKey sk = rk.OpenSubKey(skName))
           //        {
           //            try
           //            {
           //                if (sk.GetValue("SystemComponent") == null || ((int)sk.GetValue("SystemComponent") == 0))
           //                    // if (sk.GetValue("WindowsInstaller") == null || ((int)sk.GetValue("WindowsInstaller", 0) == 0))
           //                    //if (sk.GetValue("UninstallString") != null)
           //                    //if (sk.GetValue("ParentKeyName") == null)
           //                    if (sk.GetValue("DisplayName") != null)
           //                    {
           //                        if (sk.GetValue("InstallLocation") != null && !sk.GetValue("InstallLocation").Equals(""))
           //                        {
           //                            index = software.FindIndex(f => f.Name.Equals(sk.GetValue("DisplayName")));
           //                            if (index < 0)
           //                            {
           //                                software.Add(new Executable { Name = (string)sk.GetValue("DisplayName"), Location = (string)sk.GetValue("InstallLocation") });
           //                            }
           //                        }
           //                    }
           //            }
           //            catch (Exception)
           //            {
           //            }
           //        }
           //    }
           //}

           //SoftwareKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
           //using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(SoftwareKey))
           //{

           //    foreach (string skName in rk.GetSubKeyNames())
           //    {
           //        using (RegistryKey sk = rk.OpenSubKey(skName))
           //        {
           //            try
           //            {
           //                // if ((sk.GetValue("DisplayName") != null) || ((int)sk.GetValue("SystemComponent",0) != 1) || (int)sk.GetValue("WindowsInstaller", 0) !=1|| sk.GetValue("UninstallString"))
           //                if (sk.GetValue("SystemComponent") == null || ((int)sk.GetValue("SystemComponent") == 0))
           //                    //if (sk.GetValue("WindowsInstaller") == null || ((int)sk.GetValue("WindowsInstaller", 0) == 0))
           //                    //   if (sk.GetValue("UninstallString") != null)
           //                    //if (sk.GetValue("ParentKeyName") == null)
           //                    if (sk.GetValue("DisplayName") != null)
           //                    {
           //                        if (sk.GetValue("InstallLocation") != null && !sk.GetValue("InstallLocation").Equals(""))
           //                        {
           //                            index = software.FindIndex(f => f.Name.Equals(sk.GetValue("DisplayName")));
           //                            if (index < 0)
           //                                software.Add(new Executable { Name = (string)sk.GetValue("DisplayName"), Location = (string)sk.GetValue("InstallLocation") });

           //                        }
           //                    }
           //            }
           //            catch (Exception)
           //            {
           //            }
           //        }
           //    }
           //} 
           #endregion
           /*
             * Look at registry to find areas that might hold user executables 
             * Add locations to the search
             */
           RegistryKey key = key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
           string[] users = Registry.CurrentUser.GetSubKeyNames();
           if (key != null)
               foreach (string skName in key.GetSubKeyNames())
               {
                   using (RegistryKey sk = key.OpenSubKey(skName))
                   {
                       try
                       {
                           index = locations.FindIndex(f => f.Path.Equals((string)sk.GetValue("InstallLocation")));
                           if (index < 0)
                               locations.Add(new DirSearchItem { Path = (string)sk.GetValue("InstallLocation"), Levels = 1000 });
                       }
                       catch (Exception) { }
                   }
               }
           List<System.IO.FileInfo> tmp = new List<System.IO.FileInfo>();
           foreach (DirSearchItem d in locations)
           {
               tmp = TraverseTree(d.Path, d.Levels);
               if (tmp != null)
                   foreach (System.IO.FileInfo fileInfo in tmp)
                   {
                       //  if (DateTime.UtcNow.Year == fileInfo.LastAccessTimeUtc.Year && DateTime.UtcNow.Month - fileInfo.LastAccessTimeUtc.Month <= 2)  
                       index = software.FindIndex(f => f.Name.Equals(fileInfo.Name));
                       if (index < 0)
                           try
                           {
                              // Icon ico = System.Drawing.Icon.ExtractAssociatedIcon(fileInfo.ToString());
                              // ImageSource source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(ico.Handle, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                               software.Add(new Executable { Name = fileInfo.Name, Location = fileInfo.FullName, LastUsed = fileInfo.LastAccessTime });
                           }
                           catch (Exception)
                           {
                               software.Add(new Executable { Name = fileInfo.Name, Location = fileInfo.FullName, LastUsed = fileInfo.LastAccessTime });
                           }
                   }
           }
           return software;
       }

       private static List<System.IO.FileInfo> TraverseTree(string root, int levels)
       {
           List<System.IO.FileInfo> retList = new List<System.IO.FileInfo>();
           // Data structure to hold names of subfolders to be 
           // examined for files.
           Stack<string> dirs = new Stack<string>(1000);
           if (!System.IO.Directory.Exists(root))
           {
               throw new ArgumentException();
           }
           dirs.Push(root);

           while (dirs.Count > 0)
           {
               string currentDir = dirs.Pop();
               string[] subDirs;
               try
               {
                   subDirs = System.IO.Directory.GetDirectories(currentDir);

                   string[] files = null;

                   files = System.IO.Directory.GetFiles(currentDir);
                   foreach (string file in files)
                   {
                       try
                       {
                           System.IO.FileInfo fi = new System.IO.FileInfo(file);
                           if (fi.Extension.Equals(".exe") || fi.Extension.Equals(".lnk"))
                           {
                               retList.Add(fi);
                           }
                       }
                       catch (Exception) { }
                   }
                   int i = 0;
                   while (i < levels && i < subDirs.Length)
                   {
                       string str = subDirs[i];
                       dirs.Push(str);
                       i++;
                   }
               }
               catch (Exception) { }
           }
           return retList;
       }

    }
}
