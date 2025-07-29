using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDM.Src.Models
{
    internal class PhoneOS
    {
        public string OSName { get; set; }

        public override string ToString()
        {
            return OSName;
        }
    }
}
