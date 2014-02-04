using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KSP;
using UnityEngine;

namespace KeepFit
{
    public partial class InFlightActiveVesselWindow : SaveableWindow
    {
        //GlobalSettings
        private UIResources uiResources = new UIResources();
        internal GameConfig gameConfig { get; set; }
        internal ConfigWindow configWindow;

        bool drawConfig = false;

        int intLineHeight = 20;
        
        /// <summary>
        /// Class Initializer
        /// </summary>
        public InFlightActiveVesselWindow()
        {
            this.WindowCaption = "KeepFit Active Vessel";
            this.Visible = false;
            this.DragEnabled = true;
            //this.Resizable = false;
            this.TooltipsEnabled = true;

            this.WindowRect = new Rect(Screen.width - 298, 20, 299, 20); ;
        }


        /// <summary>
        /// Awake Event - when the DLL is loaded 
        /// </summary>
        internal override void Awake()
        {
            this.Log_DebugOnly("Awakening the {0}", _ClassName);

           
            //Load Textures
            uiResources.LoadTextures();
        }

        private static Boolean blnResetWindow = false;

        /// <summary>
        /// This is what we do every frame when the object is being drawn 
        /// We dont get here unless we are in the postdraw queue
        /// </summary>
        internal override void DrawWindow(int id)
        {
            //base.DrawWindow(id);

            // draw here
            //Check for loaded Textures, etc
            if (!uiResources.DrawStuffConfigured)
            {
                uiResources.SetupDrawStuff();
            }

            //Are there any resources left and the window is displayed?
            if (blnResetWindow)
            {
                WindowRect = new Rect(Screen.width - 298, 100, 299, 20);
                blnResetWindow = false;
            }

            FillWindow(id);
        }

        private void FillWindow(int WindowHandle)
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
                this.WindowRect.height = ((vessel.crew.Count() + 1) * intLineHeight * (1 + Enum.GetValues(typeof(Period)).Length)) + 12;
                GUILayout.BeginHorizontal();
                GUILayout.Label(vessel.name);
                Texture2D texActivityLevel;
                uiResources.texIconsActivityLevels.TryGetValue(vessel.activityLevel, out texActivityLevel);
                if (texActivityLevel == null)
                {
                    GUILayout.Label("(" + vessel.activityLevel + ")");
                }
                else
                {
                    GUILayout.Label(new GUIContent(uiResources.texIconsActivityLevels[vessel.activityLevel], vessel.activityLevel.ToString()));
                }
                GUILayout.Label(" x" + vessel.crew.Count());
                GUILayout.FlexibleSpace();
                // TDXX - add button to show/hide crew info here
                GUILayout.EndHorizontal();

                GUILayout.Space(4);
                foreach (KeepFitCrewMember crewMember in vessel.crew.Values)
                {

                    // first line - crewmember name
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(crewMember.Name, uiResources.styleCrewName);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(4);
                    // second line - fitness current / max
                    DrawBar2("Fit", "Current / max Fitness level", (float)crewMember.fitnessLevel, (float)gameConfig.maxFitnessLevel);
                    GUILayout.Space(2);

                    //// 3rd to <n>th line - Gee(inst) current / max
                    foreach (Period period in Enum.GetValues(typeof(Period)))
                    {
                        GeeToleranceConfig tolerance = gameConfig.GetGeeTolerance(period);
                        GeeLoadingAccumulator accum;
                        crewMember.geeAccums.TryGetValue(period, out accum);

                        if (accum != null && tolerance != null)
                        {
                            float geeWarn = GeeLoadingCalculator.GetFitnessModifiedGeeTolerance(tolerance.warn, crewMember, gameConfig);
                            float geeFatal = GeeLoadingCalculator.GetFitnessModifiedGeeTolerance(tolerance.fatal, crewMember, gameConfig);

                            string label = "G(" + period.ToString().Substring(0, 1) + ")";
                            string tooltip = "Gee(" + period + ") Current / Max - Orange warn, Red fatal";
                            float level = accum.GetLastGeeMeanPerSecond();
                            float max = geeFatal;//(level < geeFatal ? geeFatal : geeFatal + 20);
                            DrawBar3(label, tooltip, level, geeWarn, geeFatal, max);
                            GUILayout.Space(1);
                        }
                    }
                }
            }

            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUIContent btnMinMax;
                if (!this.configWindow.Visible)
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

        private void DrawBar2(string label, string tooltip, float level, float max)
        {
            GUILayout.BeginHorizontal();
            GUIContent contLabel = new GUIContent(label, tooltip);
            GUILayout.Label(contLabel, uiResources.styleBarName);
            GUILayout.Space(4);

            //set ratio for remaining resource value
            float fltBarRemainRatio = level / max;

            //For resources with no stage specifics
            //full width bar
            Rect rectBar = DrawBar(uiResources.styleBarGreen_Back, 245);
            if ((rectBar.width * fltBarRemainRatio) > 1)
            {
                DrawBarScaled(rectBar, uiResources.styleBarGreen, uiResources.styleBarGreen_Thin, fltBarRemainRatio);
            }

            //add amounts
            DrawUsage2(rectBar, level, max);
            GUILayout.EndHorizontal();
        }

