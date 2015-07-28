using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Curt.shared.Models;
using System.Threading.Tasks;

namespace Curt.shared
{
    public static class FileSearch
    {
        private static List<Extension> WhiteListedExtensions = new List<Extension>();

		/// <summary>
		/// Returns all files starting at the provided root and working its way down all sub folders
		/// </summary>
		/// <param name="Root">Root path to begin search</param>
		/// <param name="SearchString">String to search for in file names. Insert "*" to find all</param>
		/// <returns></returns>
		public static async Task<List<FileItem>> FileSpider(string Root, String SearchString)
        {
            
			try
			{
                WhiteListedExtensions = DatabaseManager.GetAllFromWhiteList();
				return RecursiveFileSpider(Root, SearchString);
			}
			catch (Exception Ex)
			{
				return null;
			}

		}


		/// <summary>
		/// Recursivly searchs folders for file matching the search string 
		/// </summary>
		/// <param name="Root">Root Folder to begin Search</param>
		/// <param name="SearchString">File name to match. Use "*" to return all files.</param>
		/// <returns></returns>
		private static List<FileItem> RecursiveFileSpider(string Root, string SearchString)
		{               
			DirectoryInfo dirPrograms = new DirectoryInfo(Root);
			List<FileItem> FilesFound = new List<FileItem>() { };
            FilesFound.Add(new FileItem { FileName = dirPrograms.Name, FileLocation = dirPrograms.FullName, Type = FileType.folder, ExtensionID = 1 });
			var Dirs = dirPrograms.EnumerateDirectories();
            try
            {
                var files = dirPrograms.EnumerateFiles(SearchString).Where(x => x != null && x.Name != null && x.Name != "" && !x.Extension.Equals("") && x.FullName.Length < 255);
                object aLock = new object();
            
                foreach (var aFile in files)
                {
                    lock (aLock)
                    {
                        FileType type;
                        if (aFile.Extension.Equals(".exe"))
                            type = FileType.app;
                        else
                            type = FileType.file;
                        if (WhiteListedExtensions.FindIndex(x=>x.Type.Equals(aFile.Extension)) >0 && WhiteListedExtensions[WhiteListedExtensions.FindIndex(x => x.Type.Equals(aFile.Extension))].IsChecked)
                        {
                            FilesFound.Add(new FileItem { FileLocation = aFile.FullName, FileName = aFile.Name, ExtensionID = ExtensionConverter.ConvertFromString(aFile.Extension), LastUsed = DateTime.MinValue, TotalUsed = 0, Type = type });
                        }
                    }
                }

            }
            catch (Exception Ex)
            {
                return new List<FileItem>() { };
            }
			try
			{
				if (Dirs.Count() > 0)
				{
                    foreach (var Dir in Dirs)
                    {
                        try
                        {
                            if (!Dir.FullName.Contains("Windows") && !Dir.FullName.Contains("Recycle"))
                            {
                                FilesFound.AddRange(RecursiveFileSpider(Dir.FullName, SearchString).ToList());
                                //	Console.WriteLine(Dir.FullName);
                            }
                        }

                        catch (Exception Ex)
                        {
                            //return;
                        }
                    }
				}
				else
				{
					try
					{
						var files = dirPrograms.EnumerateFiles(SearchString).Where(x => x != null && x.Name != null && x.Name != "" && !x.Extension.Equals("") && x.FullName.Length < 255);
						object aLock = new object();
                        foreach (var aFile in files)
                        {
                            lock (aLock)
                            {
                                FileType type;
                                if (aFile.Extension.Equals(".exe"))
                                    type = FileType.app;
                                else
                                    type = FileType.file;
                                if (WhiteListedExtensions.FindIndex(x => x.Type.Equals(aFile.Extension)) > 0 && WhiteListedExtensions[WhiteListedExtensions.FindIndex(x => x.Type.Equals(aFile.Extension))].IsChecked)
                                {
                                    FilesFound.Add(new FileItem { FileLocation = aFile.FullName, FileName = aFile.Name, ExtensionID =ExtensionConverter.ConvertFromString(aFile.Extension), LastUsed = DateTime.MinValue, TotalUsed = 0, Type = type });
                                }
                            }
                        }
					}
					catch (Exception Ex)
					{
						return new List<FileItem>() { };
					}
				}
			}
			catch (Exception Ex)
			{
				string thing = "";
			}
			return FilesFound;
		}
	}
}

