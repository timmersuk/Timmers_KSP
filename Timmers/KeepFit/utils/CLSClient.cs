using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace KeepFit
{
    class CLSClient
    {
        private static ConnectedLivingSpace.ICLSAddon _CLS = null;
        private static bool? _CLSAvailable = null;

        public static ConnectedLivingSpace.ICLSAddon GetCLS()
        {
			Type CLSAddonType = null;
			AssemblyLoader.loadedAssemblies.TypeOperation(t =>
			{
				if (t.FullName == "ConnectedLivingSpace.CLSAddon")
				{
					CLSAddonType = t;
				}
			});
            if (CLSAddonType != null)
            {
                object realCLSAddon = CLSAddonType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
                _CLS =   (ConnectedLivingSpace.ICLSAddon)realCLSAddon;
            }
            return _CLS;
        }

        public static bool CLSInstalled
        {
            get
            {
                if (_CLSAvailable == null)
                {
                    _CLSAvailable = GetCLS() != null;
                }
                return (bool)_CLSAvailable;
            }
        }
    }
}
