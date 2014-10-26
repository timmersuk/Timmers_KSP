using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeepFit
{
    class Statics
    {
        //This version of KeepFit is compatible with KSP 0.23 - for use in CompatabilityChecker
        public static int CompatibleMajorVersion { get { return 0; } }
        public static int CompatibleMinorVersion { get { return 25; } }

        public static string GetDllVersion(System.Type type)
        {
            System.Reflection.Assembly assembly = type.Assembly;
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }

        public static string GetDllVersion<T>(T t)
        {
            return GetDllVersion(t.GetType());
        }
    }
}
