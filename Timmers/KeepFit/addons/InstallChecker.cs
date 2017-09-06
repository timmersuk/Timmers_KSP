using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KeepFit
{
    /// <summary>
    /// Majiir's compatibility checker (http://forum.kerbalspaceprogram.com/threads/65395-Voluntarily-Locking-Plugins-to-a-Particular-KSP-Version?p=899895&viewfull=1#post899895)
    /// </summary>
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    internal class InstallChecker : MonoBehaviour
    {
        protected void Start()
        {
            var assemblies = AssemblyLoader.loadedAssemblies.Where(
                a => a.assembly.GetName().Name == System.Reflection.Assembly.GetExecutingAssembly().GetName().Name).Where(a => a.url != "KeepFit");

            if (assemblies.Any())
            {
                Uri kspApplicationRootPathUri = new Uri(Path.GetFullPath(KSPUtil.ApplicationRootPath));
                var badPaths = assemblies.Select(a => a.path).Select(p => Uri.UnescapeDataString(kspApplicationRootPathUri.MakeRelativeUri(new Uri(p)).ToString().Replace('/', Path.DirectorySeparatorChar)));
                PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "KeepFitInstallChecker",
                    "Incorrect KeepFit Installation",
                    "KeepFit has been installed incorrectly and will not function properly. " +
                    "All KeepFit files should be located in KSP/GameData/KeepFit. " +
                    "Do not move any files from inside the KeepFit folder.\n\nIncorrect path(s):\n" + String.Join("\n", badPaths.ToArray()),
                    "OK",
                    false,
                    HighLogic.UISkin);
            }
        }
    }


}
