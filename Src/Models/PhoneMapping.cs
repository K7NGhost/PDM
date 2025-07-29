using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDM.Src.Models
{
    internal class PhoneMapping
    {
        public string ManufacturerId { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int ReleaseYear { get; set; }
        public byte[]? ImageData { get; set; }
    }
}
