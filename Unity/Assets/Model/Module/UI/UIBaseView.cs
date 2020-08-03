/// <summary>
/// ui的view层
/// 1、用于界面显示和操作，使用model的数据进行初始化
/// 2、view层不允许修改model数据，只能访问model数据
/// 3、view层不写游戏逻辑，逻辑放到ctrl中
/// </summary>

namespace ETModel
{
    public class UIBaseView : UIContainer
	{
        protected UIWindow  window;
        protected UIBaseModel model;
        protected UIBaseCtrl ctrl;

        protected int base_order = 0;

        public void Init(UIWindow  _holder, UIBaseModel _model, UIBaseCtrl _ctrl)
        {
            window = _holder;
            model = _model;
            ctrl = _ctrl;
        }

		public override void Awake()
		{
            base.Awake();
		}

		public override void Enable()
        {
            base_order = window.PopWindowOder();
            base.Enable();
            AddUIListener();
        }

        public override void Disable()
        {
            base.Disable();
            RemoveUIListener();
            window.PushWindowOrder();
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        protected virtual void AddUIListener()
        {

        }

        protected virtual void RemoveUIListener()
        {

        }

        protected virtual void SetOrder(UnityEngine.Canvas canvas,int relative_order)
        {
            canvas.sortingOrder = base_order + relative_order;
        }

        #region 动画
        public virtual void EnterAnim(System.Action callback)
        {
            //AnimSystem.UguiAlpha(gameObject, 0, 1, ()=> {
            //    callBack();
            //    OnCompleteEnterAnim();
            //});
        }

        public virtual void OnCompleteEnterAnim()
        {

        }

        public virtual void ExitAnim(System.Action callback)
        {
            //AnimSystem.UguiAlpha(gameObject, 1, 0, ()=> {
            //    callBack();
            //    OnCompleteExitAnim();
            //});
        }

        public virtual void OnCompleteExitAnim()
        {

        }
        #endregion
    }
}
