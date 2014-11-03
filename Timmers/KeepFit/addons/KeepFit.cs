using KSP.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        /// <summary>
        /// AppLauncher button
        /// </summary>
        private ApplicationLauncherButton appLauncherButton;

        private MainWindow mainWindow;
        private RosterWindow rosterWindow;
        private AllVesselsWindow allVesselsWindow;
        private LogWindow logWindow;
        private ConfigWindow configWindow;

        /// <summary>
        /// UI window for displaying the current crew roster
        /// </summary>


        /// <summary>
        /// UI Window for in flight use for displaying the active vessel crew's fitness level
        /// </summary>




        private KeepFitCrewFitnessController crewFitnessController;
        private KeepFitCrewRosterController crewRosterController;
        private KeepFitGeeEffectsController geeEffectsController;

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

            if (mainWindow == null)
            {
                mainWindow = gameObject.AddComponent<MainWindow>();
                mainWindow.Init(this);
            }

            if (logWindow == null)
            {
                logWindow = gameObject.AddComponent<LogWindow>();
                logWindow.Init(this);
            }

            if (configWindow == null)
            {
                configWindow = gameObject.AddComponent<ConfigWindow>();
                configWindow.Init(this);
            }

            if (rosterWindow == null)
            {
                this.Log_DebugOnly("OnAwake", "Constructing rosterWindow");
                rosterWindow = gameObject.AddComponent<RosterWindow>();
                rosterWindow.Init(this);
            }

            if (allVesselsWindow == null)
            {
                this.Log_DebugOnly("OnAwake", "Constructing allVesselsWindow");
                allVesselsWindow = gameObject.AddComponent<AllVesselsWindow>();
                allVesselsWindow.Init(this);
            }


            if (appLauncherButton != null)
            {
                this.Log_DebugOnly("OnAwake", "AppLauncher button already here");
            }
            else
            {
                this.Log_DebugOnly("OnAwake", "Adding AppLauncher button");
            
                Texture toolbarButtonTexture = (Texture)GameDatabase.Instance.GetTexture("Timmers/KeepFit/KeepFit", false);
                ApplicationLauncher.AppScenes scenes = ApplicationLauncher.AppScenes.FLIGHT |
                                                       ApplicationLauncher.AppScenes.MAPVIEW |
                                                       ApplicationLauncher.AppScenes.SPACECENTER |
                                                       ApplicationLauncher.AppScenes.SPH |
                                                       ApplicationLauncher.AppScenes.VAB |
                                                       ApplicationLauncher.AppScenes.TRACKSTATION;

                appLauncherButton = ApplicationLauncher.Instance.AddModApplication(onAppLaunchToggleOn,
                                                               onAppLaunchToggleOff,
                                                               onAppLaunchHoverOn,
                                                               onAppLaunchHoverOff,
                                                               onAppLaunchEnable,
                                                               onAppLaunchDisable,
                                                               scenes,
                                                               toolbarButtonTexture);
                ApplicationLauncher.Instance.EnableMutuallyExclusive(appLauncherButton);
                ApplicationLauncher.Instance.AddOnRepositionCallback(onAppLauncherReposition);
            }


            this.Log_DebugOnly("OnAwake", "Adding KeepFitCrewRosterController");
            crewRosterController = gameObject.AddComponent<KeepFitCrewRosterController>();
            crewRosterController.Init(gameConfig);

            if (HighLogic.LoadedScene == GameScenes.FLIGHT ||
                HighLogic.LoadedScene == GameScenes.TRACKSTATION ||
                HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                this.Log_DebugOnly("OnAwake", "Adding KeepFitCrewFitnessController");
                crewFitnessController = gameObject.AddComponent<KeepFitCrewFitnessController>();
                crewFitnessController.Init(gameConfig);
            }

            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                this.Log_DebugOnly("OnAwake", "Adding KeepFitGeeEffectsController");
                this.geeEffectsController = gameObject.AddComponent<KeepFitGeeEffectsController>();
                this.geeEffectsController.Init(gameConfig);
            }
        }

        public GameConfig GetGameConfig()
        {
            return gameConfig;
        }

        public bool isKeepFitEnabled()
        {
            return gameConfig.enabled;
        }

        public bool isCrewFitnessControllerActive() 
        {
            return (this.crewFitnessController != null);
        }

        public bool isCrewRosterControllerActive()
        {
            return (this.crewRosterController != null);
        }

        public bool isGeeEffectsControllerActive()
        {
            return (this.geeEffectsController != null);
        }

        public void ShowLog()
        {
            this.logWindow.Show();
        }

        public void ShowRoster()
        {
            this.rosterWindow.Show();
        }

        public void ShowVessels()
        {
            this.allVesselsWindow.Show();
        }

        public void ShowSettings()
        {
            this.configWindow.Visible = true;
        }

        void onAppLauncherReposition()
        {

        }

        void onAppLaunchToggleOn() 
        {
            this.Log_DebugOnly("onAppLaunchToggleOn", "ToggleOn called - showing windows");
            
            /*Your code goes in here to toggle display on regardless of hover*/
            showKeepFitWindow();
        }

        void onAppLaunchToggleOff() 
        {
            this.Log_DebugOnly("onAppLaunchToggleOff", "ToggleOff called - hiding windows");

            /*Your code goes in here to toggle display off regardless of hover*/
            hideKeepFitWindows();
        }

        void onAppLaunchHoverOn() 
        {
            this.Log_DebugOnly("onAppLaunchHoverOn", "HoverOn called - does nothing");

            /*Your code goes in here to show display on*/
            showKeepFitWindow();
        }

        void onAppLaunchHoverOff() 
        {
            this.Log_DebugOnly("onAppLaunchHoverOff", "HoverOff called - does nothing");

            /*Your code goes in here to show display off*/
            hideKeepFitWindows();
        }
        
        void onAppLaunchEnable() 
        {
            this.Log_DebugOnly("onAppLaunchEnable", "LaunchEnable called - showing window for scene");

            showKeepFitWindow();
        }

        private void showKeepFitWindow()
        {
            /*Your code goes in here for if it gets enabled*/
            mainWindow.Visible = true;
        }

        void onAppLaunchDisable() 
        {
            this.Log_DebugOnly("onAppLaunchDisable", "LaunchDisable called - hiding windows");

            hideKeepFitWindows();
        }

        private void hideKeepFitWindows()
        {
            /*Your code goes in here for if it gets disabled*/
            this.mainWindow.Visible = false;
            this.logWindow.Visible = false;
            this.configWindow.Visible = false;
            this.allVesselsWindow.Visible = false;
            this.rosterWindow.Visible = false;
        }

        public override void OnLoad(ConfigNode gameNode)
        {
            base.OnLoad(gameNode);

            mainWindow.Load(gameNode);
            this.Log_DebugOnly("OnLoad: ", "Loaded mainWindow");
            
            logWindow.Load(gameNode);
            this.Log_DebugOnly("OnLoad: ", "Loaded logWindow");

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

            this.Log_DebugOnly("OnSave", "Saved keepfit persistence data");
//            this.Log_DebugOnly("OnSave", gameNode.ToString());
        }

        void OnDestroy()
        {
            this.Log_DebugOnly("OnDestroy", ".");
            if (appLauncherButton != null)
            {
                ApplicationLauncher.Instance.DisableMutuallyExclusive(appLauncherButton);
                ApplicationLauncher.Instance.RemoveModApplication(appLauncherButton);
                appLauncherButton = null;
            }

            if (this.crewFitnessController != null)
            {
                Destroy(crewFitnessController);
                crewFitnessController = null;
            }

            if (this.crewRosterController != null)
            {
                Destroy(crewRosterController);
                crewRosterController = null;
            }

            if (this.geeEffectsController != null)
            {
                Destroy(geeEffectsController);
                geeEffectsController = null;
            }
        }
    }
}
