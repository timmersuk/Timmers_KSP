using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbar;
using UnityEngine;

namespace KeepFit
{
    // <summary>
    /// This is the behaviour object that we hook events on to for flight
    /// </summary>
    public class KeepFitController : MonoBehaviour
    {
        private double lastGeeLoadingUpdateUT = -1;

        /// <summary>
        /// Per-game saved configuration information
        /// </summary>
        internal GameConfig gameConfig { get; private set; }

        public KeepFitController()
        {
            this.Log("ctor", ".");
        }

        internal void SetGameConfig(GameConfig gameConfig)
        {
            this.Log("SetGameConfig", ".");
            this.gameConfig = gameConfig;
        }

        

        internal void Awake()
        {
            this.Log("Awake", ".");

            if (!HighLogic.LoadedSceneIsFlight)
            {
                this.Log("Awake", "Not in flight scene - not starting timer tasks");
                return;
            }  

            InvokeRepeating("DiscoverKerbals", 5, 30);

            InvokeRepeating("UpdateFitnessLevels", 1, 1);
        }

        public void FixedUpdate()
        {
            this.Log("FixedUpdate", ".");

            if (gameConfig == null)
            {
                this.Log("FixedUpdate", "No gameConfig - bailing");
                return;
            }

            if (!HighLogic.LoadedSceneIsFlight)
            {
                this.Log("FixedUpdate", "Not in flight scene - bailing");
                return;
            }  

            if (lastGeeLoadingUpdateUT == -1)
            {
                this.Log("FixedUpdate", "No lastGeeLoadingUpdateUT - skipping this update");

                // don't do anything this time
                this.lastGeeLoadingUpdateUT = Planetarium.GetUniversalTime();
                return;
            }
            
            double currentUT = Planetarium.GetUniversalTime();
            float elapsed = (float)(currentUT - lastGeeLoadingUpdateUT);
            lastGeeLoadingUpdateUT = currentUT;

            this.Log("FixedUpdate", "[" + elapsed + "] seconds since last fixed update");

            // just check gee loading on active vessel for now
            Vessel vessel = FlightGlobals.ActiveVessel;
            if (vessel == null)
            {
                this.Log("FixedUpdate", "No active vessel");
                return;
            }

            this.Log("FixedUpdate", "Checking gee loading for active vessel[" + vessel.GetName() + "]");

            float geeLoading;
            string invalidReason;
            bool valid = GeeLoadingCalculator.getGeeLoading(vessel, out geeLoading, out invalidReason);
            if (!valid)
            {
                this.Log("FixedUpdate", "Gee loading for active vessel[" + vessel.GetName() + "] is not valid currently because[" + invalidReason + "]");
            }
            else
            {
                this.Log("FixedUpdate", "Gee loading for active vessel[" + vessel.GetName() + "] is[" + geeLoading + "] letting the crew know");
                foreach (ProtoCrewMember crewMember in vessel.GetVesselCrew())
                {
                    try
                    {
                        this.Log("FixedUpdate", "Gee loading for active vessel[" + vessel.GetName() + "] is[" + geeLoading + "] letting [" + crewMember.name + "] know");
                        
                        KeepFitCrewMember keepFitCrewMember = gameConfig.knownCrew[crewMember.name];

                        keepFitCrewMember.accumulatGeeLoading(geeLoading, elapsed);
                    }
                    catch (KeyNotFoundException)
                    {
                        // ignore, we'll pick them up later
                        this.Log("FixedUpdate", "Gee loading for active vessel[" + vessel.GetName() + "] is[" + geeLoading + "] crewmember [" + crewMember.name + "] not in the keepfit roster yet");
                        
                    }
                }
            }
        }


