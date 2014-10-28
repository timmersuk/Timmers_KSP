using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KSP;
using UnityEngine;

namespace KeepFit
{
    public partial class InFlightActiveVesselWindow : KeepFitInfoWindow
    {
        internal AllVesselsWindow allVesselsWindow;
        internal ConfigWindow configWindow;
        private bool drawConfig = false;

        /// <summary>
        /// Class Initializer
        /// </summary>
        public InFlightActiveVesselWindow()
        {

        }


        /// <summary>
        /// Awake Event - when the DLL is loaded 
        /// </summary>
        internal override void Awake()
        {
            base.Awake();

            this.Log_DebugOnly("Awake", ".");

            this.WindowCaption = "KeepFit Active Vessel";
            this.Visible = false;
            this.DragEnabled = true;
            //this.Resizable = false;
            this.TooltipsEnabled = true;

            this.WindowRect = new Rect(Screen.width - 298, 100, 299, 20); ;
        }

        protected override void FillWindow(int WindowHandle)
        {
            if (FlightGlobals.ActiveVessel == null)
            {
                return;
            }

            GUILayout.BeginVertical();

            KeepFitVesselRecord vessel;
            gameConfig.roster.vessels.TryGetValue(FlightGlobals.ActiveVessel.id.ToString(), out vessel);
            if (vessel == null)
            {
                //What will the height of the panel be
                this.WindowRect.height = uiResources.btnChevronUp.height + 12;
            }
            else
            {
                this.WindowRect.height = 20 // title
                                + 30 // vessel name / expand label
                                + vessel.crew.Count * 30 // crewmember names
                                + expandedCrew.Count * (100)
                                + 40; // config chevron + other bubbins

                if (this.drawConfig)
                {
                    this.WindowRect.height += this.configWindow.WindowRect.height;
                }
                
                bool expanded = true;
                DrawVesselInfo(WindowHandle, vessel, false, ref expanded, false);
            }


            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("View All", uiResources.styleButtonSettings))
                {
                    this.allVesselsWindow.Visible = !this.allVesselsWindow.Visible;
                }

                GUILayout.FlexibleSpace();
                GUIContent btnMinMax;
                if (!this.drawConfig)
                {
                    btnMinMax = new GUIContent(uiResources.btnChevronDown, "Show Settings...");
                }
                else
                {
                    btnMinMax = new GUIContent(uiResources.btnChevronUp, "Hide Settings");
                }

                if (GUILayout.Button(btnMinMax, uiResources.styleButtonSettings))
                {
                    this.drawConfig = !this.drawConfig;
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();

            if (drawConfig)
            {
                this.configWindow.DrawWindow(WindowHandle);
            }
        }
    }
}