        private void DrawBar3(string label, string tooltip, float level, float orangeLevel, float redLevel, float max)
        {
            GUILayout.BeginHorizontal();
            GUIContent contLabel = new GUIContent(label, tooltip);
            GUILayout.Label(contLabel, uiResources.styleBarName);
            GUILayout.Space(4);

            GUIStyle styleBar;
            GUIStyle styleBar_Thin;
            GUIStyle styleBar_back;
            if (level > redLevel)
            {
                styleBar = uiResources.styleBarRed;
                styleBar_Thin = uiResources.styleBarRed_Thin;
                styleBar_back = uiResources.styleBarRed_Back;
            }
            else if (level > orangeLevel)
            {
                styleBar = uiResources.styleBarOrange;
                styleBar_Thin = uiResources.styleBarOrange_Thin;
                styleBar_back = uiResources.styleBarOrange_Back;
            }
            else 
            {
                styleBar = uiResources.styleBarGreen;
                styleBar_Thin = uiResources.styleBarGreen_Thin;
                styleBar_back = uiResources.styleBarGreen_Back;
            }

            //set ratio for remaining resource value
            float fltBarRemainRatio = level / max;

            //For resources with no stage specifics
            //full width bar
            Rect rectBar = DrawBar(styleBar_back, 245);
            if ((rectBar.width * fltBarRemainRatio) > 1)
            {
                DrawBarScaled(rectBar, styleBar, styleBar_Thin, fltBarRemainRatio);
            }

            //add amounts
            DrawUsage3(rectBar, level, orangeLevel, redLevel);
            GUILayout.EndHorizontal();
        }

        private Rect DrawBar(GUIStyle Style, int Width = 0, int Height = 0)
        {
            List<GUILayoutOption> Options = new List<GUILayoutOption>();
            if (Width == 0)
            {
                Options.Add(GUILayout.ExpandWidth(true));
            }
            else
            {
                Options.Add(GUILayout.Width(Width));
            }
            if (Height != 0)
            {
                Options.Add(GUILayout.Height(Height));
            }
            GUILayout.Label("", Style, Options.ToArray());

            return GUILayoutUtility.GetLastRect();
        }

        private void DrawBar(Rect rectStart, int Row, GUIStyle Style)
        {
            GUI.Label(rectStart, "", Style);
        }

        private void DrawBarScaled(Rect rectStart, GUIStyle Style, GUIStyle StyleNarrow, float Scale)
        {
            Rect rectTemp = new Rect(rectStart);
            rectTemp.width = (float)Math.Ceiling(rectTemp.width = rectTemp.width * Scale);
            if (rectTemp.width <= 2)
            {
                Style = StyleNarrow;
            }
            GUI.Label(rectTemp, "", Style);
        }

        private void DrawUsage2(Rect rectStart, float level, float max)
        {
            Rect rectTemp = new Rect(rectStart);

            //if (blnShowInstants && (rectStart.width < 180)) rectTemp.width = (rectTemp.width * 2 / 3);
            rectTemp.x -= 20;
            rectTemp.width += 40;

            GUI.Label(rectTemp, string.Format("{0} / {1}", DisplayValue(level), DisplayValue(max)), uiResources.styleBarText);
        }

        private void DrawUsage3(Rect rectStart, float level, float warn, float fatal)
        {
            Rect rectTemp = new Rect(rectStart);

            //if (blnShowInstants && (rectStart.width < 180)) rectTemp.width = (rectTemp.width * 2 / 3);
            rectTemp.x -= 20;
            rectTemp.width += 40;

            GUI.Label(rectTemp, string.Format("{0}. W {1}, F{2}", DisplayValue(level), DisplayValue(warn), DisplayValue(fatal)), uiResources.styleBarText);
        }

        private void DrawRate(Rect rectStart, int Row, float Rate)
        {
            Rect rectTemp = new Rect(rectStart) { width = rectStart.width - 2 };
            GUI.Label(rectTemp, string.Format("({0})", DisplayValue(Rate)), uiResources.styleBarRateText);
        }

        private string DisplayValue(float Amount)
        {
            String strFormat = "{0:0}";
            if (Amount < 100)
                strFormat = "{0:0.00}";
            return string.Format(strFormat, Amount);
        }





        #region "Control Drawing"
        /// <summary>
        /// Draws a Toggle Button and sets the boolean variable to the state of the button
        /// </summary>
        /// <param name="blnVar">Boolean variable to set and store result</param>
        /// <param name="ButtonText"></param>
        /// <param name="style"></param>
        /// <param name="options"></param>
        /// <returns>True when the button state has changed</returns>
        public Boolean DrawToggle(ref Boolean blnVar, String ButtonText, GUIStyle style, params GUILayoutOption[] options)
        {
            Boolean blnReturn = GUILayout.Toggle(blnVar, ButtonText, style, options);

            return ToggleResult(ref blnVar, ref  blnReturn);
        }

        public Boolean DrawToggle(ref Boolean blnVar, Texture image, GUIStyle style, params GUILayoutOption[] options)
        {
            Boolean blnReturn = GUILayout.Toggle(blnVar, image, style, options);

            return ToggleResult(ref blnVar, ref blnReturn);
        }

        public Boolean DrawToggle(ref Boolean blnVar, GUIContent content, GUIStyle style, params GUILayoutOption[] options)
        {
            Boolean blnReturn = GUILayout.Toggle(blnVar, content, style, options);

            return ToggleResult(ref blnVar, ref blnReturn);
        }

        private Boolean ToggleResult(ref Boolean Old, ref Boolean New)
        {
            if (Old != New)
            {
                Old = New;
                this.Log_DebugOnly("ToggleResult", "Toggle Changed:" + New.ToString());
                return true;
            }
            return false;
        }
        #endregion
    }
}