        private void UpdateFitnessLevels()
        {
            if (gameConfig == null)
            {
                this.Log("UpdateFitnessLevels", "No gameConfig - bailing");
                return;
            }

            if (!HighLogic.LoadedSceneIsFlight)
            {
                this.Log("UpdateFitnessLevels", "Not in flight scene - bailing");
                return;
            }   

            double currentUT = Planetarium.GetUniversalTime();

            // work out how long since the last refresh - we really need to store the lastUpdateUT in the persistence file
            // in order for this to work correctly
            float elapsed = (float)(currentUT - gameConfig.lastExerciseUT);

            this.Log("UpdateFitnessLevels", "lastUpdateUT[" + gameConfig.lastExerciseUT + "] currentUT[" + currentUT + "] elapsed[" + elapsed + "]");

            if (!gameConfig.enabled)
            {
                this.Log("UpdateFitnessLevels", "not enabled");

                // update the time we last exercised our kerbals, otherwise we get a shock when re-enable
                gameConfig.lastExerciseUT = currentUT;

                return;
            }

            // update the fitness of the kerbals based on the time elapsed and their current activitylevel
            // - this assumes they've been doing what they are doing now, ever since the last update, which
            //   is slightly bogus but I'm shooting for this as a close enough approximation for now.
            // - the alternative later strategy will be to exerciseKerbals whenever the scene changes, and 
            //   be aware where we're going from and to - so from VAB/SPH to Flight, our active vessel kerbals
            //   can be assumed to have been in the gym at KSC until liftoff (otherwise, if its been a whole interval
            //   since we last exercised, then they'll seem to have been sat in the cramped capsule for all that time
            //   which would be bad
            exerciseKerbals(elapsed);

            // update the time we last exercised our kerbals
            gameConfig.lastExerciseUT = currentUT;
        }

        private void exerciseKerbals(float timeSinceLastExercise)
        {
            this.Log("exerciseKerbals", "timeSinceLastExercise[" + timeSinceLastExercise + "]");

            foreach (KeepFitCrewMember crewMember in gameConfig.knownCrew.Values)
            {
                float oldFitnessLevel = crewMember.fitnessLevel;
                float fitnessModifier = getFitnessModifier(crewMember.activityLevel, timeSinceLastExercise);

                crewMember.AddTime(timeSinceLastExercise);

                float updatedFitnessLevel = oldFitnessLevel + fitnessModifier;

                // cap out our fitness level - we can only go so far
                if (updatedFitnessLevel > gameConfig.maxFitnessLevel)
                {
                    updatedFitnessLevel = gameConfig.maxFitnessLevel;
                }

                // cap out our minimum - perhaps if fitness gets too low the kerbal should 
                // kark it instead
                if (updatedFitnessLevel < gameConfig.minFitnessLevel)
                {
                    updatedFitnessLevel = gameConfig.minFitnessLevel;
                }

                crewMember.fitnessLevel = updatedFitnessLevel;
                //this.Log("exerciseKerbals", "crewMan[" + crewMember.Name + "] oldFitnessLevel[" + oldFitnessLevel + "] updatedFitnessLevel[" + updatedFitnessLevel + "]");
            }
        }

        private float getFitnessModifier(KeepFitActivityLevel activityLevel, float elapsedSeconds)
        {
            float secondsPerDay = 60 * 60 * 24;

            float fitnessModifier;

            switch (activityLevel)
            {
                case KeepFitActivityLevel.TOOCRAMPEDTOMOVE:
                    // too cramped to move - fitness goes down 10% per day by default
                    fitnessModifier = (gameConfig.degradationWhenTooCrampedToMove * elapsedSeconds) / secondsPerDay;
                    break;

                case KeepFitActivityLevel.CRAMPED:
                    // cramped - fitness goes down 5% per day by default
                    fitnessModifier = (gameConfig.degradationWhenCramped * elapsedSeconds) / secondsPerDay;
                    break;

                case KeepFitActivityLevel.COMFY:
                    // comfy - fitness goes down 1% per day by default
                    fitnessModifier = (gameConfig.degradationWhenComfy * elapsedSeconds) / secondsPerDay;
                    break;

                case KeepFitActivityLevel.NEUTRAL:
                    // neutral - fitness is static by default
                    fitnessModifier = (gameConfig.degradationWhenNeutral * elapsedSeconds) / secondsPerDay;
                    break;

                case KeepFitActivityLevel.EXERCISING:
                    // exercising - fitness goes up 1% per day by default
                    fitnessModifier = (gameConfig.degradationWhenExercising * elapsedSeconds) / secondsPerDay;
                    break;

                default:
                    // how did we even get here?
                    fitnessModifier = 0;
                    break;
            }

            return fitnessModifier;
        }

