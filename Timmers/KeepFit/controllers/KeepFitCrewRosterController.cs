using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                //this.Log_DebugOnly("RefreshRoster", "No gameConfig - bailing");
                return;
            }
            

            Dictionary<string, KeepFitCrewMember> roster = gameConfig.roster.crew;
            gameConfig.roster.available.crew.Clear();
            gameConfig.roster.assigned.crew.Clear();
            gameConfig.roster.vessels.Clear();

            // first go through all the crew in the system roster - find all the ones not doing anything,
            // and get them working for a living
            {
                KerbalRoster crewRoster = HighLogic.CurrentGame.CrewRoster;
                foreach (ProtoCrewMember crewMember in crewRoster.Crew)
                {
                    switch (crewMember.rosterStatus)
                    {
                        case ProtoCrewMember.RosterStatus.Available:
                            // you're sat on your arse in the crew building, so you can get down to the gym
                            updateRosters(roster, gameConfig.roster.available, crewMember.name, ActivityLevel.EXERCISING, true);
                            break;
                        case ProtoCrewMember.RosterStatus.Assigned:
                            // in flight - do this so we don't lose track of kerbals in the non-flight windows
                            // (until i sort out how to get all current vessels outside of flight
                            updateRosters(roster, gameConfig.roster.assigned, crewMember.name, ActivityLevel.UNKNOWN, false);
                            break;
                        case ProtoCrewMember.RosterStatus.Dead:
                        case ProtoCrewMember.RosterStatus.Missing:
                        default:
                            //roster.Remove(crewMember.name);
                            break;
                    }
                }
            }

            // then go through the vessels in the system - find out what activitylevel each crewmember gets
            // and update their stored activityLevel
            foreach (Vessel vessel in (FlightGlobals.fetch == null ? FlightGlobals.Vessels : FlightGlobals.fetch.vessels))
            {
                if (IsUnmanned(vessel))
                {
                    continue;
                }

                KeepFitVesselRecord vesselRecord = new KeepFitVesselRecord(vessel.vesselName, vessel.id.ToString());
                gameConfig.roster.vessels[vessel.id.ToString()] = vesselRecord;

                if (!vessel.loaded)
                {
                    refreshNonLoadedVesselRoster(roster, vesselRecord, vessel);
                }
                else
                {
                    refreshLoadedVesselRoster(roster, vesselRecord, vessel);
                }
            }
        }

        private void refreshNonLoadedVesselRoster(Dictionary<string, KeepFitCrewMember> roster, 
                                                  KeepFitVesselRecord vesselRecord,
                                                  Vessel vessel)
        {
            ActivityLevel defaultActivityLevel = getDefaultActivityLevel(vessel);

            foreach (ProtoPartSnapshot part in vessel.protoVessel.protoPartSnapshots)
            {
                foreach (ProtoCrewMember crewMember in part.protoModuleCrew)
                {
                    updateRosters(roster, vesselRecord, crewMember.name, defaultActivityLevel, false);
                }
            }
        }

        private List<ActivityLevel> levelsBestToWorst = new List<ActivityLevel>()
        {
            ActivityLevel.EXERCISING,
            ActivityLevel.NEUTRAL,
            ActivityLevel.COMFY,
            ActivityLevel.CRAMPED
        };

        private void refreshLoadedVesselRoster(Dictionary<string, KeepFitCrewMember> roster,
                                               KeepFitVesselRecord vesselRecord,
                                               Vessel vessel)
        {
            Dictionary<ActivityLevel, int> shipSeats = new Dictionary<ActivityLevel, int>();
            Dictionary<ActivityLevel, List<KeepFitCrewMember>> shipCrew = new Dictionary<ActivityLevel, List<KeepFitCrewMember>>();
            HashSet<KeepFitCrewMember> seatUpgradePool = new HashSet<KeepFitCrewMember>();

            foreach (ActivityLevel level in Enum.GetValues(typeof(ActivityLevel)))
            {
                shipSeats[level] = 0;
                shipCrew[level] = new List<KeepFitCrewMember>();
            }

            foreach (Part part in vessel.Parts)
            {
                if (part.CrewCapacity == 0)
                {
                    continue;
                }

                ActivityLevel partActivityLevel = getDefaultActivityLevel(vessel);

                foreach (PartModule module in part.Modules)
                {
                    if (module is KeepFitPartModule)
                    {
                        KeepFitPartModule keepFitPartModule = (KeepFitPartModule)module;

                        if (keepFitPartModule.activityLevel > partActivityLevel)
                        {
                            partActivityLevel = keepFitPartModule.activityLevel;
                        }
                    }
                }

                shipSeats[partActivityLevel] += part.CrewCapacity;

                foreach (ProtoCrewMember partCrewMember in part.protoModuleCrew)
                {
                    KeepFitCrewMember crew = updateRosters(roster, vesselRecord, partCrewMember.name, partActivityLevel, true);
                    shipCrew[partActivityLevel].Add(crew);
                    if (crew.WantsBetterSeat())
                    {
                        seatUpgradePool.Add(crew);
                    }
                }
            }

            foreach (ActivityLevel level in levelsBestToWorst)
            {
                int freeSeats = shipSeats[level];
                foreach (KeepFitCrewMember crew in shipCrew[level])
                {
                    if (crew.activityLevel > level)
                    {
                        // They were already upgraded, ignore them.
                    }
                    else if (crew.WillGiveUpSeat())
                    {
                        // Effectively, vacate the seat immediately and join the pool.
                        // If unfit or plenty of seats available, will get immediately re-added to the seat.
                        seatUpgradePool.Add(crew);
                    }
                    else
                    {
                        freeSeats -= 1;
                        seatUpgradePool.Remove(crew); // already in the best seat possible
                    }
                }

                this.Log_DebugOnly("KeepFitSeats", "" + vessel.name + " seats [" + level + "]: " + freeSeats + " available, " + shipSeats[level] + " total");

                foreach (KeepFitCrewMember crew in seatUpgradePool.OrderBy(crew => crew.fitnessLevel).ToList())
                {
                    if (freeSeats < 1)
                    {
                        break;
                    }

                    this.Log_DebugOnly("KeepFitSeatUpgrade", "Upgrading " + crew.Name + " from " + crew.activityLevel + " to " + level);
                    updateRosters(roster, vesselRecord, crew.Name, level, true);
                    seatUpgradePool.Remove(crew);
                    freeSeats -= 1;
                }
            }
        }

        private bool IsUnmanned(Vessel vessel)
        {
            return (GetCrewCount(vessel) == 0);
        }

        private int GetCrewCount(Vessel vessel)
        {
            if (vessel.packed && !vessel.loaded)
            {
                return vessel.protoVessel.GetVesselCrew().Count;
            }
            else
            {
                return vessel.GetCrewCount();
            }
        }

        private ActivityLevel getDefaultActivityLevel(Vessel vessel)
        {
            // if vessel is landed/splashed then assume we can exercise freely (meh)
            if (module.isVesselLandedOnExercisableSurface(vessel))
            {
                return ActivityLevel.EXERCISING;
            }
            
            return ActivityLevel.CRAMPED;
        }

        private KeepFitCrewMember updateRosters(Dictionary<string, KeepFitCrewMember> roster,
                                                KeepFitVesselRecord vessel,
                                                string name,
                                                ActivityLevel activityLevel,
                                                bool activityLevelReliable)
        {
            this.Log_DebugOnly("updateRosters", "updating crewMember[{0}] activityLevel[{1}]]", name, activityLevel);

            KeepFitCrewMember keepFitCrewMember = null;
            roster.TryGetValue(name, out keepFitCrewMember);
            if (keepFitCrewMember != null)
            {
                this.Log_DebugOnly("updateRosters", "crewMember[{0}] was in the old roster", name);
            }
            else
            {
                this.Log_DebugOnly("updateRosters", "crewMember[{0}] wasn't in the old roster", name);

                // not in the old roster - add him to the new one ... 
                keepFitCrewMember = new KeepFitCrewMember(name, false);
                keepFitCrewMember.fitnessLevel = gameConfig.initialFitnessLevel;
                keepFitCrewMember.activityLevel = activityLevel;
                roster[name] = keepFitCrewMember;
            }

            if (activityLevelReliable)
            {
                keepFitCrewMember.activityLevel = activityLevel;
            }            

            vessel.crew[name] = keepFitCrewMember;

            this.Log_DebugOnly("updateRosters", "crewMan[{0}] activityLevel[{1}]", name, activityLevel);

            return keepFitCrewMember;
        }
    }
}
