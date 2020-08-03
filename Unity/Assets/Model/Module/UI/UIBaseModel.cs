/// <summary>
/// ui的model层
/// 1、用于存储页面数据，不做界面和逻辑
/// 2、打开页面时，首先进入model，进行数据初始化，然后进入view和ctrl可以直接使用model数据
/// 3、view和ctrl只能使用model数据，不能修改model数据
/// 4、model数据只能通过数据中心发送事件修改，然后发送ui事件通知view层
/// 5、model之存储页面相关数据，注意：单个UI页面数据，不存储游戏数据，游戏持久化数据由数据中心提供
/// </summary>

namespace ETModel
{

    public class UIBaseModel
	{
        public UIBaseModel()
        {
            Awake();
        }

        public virtual void Awake()
        {

        }

		public virtual void Enable(object[] args)
		{
            AddDataListener();
        }

        public virtual void Disable()
        {
            RemoveDataListener();
        }

        public virtual void Destroy()
        {

        }

        protected virtual void AddDataListener()
        {

        }

        protected virtual void RemoveDataListener()
        {

        }
    }
}
