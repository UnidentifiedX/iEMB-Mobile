using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace iEMB.Models
{
    public class AnnouncementDatabase
    {
        static SQLiteAsyncConnection Database;

        public static readonly AsyncLazy<AnnouncementDatabase> Instance = new AsyncLazy<AnnouncementDatabase>(async () =>
        {
            var instance = new AnnouncementDatabase();
            CreateTableResult result = await Database.CreateTableAsync<Announcement>();
            return instance;
        });

        public AnnouncementDatabase()
        {
            Database = new SQLiteAsyncConnection(DatabaseConstants.DatabasePath, DatabaseConstants.Flags);
        }

        public Task<List<Announcement>> GetAnnouncementsAsync()
        {
            return Database.Table<Announcement>().ToListAsync();
        }

        public Task<Announcement> GetAnnouncementAsync(string id)
        {
            return Database.Table<Announcement>().Where(a => a.Pid == id).FirstOrDefaultAsync();
        }

        public Task<int> SaveAnnouncementAsync(Announcement announcement)
        {
            return Database.InsertAsync(announcement);
        }

        public Task<int> DeleteAnnouncementAsync(Announcement announcement)
        {
            return Database.DeleteAsync(announcement);
        }

        public Task<int> DeleteAnnouncementsAsync()
        {
            return Database.DeleteAllAsync<Announcement>();
        }
    }
}
