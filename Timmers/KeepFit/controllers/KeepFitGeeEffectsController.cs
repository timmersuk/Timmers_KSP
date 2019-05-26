using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KeepFit
{
    // <summary>
    /// Controller for handling checking of Gee effects and impacting crew
    /// </summary>
    public class KeepFitGeeEffectsController : KeepFitController
    {
        private double lastGeeLoadingUpdateUT = -1;
		private List<float> gHistory = new List<float>();
        private float gStart = 0;
        internal Dictionary<Period, float> gLoads = new Dictionary<Period, float>();

        internal override void Init(KeepFitScenarioModule module)
		{
			base.Init(module);
		}		

        internal void Awake()
        {
            this.Log_DebugOnly("Awake", ".");
        }

        public void FixedUpdate()
        {
            // too spammy
            //this.Log_DebugOnly("FixedUpdate", ".");

            if (gameConfig == null)
            {
                // too spammy
                //this.Log_DebugOnly("FixedUpdate", "No gameConfig - bailing");
                return;
            }

            if (HighLogic.LoadedSceneIsEditor)
            {
                // too spammy
                this.Log_DebugOnly("FixedUpdate", "Not in flight scene - bailing");
                return;
            }

            // just check gee loading on active vessel for now
            Vessel vessel = FlightGlobals.ActiveVessel;
            if (vessel == null)
            {
                this.Log_DebugOnly("FixedUpdate", "No active vessel");
                return;
            }

            if (lastGeeLoadingUpdateUT == -1)
            {
                this.Log_DebugOnly("FixedUpdate", "No lastGeeLoadingUpdateUT - skipping this update");

                if (vessel.situation == Vessel.Situations.LANDED || vessel.situation == Vessel.Situations.SPLASHED || vessel.situation == Vessel.Situations.PRELAUNCH)
                    gStart = (float)vessel.mainBody.GeeASL;
                else
                    gStart = 0;

                gHistory = Enumerable.Repeat(gStart, 300).ToList();
                gLoads[Period.Inst] = gStart;
                gLoads[Period.Short] = gStart;
                gLoads[Period.Medium] = gStart;
                gLoads[Period.Long] = gStart;

                // don't do anything this time
                this.lastGeeLoadingUpdateUT = Planetarium.GetUniversalTime();
                return;
            }

            
            

            // too spammy
            //this.Log_DebugOnly("FixedUpdate", "[{0}] seconds since last fixed update", elapsedSeconds);

            // too spammy
            //this.Log_DebugOnly("FixedUpdate", "Checking gee loading for active vessel[{0}]", vessel.GetName());

            float geeLoading;
            string invalidReason;
            bool valid = GeeLoadingCalculator.GetGeeLoading(vessel, out geeLoading, out invalidReason);
            if (!valid)
            {
                this.Log_DebugOnly("FixedUpdate", "Gee loading for active vessel[{0}] is not valid currently because[{1}]", vessel.GetName(), invalidReason);
            }
            else
            {
                // too spammy
                //this.Log_DebugOnly("FixedUpdate", "Gee loading for active vessel[{0}] is[{1}] letting the crew know", vessel.GetName(), geeLoading);


                double currentUT = Planetarium.GetUniversalTime();

                float elapsedSeconds = (float)(currentUT - lastGeeLoadingUpdateUT);

                if (elapsedSeconds >= 1)
                {
                    lastGeeLoadingUpdateUT = currentUT;

                    gHistory.RemoveAt(299);
                    gHistory.Insert(0, geeLoading);

                    gLoads[Period.Inst] = geeLoading;
                    gLoads[Period.Short] = gHistory.GetRange(0,5).Average();
                    gLoads[Period.Medium] = gHistory.GetRange(0, 60).Average();
                    gLoads[Period.Long] = gHistory.Average();
                }

                

                foreach (ProtoCrewMember crewMember in vessel.GetVesselCrew())
                {
                    try
                    {
                        // too spammy
                        //this.Log_DebugOnly("FixedUpdate", "Gee loading for active vessel[{0}] is[{1}] letting [{2}] know", vessel.GetName(), geeLoading, crewMember.name);

                        KeepFitCrewMember keepFitCrewMember = gameConfig.roster.crew[crewMember.name];

                        handleGeeLoadingUpdates(keepFitCrewMember, gLoads);
                    }
                    catch (KeyNotFoundException)
                    {
                        // ignore, we'll pick them up later
                        this.Log_Release("KeepFitGeeEffectsController:FixedUpdate", "Gee loading for active vessel[{0}] is[{1}] crewmember [{2}] not in the keepfit roster yet", vessel.GetName(), geeLoading, crewMember.name);
                    }
                }
            }
        }

        private enum GeeLoadingOutCome
        {
            Ok, GeeWarn, GeeFatal
        }
        
        
        private void handleGeeLoadingUpdates(KeepFitCrewMember crew, 
                                            Dictionary<Period,float> gee)
        {
            // modify the 'experienced' gee based on the crew member's fitness relative to the start state
            float healthGeeToleranceModifier = crew.fitnessLevel / gameConfig.initialFitnessLevel;
            
            GeeLoadingOutCome harshestOutcome = GeeLoadingOutCome.Ok;
            foreach (Period period in Enum.GetValues(typeof(Period)))
            {
                GeeToleranceConfig tolerance;
                gameConfig.geeTolerances.TryGetValue(period, out tolerance);
                GeeLoadingAccumulator accum;
                crew.geeAccums.TryGetValue(period, out accum);

                if (tolerance != null && accum != null)
                {
	                GeeLoadingOutCome outcome = handleGeeLoadingUpdate(crew, gee[period], accum, tolerance, healthGeeToleranceModifier);
	                if (outcome > harshestOutcome)
	                {
	                    harshestOutcome = outcome;
	                }
                }
            }   
    
            switch (harshestOutcome)
            {
                case GeeLoadingOutCome.GeeFatal:
                    onGeeFatal(crew);
                    break;
                case GeeLoadingOutCome.GeeWarn:
                    onGeeWarn(crew);
                    break;
            }
        }

        private GeeLoadingOutCome handleGeeLoadingUpdate(KeepFitCrewMember crewMember,
                                            float geeLoading,
                                            GeeLoadingAccumulator accum,
                                            GeeToleranceConfig tolerance,
                                            float healthGeeToleranceModifier)
        {
            float meanG;
            if (accum.AccumulateGeeLoading(geeLoading, out meanG))
            {
                float geeWarn = GeeLoadingCalculator.GetFitnessModifiedGeeTolerance(tolerance.warn, crewMember, gameConfig);
                float geeFatal = GeeLoadingCalculator.GetFitnessModifiedGeeTolerance(tolerance.fatal, crewMember, gameConfig);

                if (meanG > geeFatal)
                {
                    return GeeLoadingOutCome.GeeFatal;
                }
                else if (meanG > geeWarn)
                {
                    return GeeLoadingOutCome.GeeWarn;
                }
            }

            return GeeLoadingOutCome.Ok;
        }

        private void onGeeWarn(KeepFitCrewMember crewMember)
        {
            string formatted = string.Format("KeepFit - Crewman {0} is reaching his G limits!", crewMember.Name);

            ScreenMessages.PostScreenMessage(formatted, 3f, ScreenMessageStyle.UPPER_CENTER);
        }

        private void onGeeFatal(KeepFitCrewMember crewMember)
        {
            if (!gameConfig.enabled || gameConfig.wimpMode)
            {
                string formatted = string.Format("KeepFit - Crewman {0} suffered momentary G-LOC!", crewMember.Name);

                ScreenMessages.PostScreenMessage(formatted, 3f, ScreenMessageStyle.UPPER_CENTER);
            }
            else
            {
                KerbalKiller.KillKerbal(this, crewMember);
            }
        }
    }
}
