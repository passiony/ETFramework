/*
 * 脚本名称：EventCenter
 * 项目名称：FrameWork
 * 脚本作者：黄哲智
 * 创建时间：2018-01-06 20:16:55
 * 脚本作用：
*/

using System;
using System.Collections.Generic;

namespace ETModel
{
    public class EventCenter
    {
        private static Dictionary<string, GameEventBase> m_EventPool = new Dictionary<string, GameEventBase>();

        #region 根据ID获取相应的事件

        /// <summary>
        /// 根据ID获取相应的事件
        /// </summary>
        /// <typeparam Id="T"></typeparam>
        /// <param id="ID"></param>
        /// <returns></returns>
        public static T GetEvent<T>(string ID) where T : GameEventBase
        {
            if (!m_EventPool.ContainsKey(ID))
            {
                Type t = typeof(T);
                T e = Activator.CreateInstance(t, ID) as T;
                m_EventPool.Add(ID, e);
            }

            return m_EventPool[ID] as T;
        }

        #endregion

        #region 添加事件发布者

        /// <summary>
        /// 添加事件发布者
        /// </summary>
        /// <param id="ID"></param>
        public static void AddDispatcher(string ID)
        {
            if (!m_EventPool.ContainsKey(ID))
            {
                GameEventBase gameEvent = new GameEvent(ID);
                m_EventPool.Add(ID, gameEvent);
            }
        }

        /// <summary>
        /// 添加事件发布者
        /// </summary>
        /// <typeparam id="T"></typeparam>
        /// <param id="ID"></param>
        public static void AddDispatcher<T>(string ID)
        {
            if (!m_EventPool.ContainsKey(ID))
            {
                GameEventBase gameEvent = new GameEvent<T>(ID);
                m_EventPool.Add(ID, gameEvent);
            }
        }

        /// <summary>
        /// 添加事件发布者
        /// </summary>
        /// <typeparam id="T"></typeparam>
        /// <typeparam id="K"></typeparam>
        /// <param id="ID"></param>
        public static void AddDispatcher<T, K>(string ID)
        {
            if (!m_EventPool.ContainsKey(ID))
            {
                GameEventBase gameEvent = new GameEvent<T, K>(ID);
                m_EventPool.Add(ID, gameEvent);
            }
        }

        /// <summary>
        /// 添加事件发布者
        /// </summary>
        /// <typeparam id="T"></typeparam>
        /// <typeparam id="K"></typeparam>
        /// <typeparam Id="L"></typeparam>
        /// <param id="ID"></param>
        public static void AddDispatcher<T, K, L>(string ID)
        {
            if (!m_EventPool.ContainsKey(ID))
            {
                GameEventBase gameEvent = new GameEvent<T, K, L>(ID);
                m_EventPool.Add(ID, gameEvent);
            }
        }

        /// <summary>
        /// 添加事件发布者
        /// </summary>
        /// <typeparam id="T"></typeparam>
        /// <typeparam id="K"></typeparam>
        /// <typeparam id="L"></typeparam>
        /// <typeparam id="M"></typeparam>
        /// <param id="ID"></param>
        public static void AddDispatcher<T, K, L, M>(string ID)
        {
            if (!m_EventPool.ContainsKey(ID))
            {
                GameEventBase gameEvent = new GameEvent<T, K, L, M>(ID);
                m_EventPool.Add(ID, gameEvent);
            }
        }

        public static void AddDispatcher<T, K, L, M, N>(string ID)
        {
            if (!m_EventPool.ContainsKey(ID))
            {
                GameEventBase gameEvent = new GameEvent<T, K, L, M, N>(ID);
                m_EventPool.Add(ID, gameEvent);
            }
        }

        public static void AddDispatcher<T, K, L, M, N, B>(string ID)
        {
            if (!m_EventPool.ContainsKey(ID))
            {
                GameEventBase gameEvent = new GameEvent<T, K, L, M, N, B>(ID);
                m_EventPool.Add(ID, gameEvent);
            }
        }

        #endregion

        #region 分发消息

        public static void Dispatch(string ID)
        {
            if (!m_EventPool.ContainsKey(ID))
            {
                AddDispatcher(ID);
            }

            (m_EventPool[ID] as GameEvent).Invoke(false);
        }


        public static void Dispatch<T>(string ID, T arg)
        {
            if (!m_EventPool.ContainsKey(ID))
            {
                AddDispatcher<T>(ID);
            }

            GameEvent<T> res = (m_EventPool[ID] as GameEvent<T>);
            if (res == null)
            {
                if (m_EventPool.ContainsKey(ID))
                {
                    Log.Error("EventID:" + ID + "的类型是" + m_EventPool[ID].GetType().FullName);
                }
                return;
            }

            res.Invoke(false, arg);
        }

