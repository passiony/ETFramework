/*
 * 脚本名称：GameEvent
 * 项目名称：FrameWork
 * 脚本作者：黄哲智
 * 创建时间：2018-01-06 20:08:31
 * 脚本作用：
*/

using System;

namespace ETHotfix
{
    public abstract class GameEventBase
    {
        public string ID { protected set; get; }
        public bool isWait { protected set; get; }
        public bool isDispatch { get; set; }
        public abstract void Clear();
    }

    #region GameEvent

    public class GameEvent : GameEventBase
    {
        private Action m_Action;

        public GameEvent(string ID)
        {
            this.ID = ID;
        }

        public void Register(Action callBack)
        {
            m_Action -= callBack;
            m_Action += callBack;

            if (isWait && isDispatch)
            {
                callBack?.Invoke();
            }
        }

        public void Unregister(Action callBack)
        {
            m_Action -= callBack;
        }

        public override void Clear()
        {
            m_Action = null;
        }

        public void Invoke(bool isWait)
        {
            this.isWait = isWait;
            isDispatch = true;
            if (m_Action != null)
            {
                m_Action.Invoke();
            }
        }
    }

    #endregion

    #region GameEvent<T>

    public class GameEvent<T> : GameEventBase
    {
        private Action<T> m_Action;
        private T arg1;

        public GameEvent(string ID)
        {
            this.ID = ID;
        }

        public void Register(Action<T> callBack)
        {
            m_Action -= callBack;
            m_Action += callBack;
            if (isWait && isDispatch)
            {
                callBack?.Invoke(arg1);
            }
        }

        public void Unregister(Action<T> callBack)
        {
            m_Action -= callBack;
        }

        public override void Clear()
        {
            m_Action = null;
        }

        public void Invoke(bool isWait, T arg)
        {
            this.isWait = isWait;
            isDispatch = true;
            arg1 = arg;
            if (m_Action != null)
            {
                m_Action.Invoke(arg);
            }
        }
    }

    #endregion

    #region GameEvent<T, K>

    public class GameEvent<T, K> : GameEventBase
    {
        private Action<T, K> m_Action;
        private T arg1;
        private K arg2;

        public GameEvent(string ID)
        {
            this.ID = ID;
        }

        public void Register(Action<T, K> callBack)
        {
            m_Action -= callBack;
            m_Action += callBack;
            if (isWait && isDispatch)
            {
                callBack?.Invoke(arg1, arg2);
            }
        }

        public void Unregister(Action<T, K> callBack)
        {
            m_Action -= callBack;
        }

        public override void Clear()
        {
            m_Action = null;
        }

        public void Invoke(bool isWait, T arg1, K arg2)
        {
            this.isWait = isWait;
            isDispatch = true;
            this.arg1 = arg1;
            this.arg2 = arg2;
            if (m_Action != null)
            {
                m_Action.Invoke(arg1, arg2);
            }
        }
    }

    #endregion

    #region GameEvent<T, K, L>

    public class GameEvent<T, K, L> : GameEventBase
    {
        private Action<T, K, L> m_Action;
        private T arg1;
        private K arg2;
        private L arg3;

        public GameEvent(string ID)
        {
            this.ID = ID;
        }

        public void Register(Action<T, K, L> callBack)
        {
            m_Action -= callBack;
            m_Action += callBack;
            if (isWait && isDispatch)
            {
                callBack(arg1, arg2, arg3);
            }
        }

        public void Unregister(Action<T, K, L> callBack)
        {
            m_Action -= callBack;
        }

        public override void Clear()
        {
            m_Action = null;
        }

        public void Invoke(bool isWait, T arg1, K arg2, L arg3)
        {
            this.isWait = isWait;
            isDispatch = true;
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.arg3 = arg3;
            if (m_Action != null)
            {
                m_Action.Invoke(arg1, arg2, arg3);
            }
        }
    }

    #endregion

    #region GameEvent<T, K, L, M> 

    public class GameEvent<T, K, L, M> : GameEventBase
    {
        private Action<T, K, L, M> m_Action;
        private T arg1;
        private K arg2;
        private L arg3;
        private M arg4;

        public GameEvent(string ID)
        {
            this.ID = ID;
        }

        public void Register(Action<T, K, L, M> callBack)
        {
            m_Action -= callBack;
            m_Action += callBack;
            if (isWait && isDispatch)
            {
                callBack?.Invoke(arg1, arg2, arg3, arg4);
            }
        }

        public void Unregister(Action<T, K, L, M> callBack)
        {
            m_Action -= callBack;
        }

        public override void Clear()
        {
            m_Action = null;
        }

        public void Invoke(bool isWait, T arg1, K arg2, L arg3, M arg4)
        {
            this.isWait = isWait;
            isDispatch = true;
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.arg3 = arg3;
            this.arg4 = arg4;
            if (m_Action != null)
            {
                m_Action.Invoke(arg1, arg2, arg3, arg4);
            }
        }
    }

    #endregion

    #region GameEvent<T, K, L, M,N> 

    public class GameEvent<T, K, L, M, N> : GameEventBase
    {
        private Action<T, K, L, M, N> m_Action;
        private T arg1;
        private K arg2;
        private L arg3;
        private M arg4;
        private N arg5;

        public GameEvent(string ID)
        {
            this.ID = ID;
        }

        public void Register(Action<T, K, L, M, N> callBack)
        {
            //m_Action -= callBack;
            m_Action += callBack;
            if (isWait && isDispatch)
            {
                callBack?.Invoke(arg1, arg2, arg3, arg4, arg5);
            }
        }

        public void Unregister(Action<T, K, L, M, N> callBack)
        {
            m_Action -= callBack;
        }

        public override void Clear()
        {
            m_Action = null;
        }

        public void Invoke(bool isWait, T arg1, K arg2, L arg3, M arg4, N arg5)
        {
            this.isWait = isWait;
            isDispatch = true;
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.arg3 = arg3;
            this.arg4 = arg4;
            this.arg5 = arg5;
            if (m_Action != null)
            {
                m_Action.Invoke(arg1, arg2, arg3, arg4, arg5);
            }
        }
    }

    #endregion

    #region GameEvent<T, K, L, M,N,B> 

    public class GameEvent<T, K, L, M, N, B> : GameEventBase
    {
        private Action<T, K, L, M, N, B> m_Action;
        private T arg1;
        private K arg2;
        private L arg3;
        private M arg4;
        private N arg5;
        private B arg6;

        public GameEvent(string ID)
        {
            this.ID = ID;
        }

        public void Register(Action<T, K, L, M, N, B> callBack)
        {
            //m_Action -= callBack;
            m_Action += callBack;
            if (isWait && isDispatch)
            {
                callBack?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
            }
        }

        public void Unregister(Action<T, K, L, M, N, B> callBack)
        {
            m_Action -= callBack;
        }

        public override void Clear()
        {
            m_Action = null;
        }

        public void Invoke(bool isWait, T arg1, K arg2, L arg3, M arg4, N arg5, B arg6)
        {
            this.isWait = isWait;
            isDispatch = true;
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.arg3 = arg3;
            this.arg4 = arg4;
            this.arg5 = arg5;
            this.arg6 = arg6;
            if (m_Action != null)
            {
                m_Action.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
            }
        }
    }

    #endregion
}