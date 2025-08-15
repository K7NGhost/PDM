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
    class Part
    {
        [BsonId]
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public PartType PartType { get; set; }
        public string Notes { get; set; }
        public byte[]? ImageData { get; set; }

        [LiteDB.BsonIgnore]
        public ImageSource? ImagePreview
        {
            get
            {
                if (ImageData == null || ImageData.Length == 0)
                {
                    return null;
                }
                using var ms = new MemoryStream(ImageData);
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();
                return image;
            }
        }

    }
}
