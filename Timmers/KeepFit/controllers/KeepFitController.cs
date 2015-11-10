using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KeepFit
{
    public abstract class KeepFitController : MonoBehaviour
    {
        /// <summary>
        /// Per-game saved configuration information
        /// </summary>
        internal KeepFitScenarioModule module { get; private set; }

        protected GameConfig gameConfig;

        public KeepFitController()
        {
            this.Log_DebugOnly("ctor", ".");
        }

        internal virtual void Init(KeepFitScenarioModule module)
        {
            this.Log_DebugOnly("Init", ".");
            this.module = module;
            this.gameConfig = module.GetGameConfig();
        }
    }
}
