using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using PDM.Src.Enums;
using PDM.Src.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDM.Src.Models
{
    internal class DatabaseManager
    {
        private LiteDatabase? _db;
        public string? DatabasePath { get; private set; }
        public bool IsOpen => _db != null;
        // Collection Access (e.g., Phones)
        public ILiteCollection<Phone>? Phones => _db?.GetCollection<Phone>("phones");

        /// <summary>
        /// Create a new LiteDB database at the given path
        /// </summary>
        public void CreateNew(string path)
        {
            if (File.Exists(path)) throw new InvalidOperationException("File already exists");
            _db?.Dispose();
            _db = new LiteDatabase(path);
            DatabasePath = path;

            // Ensure initial structure
            Phones?.EnsureIndex(x => x.Id, unique:true);
        }

        /// <summary>
        /// Open an existing LiteDB Structure
        /// </summary>
        public void Open(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("Database file not found.", path);
            _db?.Dispose();
            _db = new LiteDatabase(path);
            DatabasePath = path;
            App.ServiceProvider.GetRequiredService<PhoneListViewModel>().ReloadPhones();
        }

        public LiteDatabase GetDatabase()
        {
            return _db;
        }

        public void SavePhone(Phone phone)
        {
            if (_db != null)
            {
                var collection = _db.GetCollection<Phone>("phones");
                if (phone.Id == 0)
                {
                    var last = collection.Query().OrderByDescending(p => p.Id).FirstOrDefault();
                    phone.Id = last != null ? last.Id + 1 : 0;
                }
                collection.Insert(phone);
            }
            else throw new InvalidOperationException("The database is null");
        }

        public void UpdatePhone(Phone phone)
        {
            if (_db != null)
            {
                var col = _db.GetCollection<Phone>("phones");
                col.Update(phone);
            }
            else throw new InvalidOperationException("The Database is Null");
            
        }

        public int GetNextPhoneId()
        {
            if (_db != null)
            {
                var col = _db.GetCollection<Phone>("phones");
                var lastPhone = col.Query().OrderByDescending(x => x.Id).FirstOrDefault();
                return lastPhone != null ? lastPhone.Id + 1 : 0;
            }
            else
            {
                return -1;
            }

        }

        /// <summary>
        /// Closes the database connection
        /// </summary>
        public void Close()
        {
            _db?.Dispose();
            _db = null;
            DatabasePath = null;
        }

        public void Dispose()
        {
            Close();
        }

    }
}
