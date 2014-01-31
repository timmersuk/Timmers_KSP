using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbar;
using UnityEngine;

namespace KeepFit
{
    // <summary>
    /// Keeps the crewRoster in the gameConfig up to date
    /// </summary>
    public class KeepFitCrewRosterController : KeepFitController
    {     
        internal void Awake()
        {
            this.Log_DebugOnly("Awake", ".");

            if (!HighLogic.LoadedSceneIsFlight)
            {
                this.Log_DebugOnly("Awake", "Not in flight scene - not starting timer tasks");
                return;
            }  

            InvokeRepeating("DiscoverKerbals", 5, 30);
        }


        internal void DiscoverKerbals()
        {
            this.Log_DebugOnly("DiscoverKerbals", ".");

            if (gameConfig == null)
            {
                this.Log_DebugOnly("DiscoverKerbals", "No gameConfig - bailing");
                return;
            }

            if (!HighLogic.LoadedSceneIsFlight)
            {
                this.Log_DebugOnly("UpdateFitnessLevels", "Not in flight scene - bailing");
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
                    updateRosters(oldRoster, gameConfig.knownCrew, crewMember.name, null, ActivityLevel.EXERCISING);
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
                    ActivityLevel activityLevel;
                    if (vesselKeepFitPartModule == null)
                    {
                        activityLevel = getActivityLevel(vessel);
                    }
                    else 
                    {
                        activityLevel = ActivityLevel.NEUTRAL;
                    }

                    updateRosters(oldRoster, gameConfig.knownCrew, crewMember.name, vessel.GetName(), activityLevel);
                }
            }
        }

        private ActivityLevel getActivityLevel(Vessel vessel)
        {
            // if vessel is landed/splashed then assume we can exercise freely (meh)
            if (vessel.LandedOrSplashed)
            {
                return ActivityLevel.EXERCISING;
            }

            // if vessel is not in orbit, we treat it as neutral for now
            if (vessel.situation == Vessel.Situations.FLYING ||
                vessel.situation == Vessel.Situations.SUB_ORBITAL)
            {
                return ActivityLevel.NEUTRAL;
            }
            
            // we'll need to find where the crewmember is in the vessel, see how cramped the crewmember is in the part, 
            // and see what modules that part has

            // for now we'll say he's totally cramped if in EVA, or otherwise just plain idle
            if (vessel.isEVA)
            {
                return ActivityLevel.TOOCRAMPEDTOMOVE;
            }

            if (vessel.GetCrewCount() == vessel.GetCrewCapacity())
            {
                return ActivityLevel.TOOCRAMPEDTOMOVE;
            }

            // crewmember is in a vessel with spare capacity
            return ActivityLevel.CRAMPED;
        }

        private void updateRosters(Dictionary<string, KeepFitCrewMember> oldRoster,
                                   Dictionary<string, KeepFitCrewMember> newRoster,
                                   string name,
                                   string vesselName,
                                   ActivityLevel activityLevel)
        {
            this.Log_DebugOnly("updateRosters", "updating crewMember[{0}] activityLevel[{1}]]", name, activityLevel);

            KeepFitCrewMember keepFitCrewMember = null;
            try
            {
                keepFitCrewMember = oldRoster[name];

                this.Log_DebugOnly("updateRosters", "crewMember[{0}] was in the old roster", name);

                oldRoster.Remove(name);
            }
            catch (KeyNotFoundException)
            {
                this.Log_DebugOnly("updateRosters", "crewMember[{0}] wasn't in the old roster", name);

                // not in the old roster - add him to the new one ... 
                keepFitCrewMember = new KeepFitCrewMember(name);
                keepFitCrewMember.fitnessLevel = gameConfig.initialFitnessLevel;
            }

            keepFitCrewMember.vesselName = vesselName;
            keepFitCrewMember.activityLevel = activityLevel;
            newRoster.Add(name, keepFitCrewMember);

            this.Log_DebugOnly("updateRosters", "crewMan[{0}] activityLevel[{1}]", name, activityLevel);
        }
    }
}
