using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDM.Src.Enums
{
    internal enum PasscodeType
    {
        // No passcode set
        None,
        // Numeric PIN (e.g., 4 or 6 digits)
        PIN,
        // Alphanumberic password
        Password,
        // Android-style unlock pattern
        Pattern,
        // Swipe-to-unlock
        Swipe,
        Unknown
    }
}
