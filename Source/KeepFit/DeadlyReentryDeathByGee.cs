using System;
using System.Collections.Generic;
using UnityEngine;

namespace KeepFit
{
    interface GeeLevelsProvider
    {
        float getGWarn(KeepFitCrewMember crewMember);
        float getGFatal(KeepFitCrewMember crewMember);
    }

    interface OverGeeHandler
    {
        void onOverGee(Part part, KeepFitCrewMember crewMember);
    }

    /// <summary>
    /// Checking for the effects of Vessel Gee on the occupants of said vessel
    /// Taken from DeadlyReentry 2 by NathanKell, originally built by ialdabaoth, based on r4m0n's Deadly Reentry
    /// </summary>
    class DeadlyReentryDeathByGee
    {
        private float gExperienced;
        private float displayGForce;
        private float lastGForce;


        private void updateVesselGeeForces(Vessel vessel,
                                           float crewGClamp,
                                           float crewGPower,
                                           float crewGMin)
        {
            if (!HighLogic.LoadedSceneIsFlight || !FlightGlobals.ready)
            {
                // don't check g forces if we aren't flying or aren't ready
                return;
            }
               
            float deltaTime = TimeWarp.fixedDeltaTime;
            if ((object)vessel == null || deltaTime > 0.5 || deltaTime <= 0)
            {
                return; // don't check G-forces in warp
            }

            float geeForce = (float)vessel.geeForce_immediate;

            if (geeForce > 40 && geeForce > lastGForce)
            {
                // G forces over 40 are probably a Kraken twitch unless they last multiple frames
                displayGForce = displayGForce * (1 - deltaTime) + (lastGForce * deltaTime);
            }
            else
            {
                //keep a running average of G force over 1s, to further prevent absurd spikes (mostly decouplers & parachutes)
                displayGForce = displayGForce * (1 - deltaTime) + (geeForce * deltaTime);
            }
            lastGForce = geeForce;

            if (displayGForce < crewGMin)
            { 
                gExperienced = 0;
            }
            
            float effectiveGeeForce = Math.Abs(Math.Max(displayGForce, geeForce));
              
            float clampedGeeForce = Math.Min(effectiveGeeForce, crewGClamp);

            gExperienced += ((float)Math.Pow(clampedGeeForce, crewGPower)) * deltaTime;
        }


        public void CheckGeeEffectsOnCrew(Part part,
                                          List<KeepFitCrewMember> crewMembers,
                                          GeeLevelsProvider geeLevelsProvider,
                                          OverGeeHandler overGeeHandler)
        {
            if (crewMembers.Count == 0)
            {
                // don't check g forces if we don't have any crew
                return;
            }
                   
            foreach (KeepFitCrewMember crewMember in crewMembers)
            {
                float crewGWarn = geeLevelsProvider.getGWarn(crewMember);
                float crewGLimit = geeLevelsProvider.getGFatal(crewMember);

                if (gExperienced < crewGWarn)
                {
                    // all good
                    continue;
                }
                
                if (gExperienced < crewGLimit)
                {
                    ScreenMessages.PostScreenMessage("KeepFit - Crewman " + crewMember.Name + " is reaching his G limit!", 3f, ScreenMessageStyle.UPPER_CENTER);
                    continue;
                }
                      
                ScreenMessages.PostScreenMessage("KeepFit - Crewman " + crewMember.Name + " breached his G limit!", 3f, ScreenMessageStyle.UPPER_CENTER);
                if (overGeeHandler != null)
                {
                    overGeeHandler.onOverGee(part, crewMember);
                }
            }
        }
    }

    class RandomlyFatalOverGeeHandler : OverGeeHandler
    {
        public float gKillChance { get; set; }

        private ProtoCrewMember getPartCrewMember(Part part, string name)
        {
            List<ProtoCrewMember> crew = part.vessel.GetVesselCrew();
            foreach (ProtoCrewMember crewman in crew)
            {
                if (crewman.name.Equals(name))
                {
                    return crewman;
                }
            }

            return null;
        }

        static string FormatTime(double time)
        {
            int iTime = (int)time % 3600;
            int seconds = iTime % 60;
            int minutes = (iTime / 60) % 60;
            int hours = (iTime / 3600);

            return hours.ToString("D2")
                   + ":" + minutes.ToString("D2") + ":" + seconds.ToString("D2");
        }

        public void onOverGee(Part part, KeepFitCrewMember crewMember)
        {
            // borrowed from TAC Life Support
            if (UnityEngine.Random.Range(0, 1) > gKillChance)
            {
                // lucky this time!
                return;
            }
            
            ProtoCrewMember member = getPartCrewMember(part, crewMember.Name);
            if (member == null)
            {
                // very odd - we couldn't find the ProtoCrewMember for this KeepFitCrewMember
                Debug.Log("[RandomlyFatalOverGeeHandler] - very odd - we couldn't find the ProtoCrewMember for this KeepFitCrewMember");
                return;
            }

            if (CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA)
            {
                CameraManager.Instance.SetCameraFlight();
            }
            
            ScreenMessages.PostScreenMessage(part.vessel.vesselName + ": Crewmember " + member.name + " died of G-force damage!", 30.0f, ScreenMessageStyle.UPPER_CENTER);
            FlightLogger.eventLog.Add("[" + FormatTime(part.vessel.missionTime) + "] " + member.name + " died of G-force damage.");
            Debug.Log("[RandomlyFatalOverGeeHandler] - [" + Time.time + "]: " + part.vessel.vesselName + " - " + member.name + " died of G-force damage.");
            
            if (!part.vessel.isEVA)
            {
                part.RemoveCrewmember(member);
                member.Die();
            }
        }

    }
}
