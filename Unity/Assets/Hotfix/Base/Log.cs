using System;
using System.Diagnostics;
using UnityEngine;

namespace ETHotfix
{
    public static class Log
    {
        [Conditional("LOGGER_ON")]
        public static void Debug(string msg)
        {
            ETModel.Log.Debug(msg);
        }
        [Conditional("LOGGER_ON")]
        public static void Debug(string msg, Color color)
        {
            ETModel.Log.Debug(msg, color);
        }
        [Conditional("LOGGER_ON")]
        public static void Debug(string message, params object[] args)
        {
            ETModel.Log.Debug(message, args);
        }
        [Conditional("LOGGER_ON")]
        public static void Warning(string msg)
        {
            ETModel.Log.Warning(msg);
        }
        [Conditional("LOGGER_ON")]
        public static void Warning(string message, params object[] args)
        {
            ETModel.Log.Warning(message, args);
        }

        [Conditional("LOGGER_ON")]
        public static void Assert(bool condition, string msg)
        {
            ETModel.Log.Assert(condition, msg);
        }
        
        [Conditional("LOGGER_ON")]
        public static void Msg(object msg)
        {
            ETModel.Log.Msg(Dumper.DumpAsString(msg));
        }
        
        public static void Error(Exception e)
        {
            ETModel.Log.Error(e.ToStr());
        }

        public static void Error(string msg)
        {
            ETModel.Log.Error(msg);
        }
        
        public static void Error(string message, params object[] args)
        {
            ETModel.Log.Error(message, args);
        }
        
        //上报专用接口
        public static void Trace(string msg)
        {
            ETModel.Log.Trace(msg);
        }
    }
}