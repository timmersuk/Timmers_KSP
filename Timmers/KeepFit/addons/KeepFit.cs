using KSP.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Toolbar;
using UnityEngine;

namespace KeepFit
{
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
                this.Log_DebugOnly("Start", "Adding the scenario module.");
                psm = game.AddProtoScenarioModule(typeof(KeepFitScenarioModule), 
                    GameScenes.SPACECENTER,
                    GameScenes.TRACKSTATION, 
                    GameScenes.FLIGHT, 
                    GameScenes.EDITOR, 
                    GameScenes.SPH);
            }
            else
            {
                if (!psm.targetScenes.Any(s => s == GameScenes.SPACECENTER))
                {
                    psm.targetScenes.Add(GameScenes.SPACECENTER);
                }
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
        private IButton toolbarButton;

        /// <summary>
        /// UI window for editing the config
        /// </summary>
        private ConfigWindow configWindow;

        /// <summary>
        /// UI window for displaying the current crew roster
        /// </summary>
        private RosterWindow rosterWindow;

        /// <summary>
        /// UI Window for in flight use for displaying the active vessel crew's fitness level
        /// </summary>
        private InFlightActiveVesselWindow inFlightActiveVesselWindow;

        private AllVesselsWindow allVesselsWindow;



        /// <summary>
        /// Main copy of the per-game config
        /// </summary>
        private GameConfig gameConfig = new GameConfig();

        public KeepFitScenarioModule()
        {
            this.Log_DebugOnly("Constructor", ".");

            gameConfig = new GameConfig();
        }

        public override void OnAwake()
        {
            this.Log_DebugOnly("OnAwake", "Scene[{0}]", HighLogic.LoadedScene);
            base.OnAwake();

            if (configWindow == null)
            {
                configWindow = gameObject.AddComponent<ConfigWindow>();
                configWindow.config = gameConfig;
            }

            if (rosterWindow == null)
            {
                rosterWindow = gameObject.AddComponent<RosterWindow>();
                rosterWindow.gameConfig = gameConfig;
                rosterWindow.configWindow = configWindow;
            }

            if (allVesselsWindow == null)
            {
                allVesselsWindow = gameObject.AddComponent<AllVesselsWindow>();
                allVesselsWindow.gameConfig = gameConfig;
            }

            if (inFlightActiveVesselWindow == null)
            {
                inFlightActiveVesselWindow = gameObject.AddComponent<InFlightActiveVesselWindow>();
                inFlightActiveVesselWindow.gameConfig = gameConfig;
                inFlightActiveVesselWindow.configWindow = configWindow;
                inFlightActiveVesselWindow.allVesselsWindow = allVesselsWindow;
            }

            if (toolbarButton == null)
            {
                toolbarButton = ToolbarManager.Instance.add("KeepFit", "ShowKeepFit");
                toolbarButton.TexturePath = "Timmers/KeepFit/KeepFit";
                toolbarButton.ToolTip = "KeepFit";
                toolbarButton.OnClick += (e) =>
                {
                    this.Log_DebugOnly("toolbarButtonOnClick", "Toggling keep fit Window visibility");
                    switch (HighLogic.LoadedScene)
                    {
                        case GameScenes.FLIGHT:
                            inFlightActiveVesselWindow.Visible = !inFlightActiveVesselWindow.Visible;
                            break;
                        case GameScenes.SPACECENTER:
                            this.configWindow.Visible = !configWindow.Visible;
                            break;
                        case GameScenes.EDITOR:
                        case GameScenes.SPH:
                        case GameScenes.TRACKSTATION:
                            rosterWindow.Visible = !rosterWindow.Visible;
                            break;
                    }
                };
            }

            this.Log_DebugOnly("OnAwake", "Adding KeepFitCrewRosterController");
            addController(gameObject.AddComponent<KeepFitCrewRosterController>());

            this.Log_DebugOnly("OnAwake", "Adding KeepFitController");
            if (HighLogic.LoadedScene == GameScenes.FLIGHT ||
                HighLogic.LoadedScene == GameScenes.TRACKSTATION ||
                HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                addController(gameObject.AddComponent<KeepFitCrewFitnessController>());
            }

            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                addController(gameObject.AddComponent<KeepFitGeeEffectsController>());
            }
        }

        private void addController(KeepFitController controller)
        {
            controller.SetGameConfig(gameConfig);
            children.Add(controller);
        }

        public override void OnLoad(ConfigNode gameNode)
        {
            base.OnLoad(gameNode);

            this.Log_DebugOnly("OnLoad: ", "{0}", gameNode.ToString());
            configWindow.Load(gameNode);
            this.Log_DebugOnly("OnLoad: ", "Loaded configWindow");

            rosterWindow.Load(gameNode);
            this.Log_DebugOnly("OnLoad: ", "Loaded rosterWindow");

            gameConfig.Load(gameNode, true);
            this.Log_DebugOnly("OnLoad: ", "Loaded gameConfig");
        }

        public override void OnSave(ConfigNode gameNode)
        {
            this.Log_DebugOnly("OnSave", ".");
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

            this.Log_DebugOnly("OnSave", gameNode.ToString());
        }

        void OnDestroy()
        {
            this.Log_DebugOnly("OnDestroy", ".");
            if (toolbarButton != null)
            {
                toolbarButton.Destroy();
                toolbarButton = null;
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
