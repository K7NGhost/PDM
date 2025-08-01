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
            SeedDatabase(_db);
            // Ensure initial structure
            Phones?.EnsureIndex(x => x.Id, unique:true);
            App.ServiceProvider.GetRequiredService<PhoneListViewModel>().ReloadPhones();
            App.ServiceProvider.GetRequiredService<DashboardViewModel>().LoadData();
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
            App.ServiceProvider.GetRequiredService<DashboardViewModel>().LoadData();
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

        private void SeedDatabase(LiteDatabase db)
        {
            var brandCol = db.GetCollection<PhoneBrand>("brands");
            if (brandCol.Count() == 0)
            {
                var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "brands.csv");
                foreach (var line in File.ReadAllLines(csvPath).Skip(1))
                {
                    brandCol.Insert(new PhoneBrand { BrandName = line });
                }
            }

            var modelCol = db.GetCollection<PhoneModel>("models");
            if (modelCol.Count() == 0)
            {
                var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "models.csv");
                foreach (var line in File.ReadAllLines(csvPath).Skip(1))
                {
                    var parts = line.Split(',');
                    modelCol.Insert(new PhoneModel { Brand = parts[0], ModelName = parts[1] });
                }
            }

            var osCol = db.GetCollection<PhoneOS>("oses");
            if (osCol.Count() == 0)
            {
                var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "oses.csv");
                foreach (var line in File.ReadAllLines(csvPath).Skip(1))
                {
                    osCol.Insert(new PhoneOS { OSName = line });
                }
            }

            var mapCol = db.GetCollection<PhoneMapping>("mappings");
            if (mapCol.Count() == 0)
            {
                var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "mappings.csv");
                foreach (var line in File.ReadAllLines(csvPath).Skip(1))
                {
                    var parts = line.Split(',');
                    mapCol.Insert(new PhoneMapping { Brand = parts[0], Model = parts[1], ManufacturerId = parts[2], ReleaseYear = Int32.Parse(parts[3]) });
                }
            }
        }

        public void DeletePhone(int phoneId)
        {
            var db = GetDatabase();
            var col = db.GetCollection<Phone>("phones");
            col.Delete(phoneId);
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
