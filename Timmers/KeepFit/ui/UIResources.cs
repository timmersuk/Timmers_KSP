using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using KSP;
using UnityEngine;

namespace KeepFit
{
    public class UIResources 
    {
        internal bool DrawStuffConfigured = false;

        public GUIStyle styleButton { get; private set; }
        public GUIStyle styleButtonMain { get; private set; }
        public GUIStyle styleButtonSettings { get; private set; }

        public GUIStyle styleCrewName { get; private set; }

        public GUIStyle styleBarName { get; private set; }
        public GUIStyle styleBarDef { get; private set; }

        public GUIStyle styleBarRed { get; private set; }
        public GUIStyle styleBarRed_Back { get; private set; }
        public GUIStyle styleBarRed_Thin { get; private set; }
        public GUIStyle styleBarOrange { get; private set; }
        public GUIStyle styleBarOrange_Back { get; private set; }
        public GUIStyle styleBarOrange_Thin { get; private set; }
        public GUIStyle styleBarGreen { get; private set; }
        public GUIStyle styleBarGreen_Back { get; private set; }
        public GUIStyle styleBarGreen_Thin { get; private set; }

        public GUIStyle styleTextCenter { get; private set; }
        public GUIStyle styleTextCenterGreen { get; private set; }

        public GUIStyle styleBarText { get; private set; }
        public GUIStyle styleBarTextRed { get; private set; }
        public GUIStyle styleBarTextGreen { get; private set; }
        public GUIStyle styleBarRateText { get; private set; }

        public GUIStyle styleStageText { get; private set; }
        public GUIStyle styleStageTextHead { get; private set; }
        public GUIStyle styleStageButton { get; private set; }

        public GUIStyle styleTooltipStyle { get; private set; }

        public GUIStyle styleToggle { get; private set; }

        public GUIStyle styleSettingsArea { get; private set; }

        public Texture2D texPanel { get; private set; } // = new Texture2D(16, 16, TextureFormat.ARGB32, false);

        public Texture2D texBarRed { get; private set; } // = new Texture2D(14, 14, TextureFormat.ARGB32, false);
        public Texture2D texBarRed_Back { get; private set; } // = new Texture2D(14, 14, TextureFormat.ARGB32, false);
        public Texture2D texBarOrange { get; private set; } // = new Texture2D(14, 14, TextureFormat.ARGB32, false);
        public Texture2D texBarOrange_Back { get; private set; } // = new Texture2D(14, 14, TextureFormat.ARGB32, false);
        public Texture2D texBarGreen { get; private set; }// = new Texture2D(14, 14, TextureFormat.ARGB32, false);
        public Texture2D texBarGreen_Back { get; private set; } // = new Texture2D(14, 14, TextureFormat.ARGB32, false);

        public Texture2D btnChevronUp { get; private set; } // = new Texture2D(17, 16, TextureFormat.ARGB32, false);
        public Texture2D btnChevronDown { get; private set; } // = new Texture2D(17, 16, TextureFormat.ARGB32, false);

        public Texture2D btnSettingsAttention { get; private set; } // = new Texture2D(17, 16, TextureFormat.ARGB32, false);

        public Texture2D txtTooltipBackground { get; private set; } // = new Texture2D(9, 9);//, TextureFormat.ARGB32, false);

        public Dictionary<ActivityLevel, Texture2D> texIconsActivityLevels { get; private set; }


        private static String _ClassName = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;

        private static String PathApp = KSPUtil.ApplicationRootPath.Replace("\\", "/");
        private static String PathKeepFit = string.Format("{0}GameData/Timmers/KeepFit", PathApp);
        private static String PathPluginData = string.Format("{0}/PluginData/{1}", PathKeepFit, _ClassName);
        private static String PathTextures = string.Format("{0}/{1}", PathKeepFit, _ClassName);

