using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeWorld.Tracker.App.DAL.Model;
using SQLite.Net;
using SQLite.Net.Interop;
using SQLite.Net.Platform.WinRT;

namespace HomeWorld.Tracker.App.DAL
{
    public class DataService
    {
        DL.TrackerDb _db = null;
        protected static string DbLocation;
        protected static DataService Data;

        static DataService()
        {
            Data = new DataService();
        }

        protected DataService()
        {
            // set the _db location
            DbLocation = DatabaseFilePath;

            // instantiate the database	
            _db = new DL.TrackerDb(DbLocation);
        }

        public static string DatabaseFilePath
        {
            get
            {
                var sqliteFilename = "TrackerDB.db3";

#if NETFX_CORE
                var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, sqliteFilename);
#else

#if SILVERLIGHT
				// Windows Phone expects a local path, not absolute
				var path = sqliteFilename;
#else

#if __ANDROID__
				// Just use whatever directory SpecialFolder.Personal returns
				string libraryPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); ;
#else
				// we need to put in /Library/ on iOS5.1 to meet Apple's iCloud terms
				// (they don't want non-user-generated Data in Documents)
				string documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal); // Documents folder
				string libraryPath = Path.Combine (documentsPath, "../Library/"); // Library folder
#endif
				var path = Path.Combine (libraryPath, sqliteFilename);
#endif		

#endif
                return path;
            }
        }

        #region Transactions

        public static void BeginTransaction()
        {
            Data._db.BeginTransaction();
        }

        public static void CommitTransaction()
        {
            if (Data._db.IsInTransaction) Data._db.Commit();
        }

        public static void RollbackTransaction()
        {
            if (Data._db.IsInTransaction) Data._db.Rollback();
        }

        #endregion

        #region Movement

        public static int AddMovement(Movement movement)
        {
            return Data._db.SaveItem<Movement>(movement);
        }

        public static IEnumerable<Movement> GetMovements()
        {
            return Data._db.GetItems<Movement>();
        }

        public static int DeleteMovement(int id)
        {
            return Data._db.DeleteItem<Movement>(id);
        }

        #endregion

        #region People

        public static int AddUpdatePerson(Person person)
        {
            return Data._db.InsertOrReplace(person);
        }

        public static Person GetPersonByCardId(string cardUid)
        {
            return
                Data._db.GetItems<Person>()
                    .FirstOrDefault(p => string.Equals(p.CardUid, cardUid, StringComparison.OrdinalIgnoreCase));
        }

        #endregion
    }
}



namespace HomeWorld.Tracker.App.DL
{
    public class TrackerDb : SQLiteConnection
    {
        private static object _locker = new object();


        public TrackerDb(string databasePath)
            : base(new SQLitePlatformWinRT(), databasePath)
        {
            CreateTable<Movement>();
            CreateTable<Person>();
        }
        public IEnumerable<T> GetItems<T>() where T : EntityBase, new()
        {
            lock (_locker)
            {
                return (from i in Table<T>() select i).ToList();
            }
        }

        public T GetItem<T>(int id) where T : EntityBase, new()
        {
            lock (_locker)
            {
                return Table<T>().FirstOrDefault(x => x.Id == id);
                // Following throws NotSupportedException - thanks aliegeni
                //return (from i in Table<T> ()
                //        where i.ID == id
                //        select i).FirstOrDefault ();
            }
        }

        public int SaveItem<T>(T item) where T : EntityBase, new()
        {
            lock (_locker)
            {
                if (item.Id != 0)
                {
                    Update(item);
                    return item.Id;
                }
                else
                {
                    return Insert(item);
                }
            }
        }

        public int DeleteItem<T>(int id) where T : EntityBase, new()
        {
            lock (_locker)
            {
#if NETFX_CORE
                return Delete(new T() { Id = id });
#else
				return Delete<T> (new T () { ID = id });
#endif
            }
        }

        public void ClearTable<T>() where T : EntityBase, new()
        {
            lock (_locker)
            {
                Execute(string.Format("delete from \"{0}\"", typeof(T).Name));
            }
        }

        // helper for checking if database has been populated
        //		public int CountTable<T>() where T : EntityBase, new ()
        //		{
        //			lock (locker) {
        //				string sql = string.Format ("select count (*) from \"{0}\"", typeof (T).Name);
        //
        //
        //				var c = db.CreateCommand (sql, new object[0]);
        //				return ExecuteScalar<int>();
        //			}
        //		}
    }
}
