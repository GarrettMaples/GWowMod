using System;
using System.Collections.Generic;
using System.Text;

namespace GWowMod.Requests
{
    public class AddonInstallException : Exception
    {
        public AddonInstallException(string message)
            : base(message)
        {
            
        }

        public AddonInstallException(string message, AddonInstallException innerException)
            : base(message, innerException)
        {
            
        }
    }
}
