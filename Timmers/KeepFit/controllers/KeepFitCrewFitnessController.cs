using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KeepFit
{
    // <summary>
    /// Tracks and updates the fitness of crew in the roster
    /// </summary>
    public class KeepFitCrewFitnessController : KeepFitController
    {
        internal void Awake()
        {
            this.Log_DebugOnly("Awake", ".");

            if (shouldWeBeActive())
            {
                this.Log_DebugOnly("Awake", "Setting up repeating task to update fitness levels");
                InvokeRepeating("UpdateFitnessLevels", 1, 1);
            }
        }

        private Boolean shouldWeBeActive()
        {
            return (HighLogic.LoadedScene == GameScenes.FLIGHT ||
                HighLogic.LoadedScene == GameScenes.SPACECENTER ||
                HighLogic.LoadedScene == GameScenes.TRACKSTATION);
        }

        private void UpdateFitnessLevels()
        {
            if (gameConfig == null)
            {
                //too spammy
                //this.Log_DebugOnly("UpdateFitnessLevels", "No gameConfig - bailing");
                return;
            }

            if (!shouldWeBeActive())
            {
                // too spammy
                //this.Log_DebugOnly("UpdateFitnessLevels", "Not in flight scene - bailing");
                return;
            }   

            double currentUT = Planetarium.GetUniversalTime();

            // work out how long since the last refresh - we really need to store the lastUpdateUT in the persistence file
            // in order for this to work correctly
            float elapsed = (float)(currentUT - gameConfig.lastExerciseUT);

            this.Log_DebugOnly("UpdateFitnessLevels", "lastUpdateUT[{0}] currentUT[{1}] elapsed[{2}]", gameConfig.lastExerciseUT, currentUT, elapsed);

            if (!gameConfig.enabled)
            {
                this.Log_DebugOnly("UpdateFitnessLevels", "not enabled");

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
            this.Log_DebugOnly("exerciseKerbals", "timeSinceLastExercise[{0}]", timeSinceLastExercise);

            foreach (KeepFitCrewMember crewMember in gameConfig.roster.crew.Values)
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

        private float getFitnessModifier(ActivityLevel activityLevel, float elapsedSeconds)
        {
            float secondsPerDay = 60 * 60 * 24;

            float fitnessModifier;

            switch (activityLevel)
            {
                case ActivityLevel.CRAMPED:
                    // cramped - fitness goes down 5% per day by default
                    fitnessModifier = (gameConfig.degradationWhenCramped * elapsedSeconds) / secondsPerDay;
                    break;

                case ActivityLevel.COMFY:
                    // comfy - fitness goes down 1% per day by defaultt@
                    fitnessModifier = (gameConfig.degradationWhenComfy * elapsedSeconds) / secondsPerDay;
                    break;

                case ActivityLevel.NEUTRAL:
                    // neutral - fitness is static by default
                    fitnessModifier = (gameConfig.degradationWhenNeutral * elapsedSeconds) / secondsPerDay;
                    break;

                case ActivityLevel.EXERCISING:
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
    }
}
