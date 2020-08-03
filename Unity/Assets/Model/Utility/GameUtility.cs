using System;
using UnityEngine;
using UnityEngine.UI;

namespace ETModel
{
    public static class GameUtility
    {
        #region Async-Await
        
        /// <summary>
        /// 延迟执行函数
        /// </summary>
        public static async ETVoid DelayAction(int delay,Action action)
        {
            await TimerComponent.Instance.WaitAsync(delay);
            action?.Invoke();
        }

        /// <summary>
        /// Foreach延迟遍历
        /// </summary>
        public static async ETTask ForeachAsync<T>(T[] list, int interval, Action<T> callback, Action complete)
        {
            foreach (var item in list)
            {
                callback?.Invoke(item);
                await TimerComponent.Instance.WaitAsync(interval);
            }
            complete?.Invoke();
        }

        /// <summary>
        /// Foreach延迟遍历
        /// </summary>
        /// <param name="cancel">可取消</param>
        public static async ETTask ForeachAsync<T>(T[] list, int interval, Action<T> callback, Action complete, ETCancellationToken cancel)
        {
            foreach (var item in list)
            {
                callback?.Invoke(item);
                await TimerComponent.Instance.WaitAsync(interval, cancel);
            }
            complete?.Invoke();
        }

        /// <summary>
        /// For延迟遍历
        /// </summary>
        public static async ETTask ForAsync(int init, int length, int interval, Action<int> callback, Action complete)
        {
            for (int i = init; i < init + length; i++)
            {
                callback?.Invoke(i);
                await TimerComponent.Instance.WaitAsync(interval);
            }
            complete?.Invoke();
        }

        /// <summary>
        /// For延迟遍历
        /// </summary>
        /// <param name="cancel">可取消</param>
        public static async ETTask ForAsync(int init, int length, int interval, Action<int> callback, Action complete, ETCancellationToken cancel)
        {
            for (int i = init; i < init + length; i++)
            {
                callback?.Invoke(i);
                await TimerComponent.Instance.WaitAsync(interval, cancel);
            }
            complete?.Invoke();
        }


        /// <summary>
        /// 仿unity协程，WaitUntil：直到为true跳出
        /// </summary>
        public static async ETTask WaitUntil(Func<bool> predicate)
        {
            while (!predicate())
            {
                await TimerComponent.Instance.WaitAsync(30);
            }
        }

        /// <summary>
        /// 仿unity协程，WaitWhile：直到为false跳出
        /// </summary>
        public static async ETTask WaitWhile(Func<bool> predicate)
        {
            while (predicate())
            {
                await TimerComponent.Instance.WaitAsync(20);
            }
        }
        
        #endregion
        
        
        /// <summary>
        /// 设置Tag
        /// </summary>
        public static void SetTag(GameObject go, string tag,bool recursive=false)
        {
            if (null == go)
                return;

            if (string.IsNullOrEmpty(tag))
                return;

            go.tag = tag;

            if (recursive)
            {
                for (int i = 0; i < go.transform.childCount; i++)
                {
                    SetTag(go.transform.GetChild(i).gameObject, tag, true);
                }
            }
        }
        
        /// <summary>  
        /// 网络可用否
        /// </summary>  
        public static bool NetAvailable
        {
            get
            {
                return Application.internetReachability != NetworkReachability.NotReachable;
            }
        }

        /// <summary>  
        /// WIFI否
        /// </summary>  
        public static bool IsWifi
        {
            get
            {
                return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
            }
        }

        /// <summary>
        /// 获取适配比例
        /// </summary>
        public static float GetAdaptation()
        {
            float iphoneX = 1125 / (float)2436;
            float pad = 3 / (float)4;
            float x = Screen.width / (float)Screen.height;
            float k = 1 / (pad - iphoneX);
            float b = iphoneX / (iphoneX - pad);
            return Mathf.Clamp(k * x + b, 0, 1);
        }

        /// <summary>
        /// 是否是Pad机型
        /// </summary>
        /// <returns></returns>
        public static bool IsPad()
        {
#if UNITY_IPHONE || UNITY_IOS
            string a = SystemInfo.deviceModel.ToLower().Trim();
            Log.Debug("IsPad:" + SystemInfo.deviceName);
            if (a.StartsWith("ipad"))
            {
                return true;
            }
            else
            {
                return false;
            }
#elif UNITY_ANDROID
            
#endif
            return false;
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        }

        /// <summary>
        /// 获取时间戳，指定时间
        /// </summary>
        public static long GetTimeStamp(DateTime dateTime)
        {
            TimeSpan ts = dateTime - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds);
        }
        
        /// <summary>
        /// 返回Int数据中某一位是否为1
        /// </summary>
        /// <param name="value"></param>
        /// <param name="index">32位数据的从右向左的偏移位索引(0~31)</param>
        /// <returns>true表示该位为1，false表示该位为0</returns>
        public static bool GetBitValue(int value, int index)
        {
            if (index > 31) 
                throw new ArgumentOutOfRangeException("index"); //索引出错
            var val = 1 << index;
            return (value & val) == val;
        }
    }
}