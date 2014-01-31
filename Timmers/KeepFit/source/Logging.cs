using UnityEngine;

namespace KeepFit
{
    public static class Logging
    {
        public static void Log(this UnityEngine.Object obj, string context, string message)
        {
            Debug.Log(obj.GetType().FullName + "[" + context + "][" + Time.time.ToString("0.00") + "]: " + message);
        }

        public static void Warn(this UnityEngine.Object obj, string context, string message)
        {
            Debug.LogWarning(obj.GetType().FullName + "[" + context + "][" + Time.time.ToString("0.00") + "]: " + message);
        }

        public static void Error(this UnityEngine.Object obj, string context, string message)
        {
            Debug.LogError(obj.GetType().FullName + "[" + context + "][" + Time.time.ToString("0.00") + "]: " + message);
        }

        public static void Log(this System.Object obj, string context, string message)
        {
            Debug.Log(obj.GetType().FullName + "[" + context + "][" + Time.time.ToString("0.00") + "]: " + message);
        }

        public static void Warn(this System.Object obj, string context, string message)
        {
            Debug.LogWarning(obj.GetType().FullName + "[" + context + "][" + Time.time.ToString("0.00") + "]: " + message);
        }

        public static void Error(this System.Object obj, string context, string message)
        {
            Debug.LogError(obj.GetType().FullName + "[" + context + "][" + Time.time.ToString("0.00") + "]: " + message);
        }
    }
}
