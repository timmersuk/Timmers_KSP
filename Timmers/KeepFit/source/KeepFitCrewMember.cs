using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeepFit
{
    public enum KeepFitActivityLevel
    {
        UNKNOWN,
        TOOCRAMPEDTOMOVE,
        CRAMPED,
        COMFY,
        NEUTRAL,
        EXERCISING,
    }

    internal class ActivityLevelFloatValue
    {
        [Persistent]
        internal float time;

        [Persistent]
        internal KeepFitActivityLevel level;

        internal ActivityLevelFloatValue(KeepFitActivityLevel level, float time)
        {
            this.level = level;
            this.time = time;
        }
    }

    /// <summary>
    /// Retains KeepFit information over and above the core ProtoCrewMember information
    /// </summary>
    public class KeepFitCrewMember : ConfigNodeStorage
    {
        [Persistent] internal String Name;
        [Persistent] internal String vesselName;
        [Persistent] internal KeepFitActivityLevel activityLevel;
        [Persistent] internal Single fitnessLevel;

        [Persistent] private float timeTooCrampedToMove;
        [Persistent] private float timeCramped;
        [Persistent] private float timeComfy;
        [Persistent] private float timeNeutral;
        [Persistent] private float timeExercising;

        [Persistent] internal GeeLoadingAccumulator instantaniousGeeLoadingAccumulator = new GeeLoadingAccumulator(1); // 1 second
        [Persistent] internal GeeLoadingAccumulator shortTermGeeLoadingAccumulator = new GeeLoadingAccumulator(5); // 5 seconds
        [Persistent] internal GeeLoadingAccumulator mediumTermGeeLoadingAccumulator = new GeeLoadingAccumulator(5 * 60); // 5 minutes
        [Persistent] internal GeeLoadingAccumulator longTermGeeLoadingAccumulator = new GeeLoadingAccumulator(60 * 60); // 1 hour

        public KeepFitCrewMember()
        {
        }

        public KeepFitCrewMember(string name)
            : this()
        {
            Name = name;
        }

        internal void AddTime(float elapsed)
        {
            switch (activityLevel)
            {
                case KeepFitActivityLevel.TOOCRAMPEDTOMOVE:
                    timeTooCrampedToMove += elapsed;
                    break;
                case KeepFitActivityLevel.CRAMPED:
                    timeCramped += elapsed;
                    break;
                case KeepFitActivityLevel.COMFY:
                    timeComfy += elapsed;
                    break;
                case KeepFitActivityLevel.NEUTRAL:
                    timeNeutral += elapsed;
                    break;
                case KeepFitActivityLevel.EXERCISING:
                    timeExercising += elapsed;
                    break;
            }
        }

        internal class ConsequencesGeeToleranceValidator : GeeToleranceValidator
        {
            private KeepFitCrewMember crewMember;
            private GeeLoadingConsequencesHandler consequencesHandler;

            internal ConsequencesGeeToleranceValidator(KeepFitCrewMember crewMember, GeeLoadingConsequencesHandler consequencesHandler)
            {
                this.crewMember = crewMember;
                this.consequencesHandler = consequencesHandler;
            }

            public void onGeeMeanRollover(float meanGee)
            {
                if (meanGee > 15) // TDXX parameterise this, and also modify based on crew fitness level
                {
                    consequencesHandler.onGeeFatal(crewMember);
                }
                else if (meanGee > 10) // TDXX parameterise this, and also modify based on crew fitness level
                {
                    consequencesHandler.onGeeLOC(crewMember);
                }
                else if (meanGee > 5) // TDXX parameterise this, and also modify based on crew fitness level
                {
                    consequencesHandler.onGeeWarn(crewMember);
                }
            }
        }

        internal void accumulatGeeLoading(float geeLoading, float elapsedSeconds, GeeLoadingConsequencesHandler consequencesHandler)
        {
            ConsequencesGeeToleranceValidator validator = new ConsequencesGeeToleranceValidator(this, consequencesHandler);

            instantaniousGeeLoadingAccumulator.AccumulateGeeLoading(geeLoading, elapsedSeconds, validator);
            shortTermGeeLoadingAccumulator.AccumulateGeeLoading(geeLoading, elapsedSeconds, validator);
            mediumTermGeeLoadingAccumulator.AccumulateGeeLoading(geeLoading, elapsedSeconds, validator);
            longTermGeeLoadingAccumulator.AccumulateGeeLoading(geeLoading, elapsedSeconds, validator);
        }


    }

    internal interface GeeToleranceValidator
    {
        void onGeeMeanRollover(float meanGee);
    }

    internal interface GeeLoadingConsequencesHandler
    {
        void onGeeWarn(KeepFitCrewMember crewMember);
        void onGeeLOC(KeepFitCrewMember crewMember);
        void onGeeFatal(KeepFitCrewMember crewMember);
    }

    /// <summary>
    /// Accumulator class for handling tracking of accumulated mean Gee loading.
    ///     /// 
    /// </summary>
    internal class GeeLoadingAccumulator
    {
        /*[Persistent]*/ private readonly float accumPeriodSeconds;

        [Persistent] private float currentGeeSecondsAccum;
        [Persistent] private float currentGeeSecondsElapsed;
        [Persistent] private float lastGeeMeanPerSecond;
        [Persistent] private bool lastValueValid;

        internal GeeLoadingAccumulator(float accumPeriodSeconds)
        {
            this.accumPeriodSeconds = accumPeriodSeconds;
        }

        /// <summary>
        /// Add on some more gee loading to the accumulator, 
        /// </summary>
        /// <param name="geeLoading"></param>
        /// <param name="elapsedSeconds"></param>
        /// <returns>True if the mean has rolled over</returns>
        internal void AccumulateGeeLoading(float geeLoading, float elapsedSeconds, GeeToleranceValidator validator)
        {
            // turn the gee loading into geeseconds accum and accumulate into the current buffer
            currentGeeSecondsAccum += (geeLoading * elapsedSeconds);
            currentGeeSecondsElapsed += elapsedSeconds;

            // if the sum in currentSeconds > the size limit, then propagate it and clear down the current.
            if (currentGeeSecondsElapsed > accumPeriodSeconds)
            {
                lastGeeMeanPerSecond = currentGeeSecondsAccum / currentGeeSecondsElapsed;
                lastValueValid = true;
                currentGeeSecondsAccum = 0;
                currentGeeSecondsElapsed = 0;

                validator.onGeeMeanRollover(lastGeeMeanPerSecond);
            }
        }

        internal float GetLastGeeMeanPerSecond()
        {
            return lastGeeMeanPerSecond;
        }

        public override string ToString()
        {
            if (!lastValueValid)
            {
                return "Period[" + accumPeriodSeconds + "] invalid[" + currentGeeSecondsElapsed + "]";
            }

            return "Period[" + accumPeriodSeconds + "]," + lastGeeMeanPerSecond;
        }
    }
}
