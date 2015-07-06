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


      public static List<DirSearchItem> GetInitialLocations()
       {
           List<DirSearchItem> locations = new List<DirSearchItem>();

           locations.Add(new DirSearchItem { Path = Environment.GetFolderPath(Environment.SpecialFolder.Windows), Levels = 1000 });
           locations.Add(new DirSearchItem { Path = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), Levels = 1000 });
           locations.Add(new DirSearchItem { Path = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), Levels = 1000 });
           locations.Add(new DirSearchItem { Path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Levels = 1000 });
           locations.Add(new DirSearchItem { Path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Levels = 1000 });
           locations.Add(new DirSearchItem { Path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), Levels = 1000 });
           locations.Add(new DirSearchItem { Path = "C://Program Files", Levels = 1000 });
           locations.Add(new DirSearchItem { Path = "C://Users", Levels = 1000 });

           return locations;
       }

      /*
              * Look at registry to find areas that might hold user executables 
              * Add locations to search
              */
      //public static async Task<List<DirSearchItem>> SearchRegistry()
      //{
      //    List<DirSearchItem> locations = new List<DirSearchItem>();
      //    int index;         
      //    RegistryKey key = key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
      //    string[] users = Registry.CurrentUser.GetSubKeyNames();
      //    if (key != null)
      //        foreach (string skName in key.GetSubKeyNames())
      //        {
      //            using (RegistryKey sk = key.OpenSubKey(skName))
      //            {
      //                try
      //                {
      //                    index = locations.FindIndex(f => f.Path.Equals((string)sk.GetValue("InstallLocation")));
      //                    if (index < 0)
      //                        locations.Add(new DirSearchItem { Path = (string)sk.GetValue("InstallLocation"), Levels = 1000 });
      //                }
      //                catch (Exception) { }
      //            }
      //        }
      //    return locations;
      //}


       public static async Task<List<Executable>> GetInstalledSoftware(List<DirSearchItem> locations, IProgress<double> progress)
       {
          // var RegistryList = await SearchRegistry();
           //if (RegistryList != null && RegistryList.Count > 0)
           //    for (int i = 0; i < RegistryList.Count; i++)
           //        locations.Add(RegistryList[i]);
           
           int index;
           double count = 1;
           List<Executable> software = new List<Executable>();                  
           List<System.IO.FileInfo> tmp = new List<System.IO.FileInfo>();
           
           foreach (DirSearchItem d in locations)
           {
               if (progress != null)
               {
                   progress.Report(count / locations.Count);
               }
               tmp = TraverseTree(d.Path, d.Levels);
               if (tmp != null)
                   foreach (System.IO.FileInfo fileInfo in tmp)
                   {
                       //  if (DateTime.UtcNow.Year == fileInfo.LastAccessTimeUtc.Year && DateTime.UtcNow.Month - fileInfo.LastAccessTimeUtc.Month <= 2)  
                       index = software.FindIndex(f => f.Name.Equals(char.ToUpper(fileInfo.Name[0]) + fileInfo.Name.Substring(1)));
                       if (index < 0)
                           try
                           {                                                         
                              // Icon ico = System.Drawing.Icon.ExtractAssociatedIcon(fileInfo.ToString());
                              // ImageSource source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(ico.Handle, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                               software.Add(new Executable { Name = char.ToUpper(fileInfo.Name[0]) + fileInfo.Name.Substring(1) , Location = fileInfo.FullName, LastUsed = fileInfo.LastAccessTime });
                           }
                           catch (Exception)
                           {
                               software.Add(new Executable { Name = char.ToUpper(fileInfo.Name[0]) + fileInfo.Name.Substring(1), Location = fileInfo.FullName, LastUsed = fileInfo.LastAccessTime });
                           }
                   }
               count++;
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
               string[] a = currentDir.Split('\\');

               if(a.Length >=3 && a[2].Equals("Downloads")){
                   currentDir = dirs.Pop();//skip downloads folder
               }
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
                           if (file.Length < 256)
                           {
                               System.IO.FileInfo fi = new System.IO.FileInfo(file);

                               if (fi.Extension.Equals(".exe"))
                               {
                                   retList.Add(fi);
                               }
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
