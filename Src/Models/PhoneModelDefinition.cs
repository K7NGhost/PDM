using LiteDB;
using PDM.Src.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDM.Src.Models
{
    class PhoneModelDefinition
    {
        [BsonId]
        public int GroupId { get; set; }

        public PhoneBrand Brand { get; set; }
        public string ModelName { get; set; }
    }
}
