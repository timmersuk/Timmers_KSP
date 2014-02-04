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

            InvokeRepeating("RefreshRoster", 5, 10);
        }


        internal void RefreshRoster()
        {
            this.Log_DebugOnly("RefreshRoster", ".");

            if (gameConfig == null)
            {
                this.Log_DebugOnly("RefreshRoster", "No gameConfig - bailing");
                return;
            }


            Dictionary<string, KeepFitCrewMember> oldRoster = new Dictionary<string, KeepFitCrewMember>(gameConfig.roster.crew);
            gameConfig.roster.crew.Clear();
            gameConfig.roster.available.crew.Clear();
            gameConfig.roster.assigned.crew.Clear();
            gameConfig.roster.vessels.Clear();

            // first go through all the crew in the system roster - find all the ones not doing anything,
            // and get them working for a living
            {

                CrewRoster crewRoster = HighLogic.CurrentGame.CrewRoster;
                foreach (ProtoCrewMember crewMember in crewRoster)
                {
                    switch (crewMember.rosterStatus)
                    {
                        case ProtoCrewMember.RosterStatus.AVAILABLE:
                            // you're sat on your arse in the crew building, so you can get down to the gym
                            updateRosters(oldRoster, gameConfig.roster.crew, gameConfig.roster.available, crewMember.name, ActivityLevel.EXERCISING);
                            break;
                        case ProtoCrewMember.RosterStatus.ASSIGNED:
                            // in flight - do this so we don't lose track of kerbals in the non-flight windows
                            // (until i sort out how to get all current vessels outside of flight
                            KeepFitCrewMember temp = updateRosters(oldRoster, gameConfig.roster.crew, gameConfig.roster.assigned, crewMember.name, ActivityLevel.UNKNOWN);

                            // bodge so we don't lose stored crew settings
                            oldRoster[crewMember.name] = temp;
                            break;
                    }
                }
            }

            // then go through the vessels in the system - find out what activitylevel each crewmember gets
            // and update their stored activityLevel
            foreach (Vessel vessel in FlightGlobals.Vessels)
            {
                KeepFitVesselRecord vesselRecord = new KeepFitVesselRecord(vessel.name, vessel.id.ToString());

                gameConfig.roster.vessels[vessel.id.ToString()] = vesselRecord;

                vesselRecord.activityLevel = getDefaultActivityLevel(vessel);
                foreach (Part part in vessel.Parts)
                {
                    foreach (PartModule module in part.Modules)
                    {
                        if (!(module is KeepFitPartModule))
                        {
                            continue;
                        }

                        KeepFitPartModule keepFitPartModule = (KeepFitPartModule)module;

                        if (keepFitPartModule.activityLevel > vesselRecord.activityLevel)
                        {
                            vesselRecord.activityLevel = keepFitPartModule.activityLevel;
                        }
                        // TDXX - add code here to determine 'the best' keep fit part ... when that has meaning
                        // instead of stopping at the first part
                        vesselRecord.hasKeepFitPartModule = true;

                        break;
                    }
                }

                if (vessel.loaded)
                {
                    foreach (ProtoCrewMember crewMember in vessel.GetVesselCrew())
                    {
                        updateRosters(oldRoster, gameConfig.roster.crew, vesselRecord, crewMember.name, vesselRecord.activityLevel);
                    }
                }
                else
                {
                    foreach (ProtoPartSnapshot part in vessel.protoVessel.protoPartSnapshots)
                    {
                        foreach (ProtoCrewMember crewMember in part.protoModuleCrew)
                        {
                            updateRosters(oldRoster, gameConfig.roster.crew, vesselRecord, crewMember.name, vesselRecord.activityLevel);

                        }
                    }
                }
            }
        }

        private ActivityLevel getDefaultActivityLevel(Vessel vessel)
        {
            // if vessel is landed/splashed then assume we can exercise freely (meh)
            if (vessel.LandedOrSplashed)
            {
                return ActivityLevel.EXERCISING;
            }
            
            return ActivityLevel.CRAMPED;
        }

        private KeepFitCrewMember updateRosters(Dictionary<string, KeepFitCrewMember> oldRoster,
                                                Dictionary<string, KeepFitCrewMember> newRoster,
                                                KeepFitVesselRecord vessel,
                                                string name,
                                                ActivityLevel activityLevel)
        {
            this.Log_DebugOnly("updateRosters", "updating crewMember[{0}] activityLevel[{1}]]", name, activityLevel);

            KeepFitCrewMember keepFitCrewMember = null;
            oldRoster.TryGetValue(name, out keepFitCrewMember);
            if (keepFitCrewMember != null)
            {
                this.Log_DebugOnly("updateRosters", "crewMember[{0}] was in the old roster", name);

                oldRoster.Remove(name);
            }
            else
            {
                this.Log_DebugOnly("updateRosters", "crewMember[{0}] wasn't in the old roster", name);

                // not in the old roster - add him to the new one ... 
                keepFitCrewMember = new KeepFitCrewMember(name);
                keepFitCrewMember.fitnessLevel = gameConfig.initialFitnessLevel;
            }

            //keepFitCrewMember.vessel = vessel;
            keepFitCrewMember.activityLevel = activityLevel;
            newRoster[name] = keepFitCrewMember;

            vessel.crew[name] = keepFitCrewMember;

            this.Log_DebugOnly("updateRosters", "crewMan[{0}] activityLevel[{1}]", name, activityLevel);

            return keepFitCrewMember;
        }
    }
}
