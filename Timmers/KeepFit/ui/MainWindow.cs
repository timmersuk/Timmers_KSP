using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KeepFit
{
    class MainWindow : KeepFitInfoWindow
    {
        private Vector2 scrollPosition;


        public MainWindow()
            : base(true, false, "main")
        {
            this.WindowOptions = new GUILayoutOption[]
            {
                GUILayout.ExpandHeight(true)
            };
            this.WindowCaption = "KeepFit";
            this.Visible = false;
            this.DragEnabled = false;

            this.WindowRect = new Rect(Screen.width - 298, 40, 299, 20);
        }


        protected override void FillWindow(int id)
        {
           // GUILayout.ExpandHeight(true);
            GUILayout.BeginVertical();
            DrawStatuses(id);
            GUILayout.Space(4);
            DrawButtons(id);
            GUILayout.Space(4); 
            DrawActiveVessel(id);
            GUILayout.EndVertical();
        }

        private void DrawStatuses(int id)
        {
            GUILayout.BeginHorizontal();
            if (scenarioModule.isKeepFitEnabled())
            {
                GUILayout.Label(new GUIContent("Fitness"), (scenarioModule.isCrewFitnessControllerActive() ? uiResources.styleBarTextGreen : uiResources.styleBarTextRed));
                GUILayout.Label(new GUIContent("Roster"), (scenarioModule.isCrewRosterControllerActive() ? uiResources.styleBarTextGreen : uiResources.styleBarTextRed));
                GUILayout.Label(new GUIContent("GeeEffects"), (scenarioModule.isGeeEffectsControllerActive() ? uiResources.styleBarTextGreen : uiResources.styleBarTextRed));
            }
            else
            {
                GUILayout.Label(new GUIContent("DISABLED"), uiResources.styleBarTextRed);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawButtons(int id)
        {
            GUILayout.BeginHorizontal();
            if (scenarioModule.isKeepFitEnabled())
            {
                if (GUILayout.Button("Roster"))
                {
                    scenarioModule.ShowRoster();
                }
                if (GUILayout.Button("Vessels"))
                {
                    scenarioModule.ShowVessels();
                }
                if (GUILayout.Button("Log"))
                {
                    scenarioModule.ShowLog();
                }
            }
            else
            {
                GUILayout.Label(new GUIContent("DISABLED"), uiResources.styleBarTextRed);
            }
            if (GUILayout.Button("Settings"))
            {
                scenarioModule.ShowSettings();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }


        private void DrawActiveVessel(int id)
        {
            if (FlightGlobals.ActiveVessel == null || FlightGlobals.ActiveVessel.packed)
            {
                return;
            }

            KeepFitVesselRecord vessel;
            scenarioModule.GetGameConfig().roster.vessels.TryGetValue(FlightGlobals.ActiveVessel.id.ToString(), out vessel);
            if (vessel == null)
            {
                return;
            }
            
            //scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            bool expanded = true;
            DrawVesselInfo(id, vessel, false, ref expanded, true);
            //GUILayout.EndScrollView();
        }
    }
}
