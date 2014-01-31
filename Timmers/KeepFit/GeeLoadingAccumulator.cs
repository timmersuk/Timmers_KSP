using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeepFit
{
    /// <summary>
    /// Accumulator class for handling tracking of accumulated mean Gee loading.
    ///     /// 
    /// </summary>
    internal class GeeLoadingAccumulator : ConfigNodeStorage
    {
        [Persistent]
        internal float accumPeriodSeconds { get; private set; }

        [Persistent]
        private float currentGeeSecondsAccum;
        [Persistent]
        private float currentGeeSecondsElapsed;
        [Persistent]
        private float lastGeeMeanPerSecond;
        [Persistent]
        private bool lastValueValid;

        private GeeLoadingAccumulator()
        {

        }

        internal GeeLoadingAccumulator(float accumPeriodSeconds)
        {
            this.accumPeriodSeconds = accumPeriodSeconds;
        }

        internal void UpdatePeriod(float accumPeriodSeconds)
        {
            this.accumPeriodSeconds = accumPeriodSeconds;
            this.currentGeeSecondsElapsed = 0;
            this.currentGeeSecondsAccum = 0;
            this.lastValueValid = false;
        }

        /// <summary>
        /// Add on some more gee loading to the accumulator, 
        /// </summary>
        /// <param name="geeLoading"></param>
        /// <param name="elapsedSeconds"></param>
        /// <returns>True if the mean has rolled over</returns>
        internal bool AccumulateGeeLoading(float geeLoading, float elapsedSeconds, out float meanG)
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

                meanG = lastGeeMeanPerSecond;
                return true;
            }

            meanG = 0;
            return false;
        }

        internal float GetLastGeeMeanPerSecond()
        {
            return (lastValueValid ? lastGeeMeanPerSecond : 0);
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
