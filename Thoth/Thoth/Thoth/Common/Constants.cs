using System;
using System.IO;

namespace Thoth.Common
{
    public static class Constants
    {
        public const string DatabaseFilename = "ThothSQLite.db3";

        public const SQLite.SQLiteOpenFlags Flags =
            // open the database in read/write mode
            SQLite.SQLiteOpenFlags.ReadWrite |
            // create the database if it doesn't exist
            SQLite.SQLiteOpenFlags.Create |
            // enable multi-threaded database access
            SQLite.SQLiteOpenFlags.SharedCache;

        public static string DatabasePath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, DatabaseFilename);
            }
        }

        public static string BaseFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.None);

        //Icons
        public static string DeleteIcon = "outline_delete_black_24.png";
        public static string DownloadIcon = "outline_cloud_download_black_24.png";
        public static string RefreshIcon = "baseline_refresh_black_24.png";
        public static string AddIcon = "baseline_add_black_24.png";
        public static string SaveIcon = "baseline_save_black_24.png";
        public static string CancelIcon = "outline_cancel_black_24.png";

    }
}