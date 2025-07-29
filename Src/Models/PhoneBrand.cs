using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDM.Src.Models
{
    internal class PhoneBrand
    {
        public string BrandName { get; set; }

        public override string ToString()
        {
            return BrandName;
        }
    }
}
