﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace KeepFit
{
    public class GeeToleranceConfig : ConfigNodeStorage
    {
        [Persistent]
        internal float warn;

        [Persistent]
        internal float fatal;

        public GeeToleranceConfig()
        {
        }

        public GeeToleranceConfig(float warn, float fatal)
        {
            this.warn = warn;
            this.fatal = fatal;
        }
    }

    public class GeeToleranceConfigAndPeriod : ConfigNodeStorage
    {
        [Persistent]
        internal Period period;

        [Persistent]
        internal GeeToleranceConfig tolerance;

        public GeeToleranceConfigAndPeriod()
        {
        }

        public GeeToleranceConfigAndPeriod(Period period, GeeToleranceConfig config)
        {
            this.period = period;
            this.tolerance = config;
        }
    }


    public class GameConfig : ConfigNodeStorage
    {
        [Persistent]
        private Dictionary<string, Vector4> windowRects = new Dictionary<string, Vector4>();

        [Persistent]
        internal Single initialFitnessLevel = 90;

        [Persistent]
        internal Single maxFitnessLevel = 100;

        [Persistent]
        internal Single minFitnessLevel = 0;

        [Persistent]
        internal Boolean enabled = false;

        [Persistent]
        internal Boolean wimpMode = false;

        [Persistent]
        internal Boolean useBestPartOnVessel = false;

        [Persistent]
        internal Boolean applyCLSLimitsIfAvailable = true;

        [Persistent]
        internal Double lastExerciseUT = -1;

        /// <summary>
        /// Persistent copy of the 
        /// </summary>
        [Persistent]
        internal float degradationWhenCramped = -5.0f;
        [Persistent]
        internal float degradationWhenComfy = -1.0f;
        [Persistent]
        internal float degradationWhenNeutral = -0.0f;
        [Persistent]
        internal float degradationWhenExercising = 1.0f;

        [Persistent]
        internal float minimumLandedGeeForExcercising = 0.05f;

        [Persistent]
        internal Roster roster = new Roster();

        [Persistent]
        private GeeToleranceConfigAndPeriod[] geeTolerancesStore;
        internal Dictionary<Period, GeeToleranceConfig> geeTolerances = new Dictionary<Period, GeeToleranceConfig>();

        private static Dictionary<Period, GeeToleranceConfig> GetDefaultGeeTolerances()
        {
            Dictionary<Period, GeeToleranceConfig> def = new Dictionary<Period, GeeToleranceConfig>();

            def[Period.Inst] = new GeeToleranceConfig(30.0f, 40.0f);  // 1 sec
            def[Period.Short] = new GeeToleranceConfig(15.0f, 20.0f);  // 5 sec
            def[Period.Medium] = new GeeToleranceConfig(10.0f, 15.0f);  // 1 min
            def[Period.Long] = new GeeToleranceConfig(5.0f, 10.0f);  // 5 mins

            return def;
        }
        
        public GameConfig()
            : base("KeepFitSavedGameSettings")
        {
            geeTolerances = GetDefaultGeeTolerances();
        }

        public bool validate()
        {
            bool modified = false;
            if (maxFitnessLevel < minFitnessLevel)
            {
                modified = true;
                maxFitnessLevel = minFitnessLevel;
            }

            if (initialFitnessLevel > maxFitnessLevel)
            {
                modified = true;
                initialFitnessLevel = maxFitnessLevel;
            }

            if (initialFitnessLevel < minFitnessLevel)
            {
                modified = true;
                initialFitnessLevel = minFitnessLevel;
            }

            return modified;
        }

        public void SetWindowRect(string name, Rect value)
        {
            windowRects[name] = new Vector4(value.xMin, value.yMin, value.width, value.height);
        }

        public bool GetWindowRect(string name, ref Rect destination)
        {
            Vector4 destVector;
            if (windowRects.TryGetValue(name, out destVector))
            {
                destination = new Rect(destVector.x, destVector.y, destVector.z, destVector.w);
                return true;
            }

            return false;
        }

        public GeeToleranceConfig GetGeeTolerance(Period period)
        {
            GeeToleranceConfig tolerance;

            geeTolerances.TryGetValue(period, out tolerance);

            return tolerance;
        }


        public override void OnDecodeFromConfigNode()
        {
            // copy across the G tolerances from persist
            {
                geeTolerances.Clear();
                if (geeTolerancesStore != null)
                {
                    foreach (GeeToleranceConfigAndPeriod geeToleranceAndPeriod in geeTolerancesStore)
                    {
                        geeTolerances[geeToleranceAndPeriod.period] = geeToleranceAndPeriod.tolerance;
                    }
                }

                if (geeTolerances.Count == 0)
                {
                    geeTolerances = GetDefaultGeeTolerances();
                }
            }
        }


        public override void OnEncodeToConfigNode()
        {
            // copy across the G tolerances to persist
            {
                List<GeeToleranceConfigAndPeriod> temp = new List<GeeToleranceConfigAndPeriod>();
                foreach (KeyValuePair<Period, GeeToleranceConfig> key in geeTolerances)
                {
                    temp.Add(new GeeToleranceConfigAndPeriod(key.Key, key.Value));
                }

                geeTolerancesStore = new GeeToleranceConfigAndPeriod[geeTolerances.Values.Count];
                temp.CopyTo(geeTolerancesStore);
            }
        }
    }

    internal class KeepFitVesselRecord : ConfigNodeStorage
    {
        [Persistent]
        internal string name;

        [Persistent]
        internal string id;

        [Persistent]
        private KeepFitCrewMember[] crewStore;
        internal Dictionary<string, KeepFitCrewMember> crew = new Dictionary<string, KeepFitCrewMember>();

        private KeepFitVesselRecord()
        {
        }

        internal KeepFitVesselRecord(string name, string id)
        {
            this.name = name;
            this.id = id;
        }


        public override void OnDecodeFromConfigNode()
        {
            // copy across the crew roster from persist
            {
                crew.Clear();
                if (crewStore != null)
                {
                    foreach (KeepFitCrewMember crewMember in crewStore)
                    {
                        crew[crewMember.Name] = crewMember;
                    }
                }
            }
        }

        public override void OnEncodeToConfigNode()
        {
            // copy across the crew roster to persist
            {
                crewStore = new KeepFitCrewMember[crew.Values.Count];
                crew.Values.CopyTo(crewStore, 0);
            }
        }

    }


    internal class Roster : ConfigNodeStorage
    {
        [Persistent]
        internal KeepFitVesselRecord available = new KeepFitVesselRecord("Available", null);

        [Persistent]
        internal KeepFitVesselRecord assigned = new KeepFitVesselRecord("Assigned", null);

        internal Dictionary<string, KeepFitVesselRecord> vessels = new Dictionary<string, KeepFitVesselRecord>();

        [Persistent]
        private KeepFitCrewMember[] crewStore;
        internal Dictionary<string, KeepFitCrewMember> crew = new Dictionary<string, KeepFitCrewMember>();

        internal Roster()
        {

        }

        public override void OnDecodeFromConfigNode()
        {
            // copy across the crew roster from persist
            {
                crew.Clear();
                if (crewStore != null)
                {
                    foreach (KeepFitCrewMember crewMember in crewStore)
                    {
                        crew[crewMember.Name] = crewMember;
                    }
                }
            }
        }

        public override void OnEncodeToConfigNode()
        {
            // copy across the crew roster to persist
            {
                crewStore = new KeepFitCrewMember[crew.Values.Count];
                crew.Values.CopyTo(crewStore, 0);
            }
        }
    }
}
