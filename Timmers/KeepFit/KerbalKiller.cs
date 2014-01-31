using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KeepFit
{
    static class KerbalKiller
    {
        public static void KillKerbal(System.Object source, KeepFitCrewMember crewMember)
        {
            Vessel vessel;
            Part part;
            ProtoCrewMember member;
            if (!getVesselCrewMember(crewMember.Name, out vessel, out part, out member))
            {
                // very odd - we couldn't find the ProtoCrewMember for this KeepFitCrewMember
                source.Log("KerbalKiller", "very odd - we couldn't find the ProtoCrewMember for this KeepFitCrewMember");
                return;
            }

            if (CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA)
            {
                CameraManager.Instance.SetCameraFlight();
            }


            ScreenMessages.PostScreenMessage(vessel.vesselName + ": Crewmember " + member.name + " died of G-force damage!", 30.0f, ScreenMessageStyle.UPPER_CENTER);
            FlightLogger.eventLog.Add("[" + FormatTime(vessel.missionTime) + "] " + member.name + " died of G-force damage.");
            source.Log("KerbalKiller", "" + Time.time + "]: " + vessel.vesselName + " - " + member.name + " died of G-force damage.");

            if (!vessel.isEVA)
            {
                part.RemoveCrewmember(member);
                member.Die();
            }
        }

        private static bool getVesselCrewMember(string name, out Vessel vessel, out Part part, out ProtoCrewMember member)
        {
            foreach (Vessel xVessel in FlightGlobals.Vessels)
            {
                foreach (Part xPart in xVessel.Parts)
                { 
                    foreach (ProtoCrewMember crewman in xPart.protoModuleCrew)
                    {
                        if (crewman.name.Equals(name))
                        {
                            vessel = xVessel;
                            part = xPart;
                            member = crewman;
                            return true;
                        }
                    }
                }
            }

            vessel = null;
            part = null;
            member = null;
            return false;
        }

        private static string FormatTime(double time)
        {
            int iTime = (int)time % 3600;
            int seconds = iTime % 60;
            int minutes = (iTime / 60) % 60;
            int hours = (iTime / 3600);

            return hours.ToString("D2")
                   + ":" + minutes.ToString("D2") + ":" + seconds.ToString("D2");
        }

    }
}
