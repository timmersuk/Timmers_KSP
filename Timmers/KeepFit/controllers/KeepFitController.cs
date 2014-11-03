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
        protected GameConfig gameConfig { get; private set; }

        public KeepFitController()
        {
            this.Log_DebugOnly("ctor", ".");
        }

        internal void Init(GameConfig gameConfig)
        {
            this.Log_DebugOnly("SetGameConfig", ".");
            this.gameConfig = gameConfig;
        }
    }
}