        public static String DBPathKeepFit = string.Format("Timmers/KeepFit");
        public static String DBPathTextures = string.Format("{0}/{1}", DBPathKeepFit, _ClassName);
        public static String DBPathSounds = string.Format("{0}/Sounds/{1}", DBPathKeepFit, _ClassName);
        

        internal UIResources()
        {
        }

        public void SetupDrawStuff()
        {
            InitStyles();

            DrawStuffConfigured = true;
        }


        internal void LoadTextures()
        {
            this.Log_DebugOnly("LoadTextures", "Loading Textures");

            texIconsActivityLevels = LoadActivityIcons();
            this.Log_DebugOnly("LoadTexture", "KeepFit Activity Level Icons Loaded: {0}", texIconsActivityLevels.Count.ToString());

            this.texPanel = LoadImageFromGameDB("img_PanelBack");
            this.texBarOrange = LoadImageFromGameDB("img_BarOrange");
            this.texBarOrange_Back = LoadImageFromGameDB("img_BarOrange_Back");
            this.texBarRed = LoadImageFromGameDB("img_BarRed");
            this.texBarRed_Back = LoadImageFromGameDB("img_BarRed_Back");
            this.texBarGreen = LoadImageFromGameDB("img_BarGreen");
            this.texBarGreen_Back = LoadImageFromGameDB("img_BarGreen_Back");

            this.btnChevronUp = LoadImageFromGameDB("img_buttonChevronUp");
            this.btnChevronDown = LoadImageFromGameDB("img_buttonChevronDown");

            this.btnSettingsAttention = LoadImageFromGameDB("img_buttonSettingsAttention");

            this.txtTooltipBackground = LoadImageFromGameDB("txt_TooltipBackground");
        }

        private Dictionary<ActivityLevel, Texture2D> LoadActivityIcons()
        {
            Dictionary<ActivityLevel, Texture2D> dictReturn = new Dictionary<ActivityLevel, Texture2D>();
            Texture2D texLoading;

            foreach (ActivityLevel activityLevel in Enum.GetValues(typeof(ActivityLevel)))
            {
                String iconName = "activityLevel_" + activityLevel.ToString().ToLower();

                try
                { 
                    texLoading = LoadImageFromGameDB(iconName, DBPathTextures);
                    if (texLoading != null)
                    {
                        dictReturn[activityLevel] = texLoading;
                    }
                }
                catch (Exception)
                {
                    this.Log_DebugOnly("LoadActivityIcons", "Unable to load Texture from GameDB:{0}", iconName);
                }
            }
            return dictReturn;
        }

        public static Byte[] LoadFileToArray2(String Filename)
        {
            Byte[] arrBytes;

            arrBytes = System.IO.File.ReadAllBytes(Filename);

            return arrBytes;
        }

        public Texture2D  LoadImageFromGameDB(String FileName, String FolderPath = "")
        {
            this.Log_DebugOnly("LoadImageFromGameDB", "{0},{1}",FileName, FolderPath);
            try
            {
                if (FolderPath == "") FolderPath = DBPathTextures;
                this.Log_DebugOnly("LoadImageFromGameDB", "Loading {0}", String.Format("{0}/{1}", FolderPath, FileName));
                Texture2D tex = GameDatabase.Instance.GetTexture(String.Format("{0}/{1}", FolderPath, FileName), false);
                return tex;
            }
            catch (Exception)
            {
                this.Log_DebugOnly("LoadImageFromGameDB", "Failed to load (are you missing a file):{0}/{1}", String.Format("{0}/{1}", FolderPath, FileName));
                return null;
            }
        }




