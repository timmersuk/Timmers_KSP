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
        internal float degradationWhenTooCrampedToMove = -10.0f;
        [Persistent]
        internal float degradationWhenCramped = -5.0f;
        [Persistent]
        internal float degradationWhenComfy = -1.0f;
        [Persistent]
        internal float degradationWhenNeutral = -0.0f;
        [Persistent]
        internal float degradationWhenExercising = 1.0f;

        [Persistent]
        private KeepFitCrewMember[] knownCrewStore;
        internal Dictionary<string, KeepFitCrewMember> knownCrew = new Dictionary<string, KeepFitCrewMember>();

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
            // copy across the crew roster from persist
            {
                knownCrew.Clear();
                if (knownCrewStore != null)
                {
                    foreach (KeepFitCrewMember crewMember in knownCrewStore)
                    {
                        knownCrew[crewMember.Name] = crewMember;
                    }
                }
            }

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
            // copy across the crew roster to persist
            {
                knownCrewStore = new KeepFitCrewMember[knownCrew.Values.Count];
                knownCrew.Values.CopyTo(knownCrewStore, 0);
            }

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
}
