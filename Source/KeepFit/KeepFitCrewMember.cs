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

        [Persistent] internal GeeLoadingAccumulator instantaniousGeeLoadingAccumulator = new GeeLoadingAccumulator(5); // 5 seconds
        [Persistent] internal GeeLoadingAccumulator shortTermGeeLoadingAccumulator = new GeeLoadingAccumulator(5 * 60); // 5 minutes
        [Persistent] internal GeeLoadingAccumulator mediumTermGeeLoadingAccumulator = new GeeLoadingAccumulator(60 * 60); // 1 hour
        [Persistent] internal GeeLoadingAccumulator longTermGeeLoadingAccumulator = new GeeLoadingAccumulator(60 * 60 * 24); // 1 day

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

        internal void accumulatGeeLoading(float geeLoading, float elapsedSeconds)
        {
            instantaniousGeeLoadingAccumulator.accumulateGeeLoading(geeLoading, elapsedSeconds);
            shortTermGeeLoadingAccumulator.accumulateGeeLoading(geeLoading, elapsedSeconds);
            mediumTermGeeLoadingAccumulator.accumulateGeeLoading(geeLoading, elapsedSeconds);
            longTermGeeLoadingAccumulator.accumulateGeeLoading(geeLoading, elapsedSeconds);
        }
    }


    /// <summary>
    /// Accumulator class for handling tracking of accumulated mean Gee loading.
    ///     /// 
    /// </summary>
    internal class GeeLoadingAccumulator
    {
        [Persistent] private readonly float accumPeriodSeconds;

        [Persistent] internal float currentGeeSecondsAccum { get; private set;  }
        [Persistent] internal float currentGeeSecondsElapsed { get; private set; }
        [Persistent] internal float lastGeeMeanPerSecond { get; private set; }
        [Persistent] internal bool lastValueValid { get; private set; }

        internal GeeLoadingAccumulator(float accumPeriodSeconds)
        {
            this.accumPeriodSeconds = accumPeriodSeconds;
        }

        internal void accumulateGeeLoading(float geeLoading, float elapsedSeconds)
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
            }
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
