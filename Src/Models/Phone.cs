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
        public string Brand { get; set; }
        public string Model { get; set; }
        public string IMEI { get; set; }
        public PhoneStatus Status { get; set; }
        public byte[]? ImageDataF { get; set; }
        public byte[]? ImageDataB { get; set; }

        // Specs

        public string ManufacturerId { get; set; }
        public string OS { get; set; }
        public string Version { get; set; }
        public string Color { get; set; }
        public DeviceType DeviceType { get; set; }
        public PhoneCondition Condition { get; set; }
        public PhoneState PhoneState { get; set; }
        public PasscodeType PasscodeType { get; set; }
        public int PasscodeLength { get; set; }
        public string Notes { get; set; }

        [LiteDB.BsonIgnore]
        private ImageSource? _cachedImagePreviewFront;

        [LiteDB.BsonIgnore]
        private ImageSource? _cachedImagePreviewBack;

        [LiteDB.BsonIgnore]
        public ImageSource? ImagePreviewFront
        {
            get
            {
                if (_cachedImagePreviewFront != null)
                    return _cachedImagePreviewFront;

                if (ImageDataF == null || ImageDataF.Length == 0)
                    return null;

                using var ms = new MemoryStream(ImageDataF);
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.DecodePixelWidth = 250; // Limit decode size to match display
                image.DecodePixelHeight = 100;
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();
                
                _cachedImagePreviewFront = image;
                return _cachedImagePreviewFront;
            }
        }

        [LiteDB.BsonIgnore]
        public ImageSource? ImagePreviewBack
        {
            get
            {
                if (_cachedImagePreviewBack != null)
                    return _cachedImagePreviewBack;

                if (ImageDataB == null || ImageDataB.Length == 0)
                    return null;

                using var ms = new MemoryStream(ImageDataB);
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.DecodePixelWidth = 250; // Limit decode size to match display
                image.DecodePixelHeight = 100;
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();
                
                _cachedImagePreviewBack = image;
                return _cachedImagePreviewBack;
            }
        }

        // Add method to clear cache when image data changes
        public void ClearImageCache()
        {
            _cachedImagePreviewFront = null;
            _cachedImagePreviewBack = null;
        }

        // Add this method to clear cache when image data changes
        public void SetImageDataF(byte[]? data)
        {
            ImageDataF = data;
            ClearImageCache();
        }

        public void SetImageDataB(byte[]? data)
        {
            ImageDataB = data;
            ClearImageCache();
        }
    }
}
