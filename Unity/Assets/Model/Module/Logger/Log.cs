using System;
using System.Diagnostics;
using UnityEngine;

namespace ETModel
{
	public static class Log
	{
        [Conditional("LOGGER_ON")]
        public static void Debug(string msg)
		{
			UnityEngine.Debug.Log(msg);
		}
        [Conditional("LOGGER_ON")]
        public static void Debug(string message, Color color)
        {
	        UnityEngine.Debug.Log($"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{message}</color>");
        }
        [Conditional("LOGGER_ON")]
        public static void Debug(string message, params object[] args)
        {
	        UnityEngine.Debug.LogFormat(message, args);
        }

        [Conditional("LOGGER_ON")]
        public static void Warning(string msg)
		{
			UnityEngine.Debug.LogWarning(msg);
		}

        [Conditional("LOGGER_ON")]
        public static void Warning(string message, params object[] args)
        {
	        UnityEngine.Debug.LogWarningFormat(message, args);
        }
        
        [Conditional("LOGGER_ON")]
        public static void Assert(bool condition, string msg)
        {
	        // UnityEngine.Debug.Assert(condition, msg);
	        #if DEBUG
	                    System.Diagnostics.Debug.Assert(condition, msg);
	        #else
	                    System.Diagnostics.Trace.Assert(condition, msg);
	        #endif
        }
        
        [Conditional("LOGGER_ON")]
        public static void Msg(object msg)
        {
	        UnityEngine.Debug.Log(Dumper.DumpAsString(msg));
        }
        
		public static void Error(string msg)
		{
 			UnityEngine.Debug.LogError(msg);
        }

        public static void Error(Exception e)
		{
			UnityEngine.Debug.LogException(e);
		}

		public static void Error(string message, params object[] args)
		{
			UnityEngine.Debug.LogErrorFormat(message, args);
		}

		//上报专用接口
		public static void Trace(string msg)
		{
			UnityEngine.Debug.Log(msg);
		}
	}
}