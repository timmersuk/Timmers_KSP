using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeepFit
{
    internal class ActivityLevelFloatValue
    {
        [Persistent]
        internal float time;

        [Persistent]
        internal ActivityLevel level;

        internal ActivityLevelFloatValue()
        {
        }

        internal ActivityLevelFloatValue(ActivityLevel level, float time)
        {
            this.level = level;
            this.time = time;
        }
    }

    internal class GeeLoadingAccumulatorAndPeriod : ConfigNodeStorage
    {
        [Persistent]
        internal Period period;

        [Persistent]
        internal GeeLoadingAccumulator accum;

        internal GeeLoadingAccumulatorAndPeriod()
        {
        }

        internal GeeLoadingAccumulatorAndPeriod(Period period, GeeLoadingAccumulator accum)
        {
            this.period = period;
            this.accum = accum;
        }
    }

    public class FloatAndActivityLevel : ConfigNodeStorage
    {
        [Persistent]
        internal ActivityLevel level;

        [Persistent]
        internal float value;

        internal FloatAndActivityLevel()
        {
        }

        public FloatAndActivityLevel(ActivityLevel level, float value)
        {
            this.level = level;
            this.value = value;
        }
    }

    /// <summary>
    /// Retains KeepFit information over and above the core ProtoCrewMember information
    /// </summary>
    public class KeepFitCrewMember : ConfigNodeStorage
    {
        internal bool loaded;

        [Persistent] internal String Name;
        //[Persistent] internal KeepFitVesselRecord vessel;
        [Persistent] internal ActivityLevel activityLevel;
        [Persistent] internal Single fitnessLevel;

        [Persistent]
        private FloatAndActivityLevel[] timesStore;
        internal Dictionary<ActivityLevel, float> times = new Dictionary<ActivityLevel, float>();

        [Persistent]
        private GeeLoadingAccumulatorAndPeriod[] geeAccumsStore;
        internal Dictionary<Period, GeeLoadingAccumulator> geeAccums = new Dictionary<Period, GeeLoadingAccumulator>();


        private static Dictionary<Period, GeeLoadingAccumulator> GetDefaultGeeAccums()
        {
            Dictionary<Period, GeeLoadingAccumulator> def = new Dictionary<Period, GeeLoadingAccumulator>();

            def[Period.Inst] = new GeeLoadingAccumulator(1); // 1 second;
            def[Period.Short] = new GeeLoadingAccumulator(5); // 5 seconds
            def[Period.Medium] = new GeeLoadingAccumulator(1 * 60); // 1 minutes
            def[Period.Long] = new GeeLoadingAccumulator(5 * 60); // 5 minutes

            return def;
        }

        public KeepFitCrewMember()
        {
            loaded = true;

            foreach (ActivityLevel level in Enum.GetValues(typeof(ActivityLevel)))
            {
                times[level] = 0;
            }

            geeAccums = GetDefaultGeeAccums();
        }

        public KeepFitCrewMember(string name, bool loaded)
            : this()
        {
            loaded = false;
            Name = name;
        }

        internal void AddTime(float elapsed)
        {
            float existing = 0;

            times.TryGetValue(activityLevel, out existing);
            times[activityLevel] = existing + elapsed;
        }


        public override void OnDecodeFromConfigNode()
        {
            // copy across the times from persist
            {
                times.Clear();
                if (timesStore != null)
                {
                    foreach (FloatAndActivityLevel timeAndActivityLevel in timesStore)
                    {
                        times[timeAndActivityLevel.level] = timeAndActivityLevel.value;
                    }
                }
            }

            // copy across the G accums from persist
            {
                geeAccums.Clear();
                if (geeAccumsStore != null)
                {
                    foreach (GeeLoadingAccumulatorAndPeriod geeAccumAndPeriod in geeAccumsStore)
                    {
                        geeAccums[geeAccumAndPeriod.period] = geeAccumAndPeriod.accum;
                    }
                }
                
                Dictionary<Period, GeeLoadingAccumulator> def = GetDefaultGeeAccums();
                foreach (Period period in Enum.GetValues(typeof(Period)))
                {
                    GeeLoadingAccumulator accum;
                    geeAccums.TryGetValue(period, out accum);

                    if (accum == null)
                    {
                        GeeLoadingAccumulator defAccum;
                        def.TryGetValue(period, out defAccum);
                        if (defAccum != null)
                        {
                            geeAccums[period] = defAccum;
                        }
                    }
                    else if (accum.accumPeriodSeconds == 0)
                    {
                        GeeLoadingAccumulator defAccum;
                        def.TryGetValue(period, out defAccum);
                        if (defAccum != null)
                        {
                            accum.UpdatePeriod(defAccum.accumPeriodSeconds);
                        }
                    }
                }
            }
        }


        public override void OnEncodeToConfigNode()
        {
            // copy across the times to persist
            {
                List<FloatAndActivityLevel> temp = new List<FloatAndActivityLevel>();
                foreach (KeyValuePair<ActivityLevel, float> key in times)
                {
                    temp.Add(new FloatAndActivityLevel(key.Key, key.Value));
                }

                timesStore = new FloatAndActivityLevel[temp.Count];
                temp.CopyTo(timesStore);
            }

            // copy across the G accums to persist
            {
                List<GeeLoadingAccumulatorAndPeriod> temp = new List<GeeLoadingAccumulatorAndPeriod>();
                foreach (KeyValuePair<Period, GeeLoadingAccumulator> key in geeAccums)
                {
                    temp.Add(new GeeLoadingAccumulatorAndPeriod(key.Key, key.Value));
                }

                geeAccumsStore = new GeeLoadingAccumulatorAndPeriod[temp.Count];
                temp.CopyTo(geeAccumsStore);
            }
        }
    }
}
