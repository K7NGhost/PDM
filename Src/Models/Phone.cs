using LiteDB;
using PDM.Src.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PDM.Src.Models
{
    internal class Phone
    {
        [BsonId]
        public int Id { get; set; }

        public int GroupId { get; set;}

        // General Info
        public PhoneBrand Brand { get; set; }
        public PhoneModel Model { get; set; }
        public string IMEI { get; set; }
        public PhoneStatus Status { get; set; }
        public byte[]? ImageData { get; set; }

        // Specs
        public PhoneOS OS { get; set; }
        public string Storage { get; set; }
        public string Color { get; set; }
        public PhoneCondition Condition { get; set; }
        public PhoneState PhoneState { get; set; }
        public PasscodeType PasscodeType { get; set; }
        public int PasscodeLength { get; set; }
        public string Notes { get; set; }



        [LiteDB.BsonIgnore] // Prevents LiteDB from trying to store it
        public ImageSource? ImagePreview
        {
            get
            {
                if (ImageData == null || ImageData.Length == 0)
                    return null;

                using var ms = new MemoryStream(ImageData);
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze(); // thread-safe
                return image;
            }
        }

    }
}
