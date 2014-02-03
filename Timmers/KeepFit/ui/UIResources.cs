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

        public Dictionary<String, Texture2D> texIconsKeepFit { get; private set; }


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

            texIconsKeepFit = LoadIconDictionary("Icons");
            this.Log_DebugOnly("LoadTexture", "KeepFit Icons Loaded: {0}", texIconsKeepFit.Count.ToString());

            this.texPanel = LoadImageFromGameDB("img_PanelBack.png");
            this.texBarOrange = LoadImageFromGameDB("img_BarOrange.png");
            this.texBarOrange_Back = LoadImageFromGameDB("img_BarOrange_Back.png");
            this.texBarRed = LoadImageFromGameDB("img_BarRed.png");
            this.texBarRed_Back = LoadImageFromGameDB("img_BarRed_Back.png");
            this.texBarGreen = LoadImageFromGameDB("img_BarGreen.png");
            this.texBarGreen_Back = LoadImageFromGameDB("img_BarGreen_Back.png");

            this.btnChevronUp = LoadImageFromGameDB("img_buttonChevronUp.png");
            this.btnChevronDown = LoadImageFromGameDB("img_buttonChevronDown.png");

            this.btnSettingsAttention = LoadImageFromGameDB("img_buttonSettingsAttention.png");

            this.txtTooltipBackground = LoadImageFromGameDB("txt_TooltipBackground.png");
        }

        private Dictionary<String, Texture2D> LoadIconDictionary(String IconFolderName)
        {
            Dictionary<String, Texture2D> dictReturn = new Dictionary<string, Texture2D>();
            Texture2D texLoading;

            String strIconPath = string.Format("{0}/{1}", PathTextures, IconFolderName);
            String strIconDBPath = string.Format("{0}/{1}", DBPathTextures, IconFolderName);

            this.Log_DebugOnly("LoadIconDictionary", "{0}--{1}",strIconPath,strIconDBPath);

            if (Directory.Exists(strIconPath))
            {
                FileInfo[] fileIcons = new System.IO.DirectoryInfo(strIconPath).GetFiles("*.png");
                foreach (FileInfo fileIcon in fileIcons)
                {
                    //this.Log_DebugOnly("LoadTexture", "{0}", fileIcon.FullName);
                    try
                    {
                        texLoading = LoadImageFromGameDB(fileIcon.Name, strIconDBPath);
                        if (texLoading != null)
                        {
                            dictReturn.Add(fileIcon.Name.ToLower().Replace(".png", ""), texLoading);
                        }
                        //texLoading = GameDatabase.Instance.GetTexture(string.Format("{0}/{1}", strIconDBPath, fileIcon.Name.ToLower().Replace(".png", "")), false);
                        //dictReturn.Add(fileIcon.Name.ToLower().Replace(".png", ""), texLoading);
                    }
                    catch (Exception)
                    {
                        this.Log_DebugOnly("LoadTexture", "Unable to load Texture from GameDB:{0}", strIconPath);
                    }
                    //texLoading; // = new Texture2D(32, 16, TextureFormat.ARGB32, false);
                    //if (LoadImageIntoTexture2(ref texLoading, fileIcon.Name, strIconPath))
                    //    dictReturn.Add(fileIcon.Name.ToLower().Replace(".png", ""), texLoading);
                }
            }
            return dictReturn;
        }

        private Dictionary<String, Texture2D> LoadIconDictionary_Defs()
        {
            Dictionary<String, Texture2D> dictReturn = new Dictionary<string, Texture2D>();
            Texture2D texLoading;

            ConfigNode[] cns = GameDatabase.Instance.GetConfigNodes("RESOURCE_DEFINITION");
            this.Log_DebugOnly("LoadIconDictionary_Defs", cns.Length.ToString());
            foreach (ConfigNode cn in cns)
            {
                if (cn.HasValue("name"))
                {
                    if (cn.HasValue("ksparpicon"))
                    {
                        try
                        {
                            texLoading = GameDatabase.Instance.GetTexture(cn.GetValue("ksparpicon"), false);
                            if ((texLoading.width > 32) || (texLoading.height > 16))
                            {
                                this.Log_DebugOnly("LoadIconDictionary_Defs", "Texture Too Big (32x16 is limit) - w:{0} h:{1}", texLoading.width, texLoading.height);
                            }
                            else
                            {
                                dictReturn.Add(cn.GetValue("name").ToLower(), texLoading);
                            }
                        }
                        catch (Exception)
                        {
                            this.Log_DebugOnly("LoadIconDictionary_Defs", "Unable to load texture {0}-{1}", cn.GetValue("name"), cn.GetValue("ksparpicon"));
                        }
                    }
                }
            }

            return dictReturn;
        }

        //public static Byte[] LoadFileToArray(String Filename)
        //{
        //    Byte[] arrBytes;

        //    arrBytes = KSP.IO.File.ReadAllBytes<KSPAlternateResourcePanel>(Filename);

        //    return arrBytes;
        //}
        public static Byte[] LoadFileToArray2(String Filename)
        {
            Byte[] arrBytes;

            arrBytes = System.IO.File.ReadAllBytes(Filename);

            return arrBytes;
        }

        //public static void SaveFileFromArray(Byte[] data, String Filename)
        //{
        //    KSP.IO.File.WriteAllBytes<KSPAlternateResourcePanel>(data, Filename);
        //}


        //public static Boolean LoadImageIntoTexture(ref Texture2D tex, String FileName)
        //{
        //    Boolean blnReturn = false;
        //    try
        //    {
        //        //DebugLogFormatted("Loading {0}", FileName);
        //        tex.LoadImage(LoadFileToArray(FileName));
        //        blnReturn = true;
        //    }
        //    catch (Exception)
        //    {
        //        DebugLogFormatted("Failed to load (are you missing a file):{0}", FileName);
        //    }
        //    return blnReturn;
        //}

        public Boolean LoadImageIntoTexture2(ref Texture2D tex, String FileName, String FolderPath = "")
        {
            //DebugLogFormatted("{0},{1}",FileName, FolderPath);
            Boolean blnReturn = false;
            try
            {
                if (FolderPath == "") FolderPath = PathPluginData;
                //DebugLogFormatted("Loading {0}", FileName);
                tex.LoadImage(LoadFileToArray2(string.Format("{0}/{1}", FolderPath, FileName)));
                blnReturn = true;
            }
            catch (Exception)
            {
                this.Error_Release("LoadImageIntoTexture2", "Failed to load (are you missing a file):{0}", FileName);
            }
            return blnReturn;
        }

        public Texture2D  LoadImageFromGameDB(String FileName, String FolderPath = "")
        {
            this.Log_DebugOnly("LoadImageFromGameDB", "{0},{1}",FileName, FolderPath);
            try
            {
                if (FileName.ToLower().EndsWith(".png")) FileName = FileName.Substring(0, FileName.Length - 4);
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
