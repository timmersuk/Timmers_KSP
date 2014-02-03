using System;
using UnityEngine;

namespace KeepFit
{
    public static class Logging
    {
        /// <summary>
        /// Some Structured logging to the debug file - ONLY RUNS WHEN DLL COMPILED IN DEBUG MODE
        /// </summary>
        /// <param name="Message">Text to be printed - can be formatted as per string.format</param>
        /// <param name="strParams">Objects to feed into a string.format</param>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Log_DebugOnly(this UnityEngine.Object obj, string context, string message, params object[] strParams)
        {
            Log_Release(obj, context, message, strParams);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Warn_DebugOnly(this UnityEngine.Object obj, string context, string message, params object[] strParams)
        {
            Warn_Release(obj, context, message, strParams);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Error_DebugOnly(this UnityEngine.Object obj, string context, string message, params object[] strParams)
        {
            Error_Release(obj, context, message, strParams);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Log_DebugOnly(this System.Object obj, string context, string message, params object[] strParams)
        {
            Log_Release(obj, context, message, strParams);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Warn_DebugOnly(this System.Object obj, string context, string message, params object[] strParams)
        {
            Warn_Release(obj, context, message, strParams);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Error_DebugOnly(this System.Object obj, string context, string message, params object[] strParams)
        {
            Error_Release(obj, context, message, strParams);
        }




        /// <summary>
        /// Some Structured logging to the debug file
        /// </summary>
        /// <param name="Message">Text to be printed - can be formatted as per string.format</param>
        /// <param name="strParams">Objects to feed into a string.format</param>
        public static void Log_Release(this UnityEngine.Object obj, string context, string message, params object[] strParams)
        {
            UnityEngine.Debug.Log(format(obj, context, message, strParams));  
        }

        public static void Warn_Release(this UnityEngine.Object obj, string context, string message, params object[] strParams)
        {
            UnityEngine.Debug.LogWarning(format(obj, context, message, strParams));  
        }

        public static void Error_Release(this UnityEngine.Object obj, string context, string message, params object[] strParams)
        {
            UnityEngine.Debug.LogError(format(obj, context, message, strParams));  
        }

        public static void Log_Release(this System.Object obj, string context, string message, params object[] strParams)
        {
            UnityEngine.Debug.Log(format(obj, context, message, strParams));
        }

        public static void Warn_Release(this System.Object obj, string context, string message, params object[] strParams)
        {
            UnityEngine.Debug.LogWarning(format(obj, context, message, strParams));
        }

        public static void Error_Release(this System.Object obj, string context, string message, params object[] strParams)
        {
            UnityEngine.Debug.LogError(format(obj, context, message, strParams));
        }


        private static string format(this UnityEngine.Object obj, string context, string message, params object[] strParams)
        {
            return format(obj.GetType().Name, context, message, strParams);
        }

        private static string format(this System.Object obj, string context, string message, params object[] strParams)
        {
            return format(obj.GetType().Name, context, message, strParams);
        }

        private static string format(string obj, string context, string message, params object[] strParams)
        {
            string withParams;
            try
            {
                withParams = (strParams == null || strParams.Length == 0 ? message : string.Format(message, strParams));
                // This fills the params into the message
            }
            catch (Exception)
            {
                withParams = "(formatting exception) - " + message;
            }
            string strMessageLine = string.Format("{0},{1},{2},{3}", DateTime.Now, obj, context, withParams);  // This adds our standardised wrapper to each line

            return strMessageLine;
        }
    }
}
