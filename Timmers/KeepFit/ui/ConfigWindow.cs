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

        public ConfigWindow()
            : base(true, true, "config")
        {
            this.WindowCaption = "KeepFit Config";
            this.Visible = false;
            this.DragEnabled = true;
            
            this.WindowRect = new Rect(0, 0, 400, 300);
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

            GUILayout.BeginHorizontal();
            GUILayout.Label("WimpMode: " + (config.wimpMode ? "Enabled" : "Disabled"));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button((config.wimpMode ? "Disable" : "Enable"), GUILayout.Width(80)))
            {
                config.wimpMode = !config.wimpMode;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Initial fitness level:" + config.initialFitnessLevel.ToString());
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+5", GUILayout.Width(80)))
            {
                config.initialFitnessLevel += 5;
                config.validate();
            }
            if (GUILayout.Button("-5", GUILayout.Width(80)))
            {
                config.initialFitnessLevel -= 5;
                config.validate();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Min fitness level:" + config.minFitnessLevel.ToString());
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+5", GUILayout.Width(80)))
            {
                config.minFitnessLevel += 5;
                config.validate();
            }
            if (GUILayout.Button("-5", GUILayout.Width(80)))
            {
                config.minFitnessLevel -= 5;
                config.validate();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Max fitness level:" + config.maxFitnessLevel.ToString());
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+5", GUILayout.Width(80)))
            {
                config.maxFitnessLevel += 5;
                config.validate();
            }
            if (GUILayout.Button("-5", GUILayout.Width(80)))
            {
                config.maxFitnessLevel -= 5;
                config.validate();
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
    }
}
