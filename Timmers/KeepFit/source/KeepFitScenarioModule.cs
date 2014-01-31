using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbar;
using UnityEngine;

namespace KeepFit
{
    
    /// <summary>
    /// Debug only helper to chuck me straight into default save.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class Debug_AutoLoadPersistentSaveOnStartup : MonoBehaviour
    {
        public static bool first = true;

        [System.Diagnostics.Conditional("DEBUG")]
        public void Start()
        {
            if (first)
            {
                this.Log("Start", "Starting in debug mode");

                first = false;
                HighLogic.SaveFolder = "default";
                var game = GamePersistence.LoadGame("persistent", HighLogic.SaveFolder, true, false);
                //if (game != null && game.flightState != null && game.compatible)
                //{
                //    FlightDriver.StartAndFocusVessel(game, 6);
                //}
                //CheatOptions.InfiniteFuel = true;
            }
        }
    }

    /*
     * This gets created when the game loads the Space Center scene. It then checks to make sure
     * the scenarios have been added to the game (so they will be automatically created in the
     * appropriate scenes).
     */
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class AddScenarioModules : MonoBehaviour
    {
        void Start()
        {
            var game = HighLogic.CurrentGame;

            ProtoScenarioModule psm = game.scenarios.Find(s => s.moduleName == typeof(KeepFitScenarioModule).Name);
            if (psm == null)
            {
                this.Log("Start", "Adding the scenario module.");
                psm = game.AddProtoScenarioModule(typeof(KeepFitScenarioModule), 
                    GameScenes.TRACKSTATION, 
                    GameScenes.FLIGHT, 
                    GameScenes.EDITOR, 
                    GameScenes.SPH);
            }
            else
            {
                if (!psm.targetScenes.Any(s => s == GameScenes.TRACKSTATION))
                {
                    psm.targetScenes.Add(GameScenes.TRACKSTATION);
                }
                if (!psm.targetScenes.Any(s => s == GameScenes.FLIGHT))
                {
                    psm.targetScenes.Add(GameScenes.FLIGHT);
                }
                if (!psm.targetScenes.Any(s => s == GameScenes.EDITOR))
                {
                    psm.targetScenes.Add(GameScenes.EDITOR);
                }
                if (!psm.targetScenes.Any(s => s == GameScenes.SPH))
                {
                    psm.targetScenes.Add(GameScenes.SPH);
                }
            }
        }
    }

    class KeepFitScenarioModule : ScenarioModule
    {
        private readonly List<KeepFitController> children = new List<KeepFitController>();
        
        /// <summary>
        /// Blizzy toolbar button for configuring the global settings of KeepFit
        /// </summary>
        private IButton rosterButton;

        /// <summary>
        /// UI window for editing the config
        /// </summary>
        private KeepFitGameConfigWindow configWindow;

        /// <summary>
        /// UI window for displaying the current crew roster
        /// </summary>
        private KeepFitRosterWindow rosterWindow;

        /// <summary>
        /// Main copy of the per-game config
        /// </summary>
        private GameConfig gameConfig = new GameConfig();

        public KeepFitScenarioModule()
        {
            this.Log("Constructor", ".");

            gameConfig = new GameConfig();
        }

        public override void OnAwake()
        {
            this.Log("OnAwake", "Scene[" + HighLogic.LoadedScene + "]");
            base.OnAwake();


            if (configWindow == null)
            {
                configWindow = gameObject.AddComponent<KeepFitGameConfigWindow>();
                configWindow.config = gameConfig;
            }

            if (rosterWindow == null)
            {
                rosterWindow = gameObject.AddComponent<KeepFitRosterWindow>();
                rosterWindow.config = gameConfig;
                rosterWindow.configWindow = configWindow;
            }

            if (rosterButton == null)
            {
                rosterButton = ToolbarManager.Instance.add("KeepFit", "rosterButton");
                rosterButton.TexturePath = "Timmers/KeepFit/img_buttonTypeMNode";
                rosterButton.ToolTip = "KeepFit Roster";
                rosterButton.OnClick += (e) =>
                {
                    this.Log("rosterButtonOnClick", "Toggling rosterWindow visibility");
                    rosterWindow.Visible = !configWindow.Visible;
                };
            }


            if (!(HighLogic.LoadedScene == GameScenes.TRACKSTATION ||
                HighLogic.LoadedScene == GameScenes.FLIGHT ||
                HighLogic.LoadedScene == GameScenes.EDITOR ||
                HighLogic.LoadedScene == GameScenes.SPH))
            {
                // not the scene we are looking for
                return;
            }

            // TDXX - need to find a way around the fact that the vessels list seems only
            // to be valid in flight, so when in SPH, VAB, KSC, TRACKING we only get the 
            // rostered not the active kerbals if we refresh


            this.Log("OnAwake", "Adding KeepFitController");
            KeepFitController c = gameObject.AddComponent<KeepFitController>();
            c.SetGameConfig(gameConfig);
            children.Add(c);
        }

        public override void OnLoad(ConfigNode gameNode)
        {
            base.OnLoad(gameNode);

            this.Log("OnLoad: ", gameNode.ToString());
            configWindow.Load(gameNode);
            rosterWindow.Load(gameNode);
            gameConfig.Load(gameNode, true);
        }

        public override void OnSave(ConfigNode gameNode)
        {
            this.Log("OnSave", ".");
            base.OnSave(gameNode);

            if (configWindow != null)
            {
                configWindow.Save(gameNode);
            } 
            if (rosterWindow != null)
            {
                rosterWindow.Save(gameNode);
            }
            gameConfig.Save(gameNode);

            this.Log("OnSave", gameNode.ToString());
        }

        void OnDestroy()
        {
            this.Log("OnDestroy", ".");
            if (rosterButton != null)
            {
                rosterButton.Destroy();
                rosterButton = null;
            }

            if (children != null)
            {
                foreach (Component c in children)
                {
                    Destroy(c);
                }
                children.Clear();
            }
        }
    }
}