        public static void Dispatch<T, K>(string ID, T arg1, K arg2)
        {
            if (!m_EventPool.ContainsKey(ID))
            {
                AddDispatcher<T, K>(ID);
            }

            GameEvent<T, K> res = (m_EventPool[ID] as GameEvent<T, K>);
            if (res == null)
            {
                if (m_EventPool.ContainsKey(ID))
                {
                    Log.Error("EventID:" + ID + "的类型是" + m_EventPool[ID].GetType().FullName);
                }
                return;
            }

            res.Invoke(false, arg1, arg2);
        }

        public static void Dispatch<T, K, L>(string ID, T arg1, K arg2, L arg3)
        {
            if (!m_EventPool.ContainsKey(ID))
            {
                AddDispatcher<T, K, L>(ID);
            }

            GameEvent<T, K, L> res = (m_EventPool[ID] as GameEvent<T, K, L>);
            if (res == null)
            {
                if (m_EventPool.ContainsKey(ID))
                {
                    Log.Error("EventID:" + ID + "的类型是" + m_EventPool[ID].GetType().FullName);
                }
                return;
            }

            res.Invoke(false, arg1, arg2, arg3);
        }

        public static void Dispatch<T, K, L, M>(string ID, T arg1, K arg2, L arg3, M arg4)
        {
            if (!m_EventPool.ContainsKey(ID))
            {
                AddDispatcher<T, K, L, M>(ID);
            }

            GameEvent<T, K, L, M> res = (m_EventPool[ID] as GameEvent<T, K, L, M>);
            if (res == null)
            {
                if (m_EventPool.ContainsKey(ID))
                {
                    Log.Error("EventID:" + ID + "的类型是" + m_EventPool[ID].GetType().FullName);
                }
                return;
            }

            res.Invoke(false, arg1, arg2, arg3, arg4);
        }

        public static void Dispatch<T, K, L, M, N>(string ID, T arg1, K arg2, L arg3, M arg4, N arg5)
        {
            if (!m_EventPool.ContainsKey(ID))
            {
                AddDispatcher<T, K, L, M, N>(ID);
            }

            GameEvent<T, K, L, M, N> res = (m_EventPool[ID] as GameEvent<T, K, L, M, N>);
            if (res == null)
            {
                if (m_EventPool.ContainsKey(ID))
                {
                    Log.Error("EventID:" + ID + "的类型是" + m_EventPool[ID].GetType().FullName);
                }
                return;
            }

            res.Invoke(false, arg1, arg2, arg3, arg4, arg5);
        }

