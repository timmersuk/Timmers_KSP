using UnityEngine;

namespace KeepFit
{
    public static class ConfigUtils
    {
        public static void UpdateSetting(ConfigNode node, string name, string value)
        {
            if (!node.HasValue(name))
            {
                node.AddValue(name, value);
            }
            else
            {
                node.SetValue(name, value);
            }
        }

        public static bool GetBoolSetting(ConfigNode settingsNode, string name, bool defValue)
        {
            string sValue = settingsNode.GetValue(name);
            if (sValue == null)
            {
                return defValue;
            }

            return bool.Parse(sValue);
        }

        public static float GetFloatSetting(ConfigNode settingsNode, string name, float defValue)
        {
            string sValue = settingsNode.GetValue(name);
            if (sValue == null)
            {
                return defValue;
            }

            return float.Parse(sValue);
        }

        public static double GetDoubleSetting(ConfigNode settingsNode, string name, double defValue)
        {
            string sValue = settingsNode.GetValue(name);
            if (sValue == null)
            {
                return defValue;
            }

            return double.Parse(sValue);
        }

        public static int GetIntSetting(ConfigNode settingsNode, string name, int defValue)
        {
            string sValue = settingsNode.GetValue(name);
            if (sValue == null)
            {
                return defValue;
            }

            return int.Parse(sValue);
        }
    }
}
