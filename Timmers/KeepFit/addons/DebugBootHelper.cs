using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KeepFit
{
    /// <summary>
    /// Debug only helper to chuck me straight into default save.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class DebugBootHelper : MonoBehaviour
    {
        public static bool first = true;

        [System.Diagnostics.Conditional("DEBUG")]
        public void Start()
        {
            if (first)
            {
                this.Log_DebugOnly("Start", "Starting in debug mode");

                first = false;
                HighLogic.SaveFolder = "default";
                var game = GamePersistence.LoadGame("persistent", HighLogic.SaveFolder, true, false);
                //if (game != null && game.flightState != null && game.compatible)
                //{
                //    FlightDriver.StartAndFocusVessel(game, 6);
                //}
                CheatOptions.InfiniteFuel = true;
            }
        }
    }

}