        public static void Dispatch<T, K, L, M, N, B>(string ID, T arg1, K arg2, L arg3, M arg4, N arg5, B arg6)
        {
            if (!m_EventPool.ContainsKey(ID))
            {
                AddDispatcher<T, K, L, M, N, B>(ID);
            }

            GameEvent<T, K, L, M, N, B> res = (m_EventPool[ID] as GameEvent<T, K, L, M, N, B>);
            if (res == null)
            {
                if (m_EventPool.ContainsKey(ID))
                {
                    Log.Error("EventID:" + ID + "的类型是" + m_EventPool[ID].GetType().FullName);
                }
                return;
            }

            res.Invoke(false, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        public static void WaitDispatch(string ID)
        {
            if (!m_EventPool.ContainsKey(ID))
            {
                AddDispatcher(ID);
            }

            GameEvent res = (m_EventPool[ID] as GameEvent);
            if (res == null)
            {
                if (m_EventPool.ContainsKey(ID))
                {
                    Log.Error("EventID:" + ID + "的类型是" + m_EventPool[ID].GetType().FullName);
                }
                return;
            }

            res.Invoke(true);
        }

        public static void WaitDispatch<T>(string ID, T arg)
        {
            if (!m_EventPool.ContainsKey(ID))
            {
                AddDispatcher<T>(ID);
            }

            GameEvent<T> res = (m_EventPool[ID] as GameEvent<T>);
            if (res == null)
            {
                if (m_EventPool.ContainsKey(ID))
                {
                    Log.Error("EventID:" + ID + "的类型是" + m_EventPool[ID].GetType().FullName);
                }
                return;
            }

            res.Invoke(true, arg);
        }

        public static void WaitDispatch<T, K>(string ID, T arg1, K arg2)
        {
            if (!m_EventPool.ContainsKey(ID))
            {
                AddDispatcher<T, K>(ID);
            }

            GameEvent<T, K> res = (m_EventPool[ID] as GameEvent<T, K>);
            if (res == null)
            {
                if (m_EventPool.ContainsKey(ID))
                {
                    Log.Error("EventID:" + ID + "的类型是" + m_EventPool[ID].GetType().FullName);
                }
                return;
            }

            res.Invoke(true, arg1, arg2);
        }

        public static void WaitDispatch<T, K, L>(string ID, T arg1, K arg2, L arg3)
        {
            if (!m_EventPool.ContainsKey(ID))
            {
                AddDispatcher<T, K, L>(ID);
            }

            GameEvent<T, K, L> res = (m_EventPool[ID] as GameEvent<T, K, L>);
            if (res == null)
            {
                if (m_EventPool.ContainsKey(ID))
                {
                    Log.Error("EventID:" + ID + "的类型是" + m_EventPool[ID].GetType().FullName);
                }
                return;
            }

            res.Invoke(true, arg1, arg2, arg3);
        }

        public static void WaitDispatch<T, K, L, M>(string ID, T arg1, K arg2, L arg3, M arg4)
        {
            if (!m_EventPool.ContainsKey(ID))
            {
                AddDispatcher<T, K, L, M>(ID);
            }

            GameEvent<T, K, L, M> res = (m_EventPool[ID] as GameEvent<T, K, L, M>);
            if (res == null)
            {
                if (m_EventPool.ContainsKey(ID))
                {
                    Log.Error("EventID:" + ID + "的类型是" + m_EventPool[ID].GetType().FullName);
                }
                return;
            }

            res.Invoke(true, arg1, arg2, arg3, arg4);
        }

        public static void WaitDispatch<T, K, L, M, N>(string ID, T arg1, K arg2, L arg3, M arg4, N arg5)
        {
            if (!m_EventPool.ContainsKey(ID))
            {
                AddDispatcher<T, K, L, M, N>(ID);
            }

            GameEvent<T, K, L, M, N> res = (m_EventPool[ID] as GameEvent<T, K, L, M, N>);
            if (res == null)
            {
                if (m_EventPool.ContainsKey(ID))
                {
                    Log.Error("EventID:" + ID + "的类型是" + m_EventPool[ID].GetType().FullName);
                }
                return;
            }

            res.Invoke(true, arg1, arg2, arg3, arg4, arg5);
        }

        public static void WaitDispatch<T, K, L, M, N, B>(string ID, T arg1, K arg2, L arg3, M arg4, N arg5, B arg6)
        {
            if (!m_EventPool.ContainsKey(ID))
            {
                AddDispatcher<T, K, L, M, N, B>(ID);
            }

            GameEvent<T, K, L, M, N, B> res = (m_EventPool[ID] as GameEvent<T, K, L, M, N, B>);
            if (res == null)
            {
                if (m_EventPool.ContainsKey(ID))
                {
                    Log.Error("EventID:" + ID + "的类型是" + m_EventPool[ID].GetType().FullName);
                }
                return;
            }

            res.Invoke(true, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        #endregion

        public static void Register(string key, Action callBack)
        {
            GetEvent<GameEvent>(key).Register(callBack);
        }

        public static void Register<T>(string key, Action<T> callback)
        {
            GetEvent<GameEvent<T>>(key).Register(callback);
        }
        public static void Register<T, K>(string key, Action<T, K> callback)
        {
            GetEvent<GameEvent<T, K>>(key).Register(callback);
        }
        public static void Register<T, K, L>(string key, Action<T, K, L> callback)
        {
            GetEvent<GameEvent<T, K, L>>(key).Register(callback);
        }
        public static void Register<T, K, L, M, N>(string key, Action<T, K, L, M, N> callback)
        {
            GetEvent<GameEvent<T, K, L, M, N>>(key).Register(callback);
        }

        public static void UnRegister(string key, Action callBack)
        {
            GetEvent<GameEvent>(key).UnRegister(callBack);
        }

        public static void UnRegister<T>(string key, Action<T> callback)
        {
            GetEvent<GameEvent<T>>(key).UnRegister(callback);
        }
        public static void UnRegister<T, K>(string key, Action<T, K> callback)
        {
            GetEvent<GameEvent<T, K>>(key).UnRegister(callback);
        }
        public static void UnRegister<T, K, L>(string key, Action<T, K, L> callback)
        {
            GetEvent<GameEvent<T, K, L>>(key).UnRegister(callback);
        }
        public static void UnRegister<T, K, L, M, N>(string key, Action<T, K, L, M, N> callback)
        {
            GetEvent<GameEvent<T, K, L, M, N>>(key).UnRegister(callback);
        }
        public static void Clear(string key)
        {
            if (m_EventPool.ContainsKey(key))
            {
                m_EventPool[key].Clear();
            }
        }
    }

}