        internal void DiscoverKerbals()
        {
            this.Log("DiscoverKerbals", ".");

            if (gameConfig == null)
            {
                this.Log("DiscoverKerbals", "No gameConfig - bailing");
                return;
            }

            if (!HighLogic.LoadedSceneIsFlight)
            {
                this.Log("UpdateFitnessLevels", "Not in flight scene - bailing");
                return;
            }   

            Dictionary<string, KeepFitCrewMember> oldRoster = new Dictionary<string, KeepFitCrewMember>(gameConfig.knownCrew);
            gameConfig.knownCrew.Clear();

            // first go through all the crew in the system roster - find all the ones not doing anything,
            // and get them working for a living
            CrewRoster crewRoster = HighLogic.CurrentGame.CrewRoster;
            foreach (ProtoCrewMember crewMember in crewRoster)
            {
                if (crewMember.rosterStatus == ProtoCrewMember.RosterStatus.AVAILABLE)
                {
                    // your sat on your arse in the crew building, so you can get down to the gym
                    updateRosters(oldRoster, gameConfig.knownCrew, crewMember.name, null, KeepFitActivityLevel.EXERCISING);
                }
            }

            // then go through the vessels in the system - find out what activitylevel each crewmember gets
            // and update their stored activityLevel
            foreach (Vessel vessel in FlightGlobals.Vessels)
            {
                KeepFitPartModule vesselKeepFitPartModule = null; 
                foreach (Part part in vessel.Parts)
                {
                    foreach (PartModule module in part.Modules)
                    {
                        if (!(module is KeepFitPartModule))
                        {
                            continue;
                        }

                        // TDXX - add code here to determine 'the best' keep fit part ... when that has meaning
                        // instead of stopping at the first part
                        vesselKeepFitPartModule = (KeepFitPartModule)module;
                        
                        break;
                    }
                }

                foreach (ProtoCrewMember crewMember in vessel.GetVesselCrew())
                {
                    KeepFitActivityLevel activityLevel;
                    if (vesselKeepFitPartModule == null)
                    {
                        activityLevel = getActivityLevel(vessel);
                    }
                    else 
                    {
                        activityLevel = KeepFitActivityLevel.NEUTRAL;
                    }

                    updateRosters(oldRoster, gameConfig.knownCrew, crewMember.name, vessel.GetName(), activityLevel);
                }
            }
        }

        private KeepFitActivityLevel getActivityLevel(Vessel vessel)
        {
            // if vessel is landed/splashed then assume we can exercise freely (meh)
            if (vessel.LandedOrSplashed)
            {
                return KeepFitActivityLevel.EXERCISING;
            }

            // if vessel is not in orbit, we treat it as neutral for now
            if (vessel.situation == Vessel.Situations.FLYING ||
                vessel.situation == Vessel.Situations.SUB_ORBITAL)
            {
                return KeepFitActivityLevel.NEUTRAL;
            }
            
            // we'll need to find where the crewmember is in the vessel, see how cramped the crewmember is in the part, 
            // and see what modules that part has

            // for now we'll say he's totally cramped if in EVA, or otherwise just plain idle
            if (vessel.isEVA)
            {
                return KeepFitActivityLevel.TOOCRAMPEDTOMOVE;
            }

            if (vessel.GetCrewCount() == vessel.GetCrewCapacity())
            {
                return KeepFitActivityLevel.TOOCRAMPEDTOMOVE;
            }

            // crewmember is in a vessel with spare capacity
            return KeepFitActivityLevel.CRAMPED;
        }

        private void updateRosters(Dictionary<string, KeepFitCrewMember> oldRoster,
                                   Dictionary<string, KeepFitCrewMember> newRoster,
                                   string name,
                                   string vesselName,
                                   KeepFitActivityLevel activityLevel)
        {
            this.Log("updateRosters", "updating crewMember[" + name + "] activityLevel[" + activityLevel + "]");

            this.Log("updateRosters", "1");

            KeepFitCrewMember keepFitCrewMember = null;
            try
            {
                keepFitCrewMember = oldRoster[name];

                this.Log("updateRosters", "crewMember[" + name + "] was in the old roster");

                this.Log("updateRosters", "5");
                oldRoster.Remove(name);
            }
            catch (KeyNotFoundException)
            {
                this.Log("updateRosters", "crewMember[" + name + "] wasn't in the old roster");

                // not in the old roster - add him to the new one ... 
                this.Log("updateRosters", "3");
                keepFitCrewMember = new KeepFitCrewMember(name);
                this.Log("updateRosters", "4");
                keepFitCrewMember.fitnessLevel = gameConfig.initialFitnessLevel;
            }

            this.Log("updateRosters", "6");

            keepFitCrewMember.vesselName = vesselName;
            keepFitCrewMember.activityLevel = activityLevel;
            newRoster.Add(name, keepFitCrewMember);

            this.Log("updateRosters", "crewMan[" + name + "] activityLevel[" + activityLevel + "]");
        }
    }
}
