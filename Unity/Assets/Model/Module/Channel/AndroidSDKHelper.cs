using System;
using UnityEngine;

namespace ETModel
{
    public class AndroidSDKHelper
    {
        public static void FuncCall(string methodName, params object[] param)
        {
#if UNITY_ANDROID
            try
            {
                AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
                if (jo != null)
                {
                    jo.Call(methodName, param);
                }
            }
            catch (Exception ex)
            {
                Log.Debug("call sdk get exception methodName:" + methodName + " message: " + ex.Message);
            }
#endif
        }
    }
}
