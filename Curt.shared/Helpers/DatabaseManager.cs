using Curt.shared.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Curt.shared
{
    public static class DatabaseManager
    {

        private static List<FileItem> CurrentSearch = new List<FileItem>();
        private static string DBLocation;
        public static void SetDBLocation(string location)
        {
            DBLocation = location;
        }
        public static bool CreateDB()
        {
            try
            {
                SQLiteConnection.CreateFile(DBLocation);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        #region FilesFoundTable
        /// <summary>
        /// Create sa SQLite database of all files on the specified drive. It will check if a version already exists. If OverWrite is false
        /// it will return the existing DB. If true it will delete it and make a new DB based on the specified Root.
        /// </summary>
        /// <param name="DatabaseLocation">Where to save the database. No trailing "\"</param>
        /// <param name="DatabaseName">What to name the database</param>
        /// <param name="OverWrite">Determines if an existing DB should be overwritten</param>
        /// <param name="Root">where to start the search</param>
        /// <returns>whether the DB created or already exists.</returns>
        public async static Task<bool> CreateFilesTable(System.IProgress<double> progress)
        {
            try
            {
                var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
                SqlConnection.Open();
                string sql = "CREATE TABLE FilesFound(ID INTEGER PRIMARY KEY, FileName varchar(280),  ExtensionID INTEGER, Type INTEGER, DisplayName varchar(280), LastUsed text, TotalUsed INTEGER, FileLocation varchar(280), FOREIGN KEY(ExtensionID) REFERENCES WhiteList(ID))";
                SQLiteCommand cmd = new SQLiteCommand(sql, SqlConnection);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return false;
            }
            if (progress != null)
            {
                progress.Report(1);
            }
            return true;
        }

        public async static Task<bool> IndexFilesTable()
        {
            try
            {
                var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
                SqlConnection.Open();
                var sql = "CREATE INDEX idx on FilesFound (FileName)";
                SQLiteCommand cmd = new SQLiteCommand(sql, SqlConnection);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        public static async Task<bool> InsertIntoFilesTable(List<FileItem> toWrite)
        {
            try {
                var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
                SqlConnection.Open();
                using (var insertCmd = new SQLiteCommand(SqlConnection))
                {
                    using (var transaction = SqlConnection.BeginTransaction())
                    {
                        try
                        {
                            foreach (var aFile in toWrite)
                            {
                                try
                                {
                                    insertCmd.CommandText = "INSERT INTO FilesFound(FileName,FileLocation,ExtensionId,DisplayName,Type,LastUsed,TotalUsed) VALUES ('" + aFile.FileName + "','" + aFile.FileLocation + "','" + aFile.ExtensionID + "','" + aFile.DisplayName + "','" + (int)aFile.Type + "','" + aFile.LastUsed + "','" + aFile.TotalUsed + "')";
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
                await IndexFilesTable();
                SqlConnection.Close();
                return true;
            }
            catch
            {
                return false;
            }

        }

        public static async Task<List<FileItem>> SelectFromFilesTable(string text, int type)
        {
            var FilesFound = new List<FileItem>();
            if (text.Length == 1)
            {
                var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
                SqlConnection.Open();
                string sql = "Select FilesFound.ID,FileName,ExtensionID,Type,DisplayName,LastUsed,TotalUsed,FileLocation FROM FilesFound INNER JOIN WhiteList on WhiteList.ID = FilesFound.ExtensionID AND WhiteList.Active = 1 AND  Type = " + type + " WHERE FilesFound.FileName LIKE '" + text + "%'";
                SQLiteCommand cmd = new SQLiteCommand(sql, SqlConnection);
                cmd = new SQLiteCommand(sql, SqlConnection);
                cmd.ExecuteNonQuery();
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    FilesFound.Add(new FileItem { ID = Convert.ToInt32(reader["ID"].ToString()), FileName = reader["FileName"].ToString(), ExtensionID = Convert.ToInt32(reader["ExtensionID"].ToString()), Type = (FileType)Convert.ToInt32(reader["Type"].ToString()), DisplayName = reader["DisplayName"].ToString(), LastUsed = Convert.ToDateTime(reader["LastUsed"].ToString()), TotalUsed = Convert.ToInt32(reader["TotalUsed"].ToString()), FileLocation = reader["FileLocation"].ToString() });
                }
                SqlConnection.Close();
                CurrentSearch = FilesFound;
            }
            else
            {
                FilesFound = CurrentSearch;
                return FilesFound.Where(x => x.FileName.ToLower().StartsWith(text.ToLower()) && x.Type == (FileType)type).OrderByDescending(x=>x.TotalUsed).Take(15).ToList();             
            }
            return FilesFound;
        }

        public static List<int> SelectID(string text, int type)
        {
            var ids = new List<int>();
            if (text.Length == 1)
            {
                var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
                SqlConnection.Open();
                string sql = "Select FilesFound.ID FROM FilesFound INNER JOIN WhiteList on WhiteList.ID = FilesFound.ExtensionID AND WhiteList.Active = 1 AND  Type >= " + type + " WHERE FilesFound.FileName LIKE '" + text + "%'";
                SQLiteCommand cmd = new SQLiteCommand(sql, SqlConnection);
                cmd = new SQLiteCommand(sql, SqlConnection);
                cmd.ExecuteNonQuery();
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ids.Add(Convert.ToInt32(reader["ID"].ToString()));
                }
                SqlConnection.Close();
          
            }
            else
            {
               // FilesFound = CurrentSearch;
               // return FilesFound.Where(x => x.FileName.ToLower().StartsWith(text.ToLower()) && x.Type >= (FileType)type).ToList();
            }
            return ids;
        }

        public static async Task<List<FileItem>> SelectFromFilesTableGreaterEqualType(string text, int type)
        {
            var FilesFound = new List<FileItem>();
            if (text.Length == 1)
            {
                var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
                SqlConnection.Open();
                string sql = "Select FilesFound.ID,FileName,ExtensionID,Type,DisplayName,LastUsed,TotalUsed,FileLocation FROM FilesFound INNER JOIN WhiteList on WhiteList.ID = FilesFound.ExtensionID AND WhiteList.Active = 1 AND  Type >= " + type + " WHERE FilesFound.FileName LIKE '" + text + "%'";              
                SQLiteCommand cmd = new SQLiteCommand(sql, SqlConnection);
                cmd = new SQLiteCommand(sql, SqlConnection);
                cmd.ExecuteNonQuery();
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {                  
                    FilesFound.Add(new FileItem { ID = Convert.ToInt32(reader["ID"].ToString()), FileName = reader["FileName"].ToString(), ExtensionID = Convert.ToInt32(reader["ExtensionID"].ToString()), Type = (FileType)Convert.ToInt32(reader["Type"].ToString()), DisplayName = reader["DisplayName"].ToString(), LastUsed = Convert.ToDateTime(reader["LastUsed"].ToString()), TotalUsed = Convert.ToInt32(reader["TotalUsed"].ToString()),  FileLocation = reader["FileLocation"].ToString() });
                }
                SqlConnection.Close();
                CurrentSearch = FilesFound;
            }
            else
            {
                FilesFound = CurrentSearch;
                 return FilesFound.Where(x => x.FileName.ToLower().StartsWith(text.ToLower()) && x.Type >= (FileType)type).OrderByDescending(x => x.TotalUsed).Take(15).ToList();
               
           }
            return FilesFound;
        }

        public static async Task<bool> UpdateFilesTable(DropDownItem item)
        {
            try
            {
                var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
                SqlConnection.Open();
                string sql = "UPDATE FilesFound SET TotalUsed = " + item.TotalTimesUsed + ", LastUsed = \"" + item.LastUsed + "\" WHERE ID = " + item.ID;
                SQLiteCommand cmd = new SQLiteCommand(sql, SqlConnection);
                cmd = new SQLiteCommand(sql, SqlConnection);
                cmd.ExecuteNonQuery();
                SqlConnection.Close();
                return true;
            }
            catch (Exception e)
            {
                return false;
                throw e;
            }
        }

        public static bool InsertIntoFilesTable(FileItem item)
        {
             try
            {

                var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
                SqlConnection.Open();
                var sql = "INSERT INTO FilesFound(FileName,ExtensionID,Type,DisplayName,LastUsed,TotalUsed,FileLocation) VALUES ('" + item.FileName + "','" + item.ExtensionID + "','" + (int)item.Type + "','" + item.DisplayName + "','" + item.LastUsed + "','" + item.TotalUsed + "','" + item.FileLocation + "')";
                SQLiteCommand cmd = new SQLiteCommand(sql, SqlConnection);
                cmd = new SQLiteCommand(sql, SqlConnection);
                cmd.ExecuteNonQuery();
                SqlConnection.Close();
                return true;
            }
            catch (Exception e)
            {
                return false;
                throw e;
            }
        }

        public static bool DeleteFromFilesTable(int ID)
        {
            try
            {
                var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
                SqlConnection.Open();
                var sql = "DELETE FROM FilesFound WHERE ID = " + ID;
                var cmd = new SQLiteCommand(sql, SqlConnection);
                cmd.ExecuteNonQuery();
                SqlConnection.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static FileItem FindFileFromID(int ID)
        {
            FileItem itemToReturn = new FileItem();
            var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
            SqlConnection.Open(); 
            var sql = "Select * FROM FilesFound WHERE ID = " + ID;
            var cmd = new SQLiteCommand(sql, SqlConnection);
            cmd.ExecuteNonQuery();
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                itemToReturn = new FileItem { ID = Convert.ToInt32(reader["ID"].ToString()), FileLocation = reader["FileLocation"].ToString(), FileName = reader["FileName"].ToString(), DisplayName = reader["DisplayName"].ToString(), ExtensionID = Convert.ToInt32(reader["ExtensionID"].ToString()), Type = (FileType)Convert.ToInt32(reader["Type"].ToString()), LastUsed = Convert.ToDateTime(reader["LastUsed"].ToString()), TotalUsed = Convert.ToInt32(reader["TotalUsed"].ToString()) };
            }
            SqlConnection.Close();
            return itemToReturn;
        }

        public static FileItem FindFileFromFileInfo(FileInfo item)
        {
            FileItem itemToReturn = new FileItem();
            var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
            SqlConnection.Open();
            var sql = "Select * FROM FilesFound WHERE FileName = \"" + item.Name + "\" AND FileLocation = \"" + item.FullName + "\"";
            var cmd = new SQLiteCommand(sql, SqlConnection);
            cmd.ExecuteNonQuery();
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                itemToReturn = new FileItem { ID = Convert.ToInt32(reader["ID"].ToString()), FileLocation = reader["FileLocation"].ToString(), FileName = reader["FileName"].ToString(), DisplayName = reader["DisplayName"].ToString(), ExtensionID = Convert.ToInt32(reader["ExtensionID"].ToString()), Type = (FileType)Convert.ToInt32(reader["Type"].ToString()), LastUsed = Convert.ToDateTime(reader["LastUsed"].ToString()), TotalUsed = Convert.ToInt32(reader["TotalUsed"].ToString()) };
            }
            SqlConnection.Close();
            return itemToReturn;
        }

        public static bool TrimFilesTable(Extension toDelete)
        {
            var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
            SqlConnection.Open();
            var sql = "Delete FROM FilesFound WHERE ExtensionID LIKE '%" + toDelete.ID + "'";
            var cmd = new SQLiteCommand(sql, SqlConnection);
            cmd.ExecuteNonQuery();
            SqlConnection.Close();
            return true;
        }
        #endregion

        #region WhiteListTable
        public static async Task<bool> WriteWhiteListTable(List<Extension> toWrite)
        {
            var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
            SqlConnection.Open();
            using (var insertCmd = new SQLiteCommand(SqlConnection))
            {
                using (var transaction = SqlConnection.BeginTransaction())
                {
                    try
                    {                       
                        foreach (var e in toWrite)
                        {
                            try
                            {
                                insertCmd.CommandText = "INSERT INTO WhiteList(Extension, Active) VALUES ('" + e.Type + "' , '" + Convert.ToInt32(e.IsChecked) + "' )";
                                insertCmd.ExecuteNonQuery();
                            }
                            catch (Exception Ex)
                            {

                            }
                        }
                        //Save
                        transaction.Commit();
                      await  IndexWhiteListTable();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        throw e;
                    }
                }
            }
            return true;
        }

        public static async Task<bool> CreateWhiteListTable(List<Extension> toWrite = null)
        {
            try
            {
                var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
                SqlConnection.Open();
                var sql = "CREATE TABLE WhiteList(ID INTEGER PRIMARY KEY, Extension varchar(10), Active int)";
                var cmd = new SQLiteCommand(sql, SqlConnection);
                cmd.ExecuteNonQuery();
                sql = "INSERT INTO WhiteList(Extension,Active) values('Folder',1)";
                cmd = new SQLiteCommand(sql, SqlConnection);
                cmd.ExecuteNonQuery();
                if (toWrite != null)
                {
                    await WriteWhiteListTable(toWrite);
                }
                return true;
            }
            catch { return false; }
        }

        public static List<Extension> GetAllFromWhiteList()
        {
            List<Extension> toReturn = new List<Extension>();
            try
            {
                var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
                SqlConnection.Open();
                var sql = "SELECT * FROM WhiteList ORDER BY Extension";
                var cmd = new SQLiteCommand(sql, SqlConnection);
                cmd.ExecuteNonQuery();
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    toReturn.Add(new Extension { ID = Convert.ToInt32(reader["ID"].ToString()), Type = reader["Extension"].ToString(), IsChecked = Convert.ToBoolean(Convert.ToInt32(reader["Active"].ToString())) });
                }
                SqlConnection.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return toReturn;
        }

        public static bool DeleteWhiteList()
        {
            try
            {
                var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
                SqlConnection.Open();
                var sql = "DELETE FROM WhiteList";
                var cmd = new SQLiteCommand(sql, SqlConnection);
                cmd.ExecuteNonQuery();
                SqlConnection.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool UpdateWhiteListValue(Extension e)
        {
            try
            {
                var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
                SqlConnection.Open();
                var sql = "UPDATE WhiteList SET Active = " + Convert.ToInt32(e.IsChecked) + " WHERE ID = " + e.ID;
                SQLiteCommand cmd = new SQLiteCommand(sql, SqlConnection);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public static bool UpdateWhiteListTable(List<Extension> toUpdate)
        {
            var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
            SqlConnection.Open();

            using (var insertCmd = new SQLiteCommand(SqlConnection))
            {
                using (var transaction = SqlConnection.BeginTransaction())
                {
                    try
                    {
                        foreach (var extension in toUpdate)
                        {
                            try
                            {
                               
                                insertCmd.CommandText = "UPDATE WhiteList SET Active = " + Convert.ToInt32(extension.IsChecked) + " WHERE ID = " + extension.ID;
                                insertCmd.ExecuteNonQuery();
                            }
                            catch (Exception Ex)
                            {

                            }
                        }
                        //Save
                        transaction.Commit();
                    }
                    catch (Exception exc)
                    {
                        transaction.Rollback();
                        throw exc;
                    }
                }
            }
            return true;
        }

        public async static Task<bool> IndexWhiteListTable()
        {
            try
            {
                var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
                SqlConnection.Open();
                var sql = "CREATE INDEX idx2 on WhiteLIst (ID)";
                SQLiteCommand cmd = new SQLiteCommand(sql, SqlConnection);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        #endregion

        #region CommandsTable
        public static async  Task<bool> CreateCommandsTable(List<Command> toWrite = null)
        {
            try
            {
               
                var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
                SqlConnection.Open();
                var sql = "Create table Commands(ID INTEGER PRIMARY KEY, Name varchar(30), Path varchar(30), TotalUsed int)";
                var cmd = new SQLiteCommand(sql, SqlConnection);
                cmd.ExecuteNonQuery();
       
                if (toWrite != null) 
                {
                    WriteCommandsTable(toWrite);
                }
                return true;
            }
            catch { return false; }
        }

        public static bool WriteCommandsTable(List<Command> toWrite)
        {
            try {
                DeleteCommandsTable();
                var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
                SqlConnection.Open();
                using (var insertCmd = new SQLiteCommand(SqlConnection))
                {
                    using (var transaction = SqlConnection.BeginTransaction())
                    {
                  
                        foreach (var c in toWrite)
                        {
                            insertCmd.CommandText = "INSERT INTO Commands(Name,Path,TotalUsed) VALUES ('" + c.Name + "' , '" + c.Path + "' , '" + c.TotalUsed + "')";
                            insertCmd.ExecuteNonQuery();
                        }
                        //Save
                        transaction.Commit();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool DeleteCommandsTable()
        {
        try
            {
                var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
                SqlConnection.Open();
                var sql = "DELETE FROM Commands";
                var cmd = new SQLiteCommand(sql, SqlConnection);
                cmd.ExecuteNonQuery();
                SqlConnection.Close();
                return true;
            }
            catch { return false; }
        }
    

        public static bool UpdateCommandsTableValue(Command c)
        {
            try
            {
                var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
                SqlConnection.Open();
                var sql = "UPDATE Commands SET Name = " + c.Name + ", Path = " + c.Path + ", TotalUsed = " + c.TotalUsed + " WHERE ID = " + c.ID;
                SQLiteCommand cmd = new SQLiteCommand(sql, SqlConnection);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public static List<Command> SelectFromCommandsTable(string text)
        {
            List<Command> toReturn = new List<Command>();   
                var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
                SqlConnection.Open();
                string sql = "Select * FROM Commands WHERE Name like '" + text + "%'";
                SQLiteCommand cmd = new SQLiteCommand(sql, SqlConnection);
                cmd = new SQLiteCommand(sql, SqlConnection);
                cmd.ExecuteNonQuery();
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                toReturn.Add(new Command { ID = Convert.ToInt32(reader["ID"].ToString()), Name = reader["Name"].ToString(), Path = reader["Path"].ToString(), TotalUsed = Convert.ToInt32(reader["TotalUsed"].ToString()) });
                }
                SqlConnection.Close();                        
                return toReturn;
        }

        public static List<Command> GetAllFromCommandsTable()
        {
            List<Command> toReturn = new List<Command>();
            var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
            SqlConnection.Open();
            string sql = "Select * FROM Commands";
            SQLiteCommand cmd = new SQLiteCommand(sql, SqlConnection);
            cmd = new SQLiteCommand(sql, SqlConnection);
            cmd.ExecuteNonQuery();
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                toReturn.Add(new Command { ID = Convert.ToInt32(reader["ID"].ToString()), Name = reader["Name"].ToString(), Path = reader["Path"].ToString(), TotalUsed = Convert.ToInt32(reader["TotalUsed"].ToString()) });
            }
            SqlConnection.Close();
            return toReturn;
        }

        public static bool UpdateCommand(DropDownItem item)
        {
            try
            {
                var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
                SqlConnection.Open();
                string sql = "UPDATE Commands SET TotalUsed = " + item.TotalTimesUsed+ ", Name = \"" + item.Content + "\", Path = \"" + item.Path +  "\" WHERE ID = " + item.ID;
                SQLiteCommand cmd = new SQLiteCommand(sql, SqlConnection);
                cmd = new SQLiteCommand(sql, SqlConnection);
                cmd.ExecuteNonQuery();
                SqlConnection.Close();
                return true;
            }
            catch (Exception e)
            {
                return false;
                throw e;
            }
        }
        #endregion

        //private static Lookup<string, FileItem> GetLookupFromDB(string text, int type)
        //{
        //    try
        //    {
        //        var SqlConnection = new SQLiteConnection("Data Source=" + DBLocation + "; Version=3");
        //        SqlConnection.Open();
        //       string sql = "SELECT * FROM FilesFound WHERE FileName LIKE '" + text + "%' AND Type = " + type;
        //        SQLiteCommand cmd = new SQLiteCommand(sql, SqlConnection);
        //        cmd = new SQLiteCommand(sql, SqlConnection);
        //        cmd.ExecuteNonQuery();
        //        SQLiteDataReader reader = cmd.ExecuteReader();
        //        var FilesFound = new List<FileItem>() { };
        //        while (reader.Read())
        //             FilesFound.Add(new FileItem { ID = Convert.ToInt32(reader["ID"].ToString()), FileLocation = reader["FileLocation"].ToString(), FileName = reader["FileName"].ToString(), DisplayName = reader["DisplayName"].ToString(), Extension = reader["Extension"].ToString(), Type = (FileType)Convert.ToInt32(reader["Type"].ToString()), LastUsed = Convert.ToDateTime(reader["LastUsed"].ToString()), TotalUsed = Convert.ToInt32(reader["TotalUsed"].ToString()) });

        //        SqlConnection.Close();
        //        var lookupTable = (Lookup<string, FileItem>)FilesFound.ToLookup(x => x.FileName);
        //        return lookupTable;
        //    }
        //    catch (Exception Ex)
        //    {
        //        System.Windows.MessageBox.Show("error in getfilesfromDB " + Ex.Message + "               databaselocation: " + DBLocation);
        //        return null;
        //    }
        //}



    }
}
