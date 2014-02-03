using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KeepFit
{
    class RosterWindow : SaveableWindow
    {
        internal GameConfig gameConfig { get; set; }
        internal ConfigWindow configWindow;
        
        private Vector2 scrollPosition;

        public RosterWindow()
        {
            this.WindowCaption = "KeepFit Roster";
            this.Visible = false;
            this.DragEnabled = true;
            this.Resizable = true;

            this.WindowRect = new Rect(0, 0, 300, 300);
        }

        internal override void DrawWindow(int id)
        {
            base.DrawWindow(id);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginVertical();
            GUILayout.Space(4);
            if (GUILayout.Button("Configure", GUILayout.Width(80)))
            {
                configWindow.Visible = !configWindow.Visible;
            }

            GUILayout.Space(10);
            if (gameConfig.roster.available != null)
            {
                DrawRoster("Available", gameConfig.roster.available.crew.Values, false);
            }

            GUILayout.Space(10);
            if (gameConfig.roster.assigned != null)
            {
                DrawRoster("Assigned", gameConfig.roster.assigned.crew.Values, false);
            }

            if (gameConfig.roster.vessels.Count > 0)
            {
                GUILayout.Space(10);
                foreach (KeepFitVesselRecord vessel in gameConfig.roster.vessels.Values)
                {
                    DrawRoster("Vessel : " + vessel.name + " (" + vessel.activityLevel + ")", vessel.crew.Values, true);
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void DrawRoster(string title, ICollection<KeepFitCrewMember> crew, bool showGeeLoadings)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(title);
            GUILayout.EndHorizontal();

            foreach (KeepFitCrewMember crewInfo in crew)
            {
                GUILayout.Label("Name: " + crewInfo.Name ); 
                GUILayout.Label("Fitness Level: " + crewInfo.fitnessLevel);
                GUILayout.Label("Activity Level: " + crewInfo.activityLevel);

                if (showGeeLoadings)
                {
                    foreach (Period period in Enum.GetValues(typeof(Period)))
                    {
                        GeeLoadingAccumulator accum;
                        crewInfo.geeAccums.TryGetValue(period, out accum);
                        if (accum != null)
                        {
                            GUIStyle style = getGeeAccumStyle(crewInfo, period, accum);

                            GUILayout.Label("G(" + period + "[" + accum.accumPeriodSeconds + "]): " + accum.GetLastGeeMeanPerSecond(), style);
                        }
                    }
                }
                GUILayout.Space(4);
            }
        }

        private GUIStyle getGeeAccumStyle(KeepFitCrewMember crew, Period period, GeeLoadingAccumulator accum)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = Color.green;
            style.wordWrap = false;

            GeeToleranceConfig tolerance = gameConfig.GetGeeTolerance(period);
            if (tolerance == null)
            {
                return style;
            }

            float geeWarn = GeeLoadingCalculator.GetFitnessModifiedGeeTolerance(tolerance.warn, crew, gameConfig);
            float geeFatal = GeeLoadingCalculator.GetFitnessModifiedGeeTolerance(tolerance.fatal, crew, gameConfig);

            float gee = accum.GetLastGeeMeanPerSecond();
            if (gee > geeFatal)
            {
                style.normal.textColor = Color.red;
            }
            else
            {
                if (gee > geeWarn)
                {
                    style.normal.textColor = Color.yellow;
                }
            }

            return style;
        }
    }
}
