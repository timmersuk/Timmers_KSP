using System;
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

        public void validate()
        {
            if (maxFitnessLevel < minFitnessLevel)
            {
                maxFitnessLevel = minFitnessLevel;
            }

            if (initialFitnessLevel > maxFitnessLevel)
            {
                initialFitnessLevel = maxFitnessLevel;
            }

            if (initialFitnessLevel < minFitnessLevel)
            {
                initialFitnessLevel = minFitnessLevel;
            }
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
        internal bool hasKeepFitPartModule;

        [Persistent]
        internal ActivityLevel activityLevel;

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

        [Persistent]
        private KeepFitVesselRecord[] vesselStore;
        internal Dictionary<string, KeepFitVesselRecord> vessels = new Dictionary<string, KeepFitVesselRecord>();

        [Persistent]
        private KeepFitCrewMember[] crewStore;
        internal Dictionary<string, KeepFitCrewMember> crew = new Dictionary<string, KeepFitCrewMember>();

        internal Roster()
        {

        }

        public override void OnDecodeFromConfigNode()
        {
            // copy across the vessels roster from persist
            {
                vessels.Clear();
                if (vesselStore != null)
                {
                    foreach (KeepFitVesselRecord vessel in vesselStore)
                    {
                        vessels[vessel.id] = vessel;
                    }
                }
            }
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
            // copy across the vessels roster to persist
            {
                vesselStore = new KeepFitVesselRecord[vessels.Values.Count];
                vessels.Values.CopyTo(vesselStore, 0);
            }

            // copy across the crew roster to persist
            {
                crewStore = new KeepFitCrewMember[crew.Values.Count];
                crew.Values.CopyTo(crewStore, 0);
            }
        }
    }
}