        private void InitStyles()
        {
            this.Log_DebugOnly("InitStyles", "Initializing Styles");

            styleButton = new GUIStyle(SkinsLibrary.CurrentSkin.button);
            styleButton.normal.textColor = new Color(207, 207, 207);
            styleButton.fontStyle = FontStyle.Normal;
            styleButton.fixedHeight = 18;
            //styleButton.alignment = TextAnchor.MiddleCenter;
            this.Log_DebugOnly("InitStyles", "styleButton done");

            styleButtonMain = new GUIStyle(styleButton);
            styleButtonMain.fixedHeight = 20;
            this.Log_DebugOnly("InitStyles", "styleButtonMain done");

            styleButtonSettings = new GUIStyle(styleButton);
            styleButtonSettings.padding = new RectOffset(1, 1, 1, 1);
            styleButtonSettings.fixedWidth = 40;
            this.Log_DebugOnly("InitStyles", "styleButtonSettings done");

            styleStageButton = new GUIStyle(styleButton);
            this.Log_DebugOnly("InitStyles", "styleStageButton done");

            styleCrewName = new GUIStyle(SkinsLibrary.CurrentSkin.label) { fixedHeight = 16 };
            styleCrewName.normal.textColor = Color.white;
            styleCrewName.alignment = TextAnchor.MiddleLeft;
            this.Log_DebugOnly("InitStyles", "styleCrewName done");

            styleBarName = new GUIStyle() { fixedHeight = 16, fixedWidth = 32 };
            styleBarName.normal.textColor = Color.white;
            styleBarName.alignment = TextAnchor.MiddleCenter;
            this.Log_DebugOnly("InitStyles", "styleBarName done");

            styleBarDef = new GUIStyle(SkinsLibrary.CurrentSkin.box);
            styleBarDef.border = new RectOffset(2, 2, 2, 2);
            styleBarDef.normal.textColor = Color.white;
            this.Log_DebugOnly("InitStyles", "styleBarDef done");

            styleBarRed = new GUIStyle(styleBarDef);
            styleBarRed.normal.background = texBarRed;
            this.Log_DebugOnly("InitStyles", "styleBarRed done");

            styleBarRed_Back = new GUIStyle(styleBarDef);
            styleBarRed_Back.normal.background = texBarRed_Back;
            this.Log_DebugOnly("InitStyles", "styleBarRed_Back done");
            
            styleBarRed_Thin = new GUIStyle(styleBarRed);
            styleBarRed_Thin.border = new RectOffset(0, 0, 0, 0);
            this.Log_DebugOnly("InitStyles", "styleBarRed_Thin done");

            styleBarOrange = new GUIStyle(styleBarDef);
            styleBarOrange.normal.background = texBarOrange;
            this.Log_DebugOnly("InitStyles", "styleBarOrange done");

            styleBarOrange_Back = new GUIStyle(styleBarDef);
            styleBarOrange_Back.normal.background = texBarOrange_Back;
            this.Log_DebugOnly("InitStyles", "styleBarOrange_Back done");
            
            styleBarOrange_Thin = new GUIStyle(styleBarOrange);
            styleBarOrange_Thin.border = new RectOffset(0, 0, 0, 0);
            this.Log_DebugOnly("InitStyles", "styleBarOrange_Thin done");

            styleBarGreen = new GUIStyle(styleBarDef);
            styleBarGreen.normal.background = texBarGreen;
            this.Log_DebugOnly("InitStyles", "styleBarGreen done");

            styleBarGreen_Back = new GUIStyle(styleBarDef);
            styleBarGreen_Back.normal.background = texBarGreen_Back;
            this.Log_DebugOnly("InitStyles", "styleBarGreen_Back done");

            styleBarGreen_Thin = new GUIStyle(styleBarGreen);
            styleBarGreen_Thin.border = new RectOffset(0, 0, 0, 0);
            this.Log_DebugOnly("InitStyles", "styleBarGreen_Thin done");

            styleBarText = new GUIStyle(SkinsLibrary.CurrentSkin.label);
            styleBarText.fontSize = 12;
            styleBarText.alignment = TextAnchor.MiddleCenter;
            styleBarText.normal.textColor = new Color(255, 255, 255, 0.8f);
            styleBarText.wordWrap = false;
            this.Log_DebugOnly("InitStyles", "styleBarText done");

            styleBarTextRed = new GUIStyle(SkinsLibrary.CurrentSkin.label);
            styleBarTextRed.fontSize = 12;
            styleBarTextRed.alignment = TextAnchor.MiddleCenter;
            styleBarTextRed.normal.textColor = new Color(255, 0, 0, 0.8f);
            styleBarTextRed.wordWrap = false;
            this.Log_DebugOnly("InitStyles", "styleBarTextRed done");

            styleBarTextGreen = new GUIStyle(SkinsLibrary.CurrentSkin.label);
            styleBarTextGreen.fontSize = 12;
            styleBarTextGreen.alignment = TextAnchor.MiddleCenter;
            styleBarTextGreen.normal.textColor = new Color(0, 255, 0, 0.8f);
            styleBarTextGreen.wordWrap = false;
            this.Log_DebugOnly("InitStyles", "styleBarTextRed done");

            styleBarRateText = new GUIStyle(styleBarText);
            styleBarRateText.alignment = TextAnchor.MiddleRight;
            this.Log_DebugOnly("InitStyles", "styleBarRateText done");


            styleTextCenter = new GUIStyle(SkinsLibrary.CurrentSkin.label);
            styleTextCenter.alignment = TextAnchor.MiddleCenter;
            styleTextCenter.wordWrap = false;
            styleTextCenter.normal.textColor = new Color(207, 207, 207);
            this.Log_DebugOnly("InitStyles", "styleTextCenter done");

            styleTextCenterGreen = new GUIStyle(styleTextCenter);
            styleTextCenterGreen.normal.textColor = new Color32(183, 254, 0, 255);
            this.Log_DebugOnly("InitStyles", "styleTextCenterGreen done");

            styleStageText = new GUIStyle(SkinsLibrary.CurrentSkin.label);
            styleStageText.normal.textColor = new Color(207, 207, 207);
            styleStageText.wordWrap = false;
            this.Log_DebugOnly("InitStyles", "styleStageText done");

            styleStageTextHead = new GUIStyle(styleStageText);
            styleStageTextHead.fontStyle = FontStyle.Bold;
            styleStageTextHead.wordWrap = false;
            this.Log_DebugOnly("InitStyles", "styleStageTextHead done");

            styleStageButton = new GUIStyle(styleButton);
            this.Log_DebugOnly("InitStyles", "styleStageButton done");

            styleToggle = new GUIStyle(SkinsLibrary.CurrentSkin.toggle);
            styleToggle.normal.textColor = new Color(207, 207, 207);
            styleToggle.fixedHeight = 20;
            styleToggle.padding = new RectOffset(6, 0, -2, 0);
            this.Log_DebugOnly("InitStyles", "styleToggle done");

            styleSettingsArea = new GUIStyle(SkinsLibrary.CurrentSkin.textArea);
            styleSettingsArea.padding = new RectOffset(0, 0, 0, 4);
            this.Log_DebugOnly("InitStyles", "styleSettingsArea done");

            styleTooltipStyle = new GUIStyle();
            styleTooltipStyle.fontSize = 12;
            styleTooltipStyle.normal.textColor = new Color32(207, 207, 207, 255);
            styleTooltipStyle.stretchHeight = true;
            styleTooltipStyle.wordWrap = true;
            styleTooltipStyle.normal.background = SkinsLibrary.CurrentSkin.box.normal.background;
            //Extra border to prevent bleed of color - actual border is only 1 pixel wide
            styleTooltipStyle.border = new RectOffset(3, 3, 3, 3);
            styleTooltipStyle.padding = new RectOffset(4, 4, 6, 4);
            styleTooltipStyle.alignment = TextAnchor.MiddleCenter;
            this.Log_DebugOnly("InitStyles", "styleTooltipStyle done");
        }



    }
}
