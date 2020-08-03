/// <summary>
/// ui的ctrl层
/// 1、用于界面逻辑，接受view层输入，做逻辑处理
/// 2、ctrl层不做界面显示，不做数据存储，只写逻辑
/// 3、ctrl层不修改model数据
/// </summary>

namespace ETModel
{
	public class UIBaseCtrl
	{
        protected UIBaseModel model;

        public void Init(UIBaseModel _model)
        {
            model = _model;
            this.Awake();
        }

		public virtual void Awake()
		{
            
		}

        public virtual void Destroy()
        {

        }
	}
}
