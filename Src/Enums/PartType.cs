using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDM.Src.Enums
{
    internal enum PartType
    {
        None = -1,
        Unknown = 0,
        Screen = 1,
        Battery = 2,
        Backing = 3,
        Internal = 4,
        ChargingPort = 5,
        Miscellaneous = 6,
        Screws = 7,
        Logicboard = 8,
        Homebutton = 9,
        Adhesive = 10,
        PhoneWithoutScreen = 11
    }
}
