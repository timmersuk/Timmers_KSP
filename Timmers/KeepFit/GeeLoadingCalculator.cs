using System;
using System.Collections.Generic;
using UnityEngine;

namespace KeepFit
{
    /// <summary>
    /// Checking for the effects of Vessel Gee on the occupants of said vessel
    /// Taken from DeadlyReentry 2 by NathanKell, originally built by ialdabaoth, based on r4m0n's Deadly Reentry
    /// </summary>
    public class GeeLoadingCalculator
    {
        public static float GetFitnessModifiedGeeTolerance(float tolerance, KeepFitCrewMember crew, GameConfig config)
        {
            float healthGeeToleranceModifier = crew.fitnessLevel / config.initialFitnessLevel;

            // tolerance goes down to 50% normal tolerance if you zero out on your fitness compared to baseline
            float result = (tolerance / 2) + tolerance * (healthGeeToleranceModifier / 2);

            return result;
        }

        public static bool GetGeeLoading(Vessel vessel, out float geeLoading, out string invalidReason)
        {
            geeLoading = 0;
            invalidReason = "none";

            if (vessel == null)
            {
                invalidReason = "no vessel";
                // no vessel
                return false;
            }

            if (!HighLogic.LoadedSceneIsFlight || !FlightGlobals.ready)
            {
                invalidReason = "not in flight or not ready";
                // don't check g forces if we aren't flying or aren't ready
                return false;
            }

            if (TimeWarp.WarpMode == TimeWarp.Modes.HIGH && TimeWarp.CurrentRate > 1)
            {
                invalidReason = "high warp";
                geeLoading = 0;
                return true; // don't check G-forces in high warp
            }
               
            float deltaTime = TimeWarp.fixedDeltaTime;
            if (deltaTime > 0.5 || deltaTime <= 0)
            {
                invalidReason = "high physics deltaTime[" + deltaTime + "]";
                geeLoading = 0;
                return true; // don't check G-forces in high physics warp
            }

            geeLoading = (float)vessel.geeForce_immediate;

            if (geeLoading > 40)
            {
                // G forces over 40 are probably a Kraken twitch unless they last multiple frames
                // ... do something with this knowledge?
                invalidReason = "geeForce_immediate["+ geeLoading + "] suspected kraken";
                return false;
            }

            return true;
        }
    }
}
