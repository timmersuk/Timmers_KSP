using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KeepFit
{
    class LogWindow : KeepFitInfoWindow
    {
        private Vector2 scrollPosition;

        public LogWindow()
            : base(true, true, "log")
        {
            this.WindowCaption = "KeepFit Log";
            this.Visible = false;
            this.DragEnabled = true;

            this.WindowRect = new Rect(0, 0, 300, 300);
        }



        protected override void FillWindow(int id)
        {
            GUILayout.BeginVertical();
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginVertical();
            foreach (String logLine in Logging.GetLogBuffer())
            {
                GUILayout.Label(logLine);
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }
}
