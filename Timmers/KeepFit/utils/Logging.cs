using System;
using UnityEngine;
using System.Collections.Generic;

namespace KeepFit
{
    public static class Logging
    {
        private static readonly int maxLogLines = 500;
        private static List<string> logLines = new List<String>(maxLogLines);

        public static ICollection<string> GetLogBuffer()
        {
            return logLines;
        }
        /// <summary>
        /// Some Structured logging to the debug file - ONLY RUNS WHEN DLL COMPILED IN DEBUG MODE
        /// </summary>
        /// <param name="Message">Text to be printed - can be formatted as per string.format</param>
        /// <param name="strParams">Objects to feed into a string.format</param>
        public static void Log_DebugOnly(this UnityEngine.Object obj, string context, string message, params object[] strParams)
        {
            string formatted = format(obj, context, message, strParams);
            addLogLine(formatted);
            Log_DebugOnly(formatted);
        }

        public static void Log_DebugOnly(this System.Object obj, string context, string message, params object[] strParams)
        {
            string formatted = format(obj, context, message, strParams);
            addLogLine(formatted);
            Log_DebugOnly(formatted);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void Log_DebugOnly(string formatted)
        {
            Log_Release(formatted);
        }

        public static void Warn_DebugOnly(this UnityEngine.Object obj, string context, string message, params object[] strParams)
        {
            string formatted = format(obj, context, message, strParams);
            addLogLine(formatted);
            Warn_DebugOnly(formatted);
        }

        public static void Warn_DebugOnly(this System.Object obj, string context, string message, params object[] strParams)
        {
            string formatted = format(obj, context, message, strParams);
            addLogLine(formatted);
            Warn_DebugOnly(formatted);
        }
        
        [System.Diagnostics.Conditional("DEBUG")]
        private static void Warn_DebugOnly(string formatted)
        {
            Warn_Release(formatted);
        }

        public static void Error_DebugOnly(this UnityEngine.Object obj, string context, string message, params object[] strParams)
        {
            string formatted = format(obj, context, message, strParams);
            addLogLine(formatted);
            Error_DebugOnly(formatted);
        }
        
        public static void Error_DebugOnly(this System.Object obj, string context, string message, params object[] strParams)
        {
            string formatted = format(obj, context, message, strParams);
            addLogLine(formatted);
            Error_DebugOnly(formatted);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void Error_DebugOnly(string formatted)
        {
            Error_Release(formatted);
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

        public static void Log_Release(this System.Object obj, string context, string message, params object[] strParams)
        {
            UnityEngine.Debug.Log(format(obj, context, message, strParams));
        }

        private static void Log_Release(string formatted)
        {
            UnityEngine.Debug.Log(formatted);
        }

        public static void Warn_Release(this UnityEngine.Object obj, string context, string message, params object[] strParams)
        {
            UnityEngine.Debug.LogWarning(format(obj, context, message, strParams));  
        }

        public static void Warn_Release(this System.Object obj, string context, string message, params object[] strParams)
        {
            UnityEngine.Debug.LogWarning(format(obj, context, message, strParams));
        }

        private static void Warn_Release(string formatted)
        {
            UnityEngine.Debug.LogWarning(formatted);
        }
        
        
        public static void Error_Release(this UnityEngine.Object obj, string context, string message, params object[] strParams)
        {
            UnityEngine.Debug.LogError(format(obj, context, message, strParams));  
        }


        public static void Error_Release(this System.Object obj, string context, string message, params object[] strParams)
        {
            UnityEngine.Debug.LogError(format(obj, context, message, strParams));
        }


        private static void Error_Release(string formatted)
        {
            UnityEngine.Debug.LogError(formatted);
        }

        private static void addLogLine(string logLine)
        {
            if (logLines.Count >= maxLogLines)
            {
                logLines.RemoveAt(0);
            }
            logLines.Add(logLine);
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
