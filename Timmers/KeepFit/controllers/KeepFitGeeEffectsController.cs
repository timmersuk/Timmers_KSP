using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbar;
using UnityEngine;

namespace KeepFit
{
    // <summary>
    /// Controller for handling checking of Gee effects and impacting crew
    /// </summary>
    public class KeepFitGeeEffectsController : KeepFitController
    {
        private double lastGeeLoadingUpdateUT = -1;


        internal void Awake()
        {
            this.Log_DebugOnly("Awake", ".");
        }

        public void FixedUpdate()
        {
            this.Log_DebugOnly("FixedUpdate", ".");

            if (gameConfig == null)
            {
                this.Log_DebugOnly("FixedUpdate", "No gameConfig - bailing");
                return;
            }

            if (!HighLogic.LoadedSceneIsFlight)
            {
                this.Log_DebugOnly("FixedUpdate", "Not in flight scene - bailing");
                return;
            }

            if (lastGeeLoadingUpdateUT == -1)
            {
                this.Log_DebugOnly("FixedUpdate", "No lastGeeLoadingUpdateUT - skipping this update");

                // don't do anything this time
                this.lastGeeLoadingUpdateUT = Planetarium.GetUniversalTime();
                return;
            }

            double currentUT = Planetarium.GetUniversalTime();
            float elapsedSeconds = (float)(currentUT - lastGeeLoadingUpdateUT);
            lastGeeLoadingUpdateUT = currentUT;

            this.Log_DebugOnly("FixedUpdate", "[{0}] seconds since last fixed update", elapsedSeconds);

            // just check gee loading on active vessel for now
            Vessel vessel = FlightGlobals.ActiveVessel;
            if (vessel == null)
            {
                this.Log_DebugOnly("FixedUpdate", "No active vessel");
                return;
            }

            this.Log_DebugOnly("FixedUpdate", "Checking gee loading for active vessel[{0}]", vessel.GetName());

            float geeLoading;
            string invalidReason;
            bool valid = GeeLoadingCalculator.GetGeeLoading(vessel, out geeLoading, out invalidReason);
            if (!valid)
            {
                this.Log_DebugOnly("FixedUpdate", "Gee loading for active vessel[{0}] is not valid currently because[{1}]", vessel.GetName(), invalidReason);
            }
            else
            {
                this.Log_DebugOnly("FixedUpdate", "Gee loading for active vessel[{0}] is[{1}] letting the crew know", vessel.GetName(), geeLoading);
                foreach (ProtoCrewMember crewMember in vessel.GetVesselCrew())
                {
                    try
                    {
                        this.Log_DebugOnly("FixedUpdate", "Gee loading for active vessel[{0}] is[{1}] letting [{2}] know", vessel.GetName(), geeLoading, crewMember.name);

                        KeepFitCrewMember keepFitCrewMember = gameConfig.knownCrew[crewMember.name];

                        handleGeeLoadingUpdates(keepFitCrewMember, geeLoading, elapsedSeconds);
                    }
                    catch (KeyNotFoundException)
                    {
                        // ignore, we'll pick them up later
                        this.Log_Release("FixedUpdate", "Gee loading for active vessel[{0}] is[{1}] crewmember [{2}] not in the keepfit roster yet", vessel.GetName(), geeLoading, crewMember.name);
                    }
                }
            }
        }

        private void handleGeeLoadingUpdates(KeepFitCrewMember crew, 
                                            float gee, 
                                            float duration)
        {
            // modify the 'experienced' gee based on the crew member's fitness relative to the start state
            float healthGeeToleranceModifier = crew.fitnessLevel / gameConfig.initialFitnessLevel;
            
            foreach (Period period in Enum.GetValues(typeof(Period)))
            {
                GeeToleranceConfig tolerance;
                GeeLoadingAccumulator accum;
                crew.geeAccums.TryGetValue(period, out accum);
                gameConfig.geeTolerances.TryGetValue(period, out tolerance);

                if (tolerance != null && accum != null)
                {
                    handleGeeLoadingUpdate(crew, gee, duration, accum, tolerance, healthGeeToleranceModifier);
                }
            }       
        }

        private void handleGeeLoadingUpdate(KeepFitCrewMember crewMember, 
                                            float geeLoading, 
                                            float elapsedSeconds, 
                                            GeeLoadingAccumulator accum, 
                                            GeeToleranceConfig tolerance,
                                            float healthGeeToleranceModifier)
        {
            
            float meanG;
            if (accum.AccumulateGeeLoading(geeLoading, elapsedSeconds, out meanG))
            {
                float fatal = tolerance.fatal * healthGeeToleranceModifier;
                float warn = tolerance.warn * healthGeeToleranceModifier;

                if (meanG > fatal) 
                {
                    onGeeFatal(crewMember, meanG, warn, fatal);
                }
                else if (meanG > warn)
                {
                    onGeeWarn(crewMember, meanG, warn, fatal);
                }
            }
        }

        private void onGeeWarn(KeepFitCrewMember crewMember, float meanG, float warn, float fatal)
        {
            string formatted = string.Format("KeepFit - Crewman {0} is reaching his G limits! ({1:000.00} - warn{2:000.00}, fatal{3:000.00}", crewMember.Name, meanG, warn, fatal);

            ScreenMessages.PostScreenMessage(formatted, 3f, ScreenMessageStyle.UPPER_CENTER);
        }

        private void onGeeFatal(KeepFitCrewMember crewMember, float meanG, float warn, float fatal)
        {
            if (gameConfig.wimpMode)
            {
                string formatted = string.Format("KeepFit - Crewman {0} suffered momentary G-LOC! ({1:000.00} - warn{2:D3}, fatal{3:000.00}", crewMember.Name, meanG, warn, fatal);

                ScreenMessages.PostScreenMessage(formatted, 3f, ScreenMessageStyle.UPPER_CENTER);
            }
            else
            {
                KerbalKiller.KillKerbal(this, crewMember);
            }
        }
    }
}
