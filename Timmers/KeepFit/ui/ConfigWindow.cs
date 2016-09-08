using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KeepFit
{
    class ConfigWindow : SaveableWindow
    {
        KeepFitScenarioModule scenarioModule;

        internal class WipValue
        {
            internal string value = null;
            internal bool valid = false;
        }

        internal class WipValuePair
        {
            internal WipValue one = new WipValue();
            internal WipValue two = new WipValue();
        }


        WipValue wipMinimumLandedGeeForExcercising = new WipValue();
        WipValue wipInitialFitnessLevel = new WipValue();
        WipValue wipMinFitnessLevel = new WipValue();
        WipValue wipMaxFitnessLevel = new WipValue();
        internal Dictionary<Period, WipValuePair> wipGeeTolerances = new Dictionary<Period, WipValuePair>();

        public ConfigWindow()
            : base(true, true, "config")
        {
            this.WindowCaption = "KeepFit Config";
            this.Hide();
            this.DragEnabled = true;
            
            this.WindowRect = new Rect(0, 0, 400, 300);

            foreach (Period period in Enum.GetValues(typeof(Period)))
            {
                wipGeeTolerances[period] = new WipValuePair();
            }
        }

        internal void Init(KeepFitScenarioModule scenarioModule)
        {
            this.scenarioModule = scenarioModule;
        }

        internal override void DrawWindow(int id)
        {
            base.DrawWindow(id);

            GameConfig config = scenarioModule.GetGameConfig();

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("KeepFit v" + Statics.GetDllVersion(this) + " : " + (config.enabled ? "Enabled" : "Disabled"));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button((config.enabled ? "Disable" : "Enable"), GUILayout.Width(80)))
            {
                config.enabled = !config.enabled;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Choose a Skin");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("KSP Style", "Sets the style to be the default KSPStyle")))
                SkinsLibrary.SetCurrent(SkinsLibrary.DefSkinType.KSP);
            if (GUILayout.Button(new GUIContent("Unity Style", "Sets the style to be the default Unity Style")))
                SkinsLibrary.SetCurrent(SkinsLibrary.DefSkinType.Unity);
            GUILayout.EndHorizontal();

            config.wimpMode = GUILayout.Toggle(config.wimpMode, "WimpMode: ", GUILayout.Width(80));
            config.useBestPartOnVessel = GUILayout.Toggle(config.useBestPartOnVessel, "Use Best Overall Part On Vessel: ", GUILayout.Width(80));
            if (scenarioModule.GetCLS() != null)
            {
                config.applyCLSLimitsIfAvailable = GUILayout.Toggle(config.applyCLSLimitsIfAvailable, "Apply CLS limits: ", GUILayout.Width(80));
            }

            showFloatEditor("Min Gee For Exercising when Landed", ref wipMinimumLandedGeeForExcercising, ref config.minimumLandedGeeForExcercising, ref config, true);
            showFloatEditor("Initial fitness level", ref wipInitialFitnessLevel, ref config.initialFitnessLevel, ref config, true);
            showFloatEditor("Min fitness level", ref wipMinFitnessLevel, ref config.minFitnessLevel, ref config, true);
            showFloatEditor("Max fitness level", ref wipMaxFitnessLevel, ref config.maxFitnessLevel, ref config, true);


            foreach (Period period in Enum.GetValues(typeof(Period)))
            {
                GeeToleranceConfig geeToleranceConfig = config.GetGeeTolerance(period);
                WipValuePair wipValuePair = wipGeeTolerances[period];
                showFloatPairEditor("Tolerance (" + period + ")", "Warn", "Fatal", ref wipValuePair, ref geeToleranceConfig.warn, ref geeToleranceConfig.fatal, ref config);
            }

            GUILayout.EndVertical();
        }

        private void showFloatEditor(string text, ref WipValue wipValue, ref float value, ref GameConfig config, bool ownHoriztonal)
        {
            if (ownHoriztonal) GUILayout.BeginHorizontal();
            GUILayout.Label(text + " : " + ((!wipValue.valid && wipValue.value != null) ? "(" + value + ")" : ""));
            if (ownHoriztonal) GUILayout.FlexibleSpace();

            if (wipValue.value == null)
            {
                wipValue.value = value.ToString();
            }
            wipValue.value = GUILayout.TextField(wipValue.value, GUILayout.Width(40));

            float tempValue;
            wipValue.valid = float.TryParse(wipValue.value, out tempValue);

            if (ownHoriztonal) GUILayout.EndHorizontal();

            if (wipValue.valid)
            {
                value = tempValue;
                if (config.validate())
                {
                    wipValue.value = null;
                }
            }
        }


        private void showFloatPairEditor(string text, string text1, string text2, ref WipValuePair wipValuePair, ref float value1, ref float value2, ref GameConfig config)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(text + " : ");
            GUILayout.FlexibleSpace();
            showFloatEditor(text1, ref wipValuePair.one, ref value1, ref config, false);
            showFloatEditor(text2, ref wipValuePair.two, ref value2, ref config, false);
            GUILayout.EndHorizontal();
        }
    }
}
