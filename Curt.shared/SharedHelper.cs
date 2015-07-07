﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curt.shared
{
    public static class SharedHelper
    {
        public static void DeleteDirectory(string target_dir, List<string> ignored = null)
        {
            if (Directory.Exists(target_dir))
            {
                string[] files = Directory.GetFiles(target_dir);
                string[] dirs = Directory.GetDirectories(target_dir);

                foreach (string file in files)
                {
                    if (ignored == null || !ignored.Contains(file))
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                        File.Delete(file);
                    }
                }

                foreach (string dir in dirs)
                {
                    DeleteDirectory(dir,ignored);
                }

                Directory.Delete(target_dir, false);
            }
        }
        public static void KillProcess(string processToKill)
        {
            Process[] processlist = Process.GetProcesses();
            foreach (Process theprocess in processlist)
            {
                if (theprocess.ProcessName.Equals(processToKill))
                {
                    theprocess.Kill();
                    theprocess.WaitForExit();
                }
            }
        }

        public static void StartInstaller(string arg)
        {
            Process p = new Process();                      
            p.StartInfo.Arguments = arg;
            p.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory +"..\\CurtInstaller\\CurtInstaller.exe";
            p.Start();
        }

    }
}
