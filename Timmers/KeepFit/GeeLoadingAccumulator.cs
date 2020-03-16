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
            this.lastValueValid = false;
        }

        /// <summary>
        /// Add on some more gee loading to the accumulator, 
        /// </summary>
        /// <param name="geeLoading"></param>
        /// <param name="elapsedSeconds"></param>
        /// <returns>True if the mean has rolled over</returns>
        internal bool AccumulateGeeLoading(float geeLoading, out float meanG)
        {
            lastGeeMeanPerSecond = geeLoading;
            meanG = lastGeeMeanPerSecond;
            lastValueValid = true;
            return true;
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
