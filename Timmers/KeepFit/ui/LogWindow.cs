using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KeepFit
{
    class LogWindow : KeepFitInfoWindow
    {
        private Vector2 scrollPosition;

        private KeepFitScenarioModule scenarioModule;

        public LogWindow()
        {
            this.WindowCaption = "KeepFit Log";
            this.Visible = false;
            this.DragEnabled = true;
            this.Resizable = true;

            this.WindowRect = new Rect(0, 0, 300, 300);
        }

        internal void Init(KeepFitScenarioModule scenarioModule)
        {
            this.scenarioModule = scenarioModule;
        }




        protected override void FillWindow(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Fitness"), (scenarioModule.isCrewFitnessControllerActive() ? uiResources.styleBarTextGreen : uiResources.styleBarTextRed));
            GUILayout.Label(new GUIContent("Roster"), (scenarioModule.isCrewRosterControllerActive() ? uiResources.styleBarTextGreen : uiResources.styleBarTextRed));
            GUILayout.Label(new GUIContent("GeeEffects"), (scenarioModule.isGeeEffectsControllerActive() ? uiResources.styleBarTextGreen : uiResources.styleBarTextRed));
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginVertical();
            foreach (String logLine in Logging.GetLogBuffer())
            {
                GUILayout.Label(logLine);
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }
}
