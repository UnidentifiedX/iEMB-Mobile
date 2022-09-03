using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace iEMB.Models
{
    // https://docs.microsoft.com/en-us/xamarin/xamarin-forms/data-cloud/data/databases

    internal class DatabaseConstants
    {
        public const string DatabaseName = "AnnouncementSQLite.db3";

        public const SQLiteOpenFlags Flags =
            // open the database in read/write mode
            SQLiteOpenFlags.ReadWrite |
            // create the database if it doesn't exist
            SQLiteOpenFlags.Create |
            // enable multi-threaded database access
            SQLiteOpenFlags.SharedCache;

        public static string DatabasePath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, DatabaseName);
            }
        }
    }
}
