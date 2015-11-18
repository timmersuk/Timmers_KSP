using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KeepFit
{
    public abstract class SaveableWindow : MonoBehaviourWindow
    {
        private readonly string configNodeName;

        private GUIStyle closeButtonStyle;
        private GUIStyle resizeStyle;
        private GUIContent resizeContent;
        private bool mouseDown;

        private readonly bool resizeable;
        private readonly bool showCloseButton;

        protected SaveableWindow(bool resizeable, bool showCloseButton, string configNodeName)
        {
            this.resizeable = resizeable;
            this.showCloseButton = showCloseButton;
            this.configNodeName = configNodeName;
        }

        internal override void DrawWindow(int id)
        {
            ConfigureStyles();

            if (showCloseButton && GUI.Button(new Rect(WindowRect.width - 24, 4, 20, 20), "X"))
            {
                Hide();
            }

            if (resizeable)
            {
                var resizeRect = new Rect(WindowRect.width - 16, WindowRect.height - 16, 16, 16);
                GUI.Label(resizeRect, resizeContent, resizeStyle);

                HandleWindowEvents(resizeRect);
            }
        }

        internal void Load(GameConfig config)
        {
            this.Log_DebugOnly("Load", "Loading config for window[{0}]", configNodeName);

            config.GetWindowRect(configNodeName, ref WindowRect);

            this.Log_DebugOnly("Load", "Loaded config for window[{0}] WindowRect[{1}]", configNodeName, WindowRect);
        }

        internal void Save(GameConfig config)
        {
            this.Log_DebugOnly("Save", "Saving config for window[{0}]", configNodeName);

            config.SetWindowRect(configNodeName, WindowRect);

            this.Log_DebugOnly("Load", "Saved config for window[{0}] WindowRect[{1}]", configNodeName, WindowRect);
        }

        private void HandleWindowEvents(Rect resizeRect)
        {
            var theEvent = Event.current;
            if (theEvent != null)
            {
                if (!mouseDown)
                {
                    if (theEvent.type == EventType.MouseDown && theEvent.button == 0 && resizeRect.Contains(theEvent.mousePosition))
                    {
                        mouseDown = true;
                        theEvent.Use();
                    }
                }
                else if (theEvent.type != EventType.Layout)
                {
                    if (Input.GetMouseButton(0))
                    {
                        // Flip the mouse Y so that 0 is at the top
                        float mouseY = Screen.height - Input.mousePosition.y;

                        WindowRect.width = Mathf.Clamp(Input.mousePosition.x - WindowRect.x + (resizeRect.width / 2), 50, Screen.width - WindowRect.x);
                        WindowRect.height = Mathf.Clamp(mouseY - WindowRect.y + (resizeRect.height / 2), 50, Screen.height - WindowRect.y);
                    }
                    else
                    {
                        mouseDown = false;
                    }
                }
            }
        }

        protected virtual void ConfigureStyles()
        {
            if (closeButtonStyle == null)
            {
                closeButtonStyle = new GUIStyle(GUI.skin.button);
                closeButtonStyle.padding = new RectOffset(5, 5, 3, 0);
                closeButtonStyle.margin = new RectOffset(1, 1, 1, 1);
                closeButtonStyle.stretchWidth = false;
                closeButtonStyle.stretchHeight = false;
                closeButtonStyle.alignment = TextAnchor.MiddleCenter;
            }

            if (resizeStyle == null)
            {
                resizeStyle = new GUIStyle(GUI.skin.button);
                resizeStyle.alignment = TextAnchor.MiddleCenter;
                resizeStyle.padding = new RectOffset(1, 1, 1, 1);
            }

            if (resizeContent == null)
            {
                resizeContent = new GUIContent("R", "Drag to resize the window.");
            }
        }
    }
}
