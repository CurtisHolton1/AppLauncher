using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SQLite;
using AppLauncher.Models;


namespace FileSearchTest.Service
{
	class FileSearch
	{

		/// <summary>
		/// Create sa SQLite database of all files on the specified drive. It will check if a version already exists. If OverWrite is false
		/// it will return the existing DB. If true it will delete it and make a new DB based on the specified Root.
		/// </summary>
		/// <param name="DatabaseLocation">Where to save the database. No trailing "\"</param>
		/// <param name="DatabaseName">What to name the database</param>
		/// <param name="OverWrite">Determines if an existing DB should be overwritten</param>
		/// <param name="Root">where to start the search</param>
		/// <returns>whether the DB created or already exists.</returns>
		public static bool CreateFilesDatabase(string DatabaseLocation, string DatabaseName, bool OverWrite = false, string Root = "c:\\")
		{
			try
			{


				//Check to see if the databae exists
				if (File.Exists(DatabaseName) && OverWrite == false)
				{
					//no need to create a new one
					return true;
				}
				//Create/erase old DB
				if (!Directory.Exists(DatabaseName))
				{
					Directory.CreateDirectory(DatabaseLocation);
				}
				SQLiteConnection.CreateFile(DatabaseName);
				var SqlConnection = new SQLiteConnection("Data Source=" + DatabaseLocation + "\\" + DatabaseName + "; Version=3");
				SqlConnection.Open();
				string sql = "CREATE TABLE FilesFound(FileName varchar(280), FileLocation varchar(280))";
				SQLiteCommand cmd = new SQLiteCommand(sql, SqlConnection);
				cmd.ExecuteNonQuery();
				//Get all files in root
				var FilesFound = FileSpider(Root, "*").ToList();

				//Insert into SQLite
				using (var insertCmd = new SQLiteCommand(SqlConnection))
				{
					using (var transaction = SqlConnection.BeginTransaction())
					{
						try
						{
							foreach (var aFile in FilesFound)
							{
								try
								{
									insertCmd.CommandText = "INSERT INTO FilesFound(FileName,FileLocation) VALUES ('" + aFile.FileName + "','" + aFile.FileLocation + "')";
									insertCmd.ExecuteNonQuery();
								}
								catch (Exception Ex)
								{
									
								}
							}
							//Save
							transaction.Commit();
						}
						catch (Exception e)
						{
							transaction.Rollback();
							throw e;
						}
					}
				}
			}
			catch (Exception ex)
			{
				return false;
			}
			return true;
		}

		public static ILookup<string, string> GetFilesFromDB(string DatabaseLocation = "C:\\Test\\MyDatabase.sqlite")
		{
			try
			{


				var SqlConnection = new SQLiteConnection("Data Source=" + DatabaseLocation + "; Version=3");
				SqlConnection.Open();
				string sql = "SELECT * FROM FilesFound";
				SQLiteCommand cmd = new SQLiteCommand(sql, SqlConnection);
				cmd = new SQLiteCommand(sql, SqlConnection);
				cmd.ExecuteNonQuery();
				SQLiteDataReader reader = cmd.ExecuteReader();
				var FilesFound = new List<FileItem>() { };
				while (reader.Read())
					FilesFound.Add(new FileItem { FileLocation = reader["FileLocation"].ToString(), FileName = reader["FileName"].ToString() });

				SqlConnection.Close();
				return FilesFound.ToLookup(x => x.FileName, x => x.FileLocation);
			}
			catch (Exception Ex)
			{
				return null;
			}
		}

		/// <summary>
		/// Returns all files starting at the provided root and working its way down all sub folders
		/// </summary>
		/// <param name="Root">Root path to begin search</param>
		/// <param name="SearchString">String to search for in file names. Insert "*" to find all</param>
		/// <returns></returns>
		private static List<FileItem> FileSpider(string Root, String SearchString)
		{
			try
			{
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
			var Dirs = dirPrograms.EnumerateDirectories();
			try
			{
				if (Dirs.Count() > 0)
				{
					Parallel.ForEach(Dirs, Dir =>
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
							return;
						}
					});
				}
				else
				{
					try
					{
						var files = dirPrograms.EnumerateFiles(SearchString).Where(x => x != null && x.Name != null && x.Name != "");
						object aLock = new object();
						Parallel.ForEach(files, aFile =>
						{
							lock (aLock)
							{
								FilesFound.Add(new FileItem { FileLocation = aFile.FullName, FileName = aFile.Name });
							}
						});

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

