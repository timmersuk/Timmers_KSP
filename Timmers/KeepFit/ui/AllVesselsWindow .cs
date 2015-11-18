using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KSP;
using UnityEngine;

namespace KeepFit
{
    public partial class AllVesselsWindow : KeepFitInfoWindow
    {
        private Dictionary<string, bool> expandedVessels = new Dictionary<string, bool>();
        private Vector2 scrollPosition;

        /// <summary>
        /// Class Initializer
        /// </summary>
        public AllVesselsWindow()
            : base(true, true, "allvessels")
        {

        }


        /// <summary>
        /// Awake Event - when the DLL is loaded 
        /// </summary>
        internal override void Awake()
        {
            base.Awake();

            this.Log_DebugOnly("Awakening the {0}", _ClassName);

            this.WindowCaption = "KeepFit All Vessels";
            this.Hide();
            this.DragEnabled = true;
            this.TooltipsEnabled = true;

            this.WindowRect = new Rect(Screen.width - 298, 100, 299, 200); ;
        }

        protected override void FillWindow(int WindowHandle)
        {
            GameConfig gameConfig = scenarioModule.GetGameConfig();
            if (!gameConfig.enabled)
            {
                GUILayout.Label(new GUIContent("DISABLED"), uiResources.styleBarTextRed);
                return;
            }

            if (gameConfig.roster == null || gameConfig.roster.vessels == null)
            {
                return;
            }

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginVertical();
            foreach (KeepFitVesselRecord vessel in gameConfig.roster.vessels.Values)
            {
                bool expanded;
                this.expandedVessels.TryGetValue(vessel.id, out expanded);

                DrawVesselInfo(WindowHandle, vessel, true, ref expanded, false);
                if (expanded)
                {
                    this.expandedVessels[vessel.id] = true;
                }
                else
                {
                    this.expandedVessels.Remove(vessel.id);
                }
            }
             
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
    }
}
