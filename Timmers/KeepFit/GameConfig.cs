using System;
using System.Collections.Generic;
using UnityEngine;

namespace KeepFit
{
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
        internal KeepFitCrewMember[] knownCrewStore;

        internal Dictionary<string, KeepFitCrewMember> knownCrew = new Dictionary<string, KeepFitCrewMember>();

        public GameConfig()
            : base("KeepFitSavedGameSettings")
        {
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
        }


        public override void OnEncodeToConfigNode()
        {
            // copy across the crew roster to persist
            {
                knownCrewStore = new KeepFitCrewMember[knownCrew.Values.Count];
                knownCrew.Values.CopyTo(knownCrewStore, 0);
            }
        }
    }
}
