
using UnityEngine;
using ETModel;

/// <summary>
/// ui的view层
/// 1、用于界面显示和操作，使用model的数据进行初始化
/// 2、view层不允许修改model数据，只能访问model数据
/// 3、view层不写游戏逻辑，逻辑放到ctrl中
/// </summary>
namespace ETHotfix
{
    public class UIBaseView : UIBaseContainer
	{
        public UIWindow window { get; protected set; }
        public UICanvas canvas { get; protected set; }
        public UIBaseModel model { get; protected set; }
        public UIBaseCtrl ctrl { get; protected set; }

        public int base_order { get; protected set; }
        public UILayer Holder { get { return holder as UILayer; } }

        protected ResourceCache cache;
        public ResourceCache Cache
        {
            get
            {
                if(cache == null)
                {
                    cache = ResourceCacheManager.Instance.Get(window.Config);
                }
                return cache;
            }
        }

        public void _init(UIWindow _window, UIBaseModel _model, UIBaseCtrl _ctrl)
        {
            window = _window;
            model = _model;
            ctrl = _ctrl;
        }

        public override void Init(UIBaseComponent _holder, GameObject go, params object[] args)
        {
            base.Init(_holder, go, args);
            this.canvas = this.AddComponent<UICanvas>(go, 0);
            // 初始化RectTransform
            this.rectTransform.offsetMax = Vector2.zero;
            this.rectTransform.offsetMin = Vector2.zero;
            this.rectTransform.localScale = Vector3.one;
            this.rectTransform.localPosition = Vector3.zero;
        }

        public override void Awake()
		{
            base.Awake();
        }

		public override void Enable()
        {
            base_order = Holder.PopWindowOder();
            base.Enable();
            AddUIListener();
        }

        public override void Disable()
        {
            base.Disable();
            RemoveUIListener();
            Holder.PushWindowOrder();
        }

        public virtual void OnBack()
        {

        }

        public override void Destroy()
        {
            base.Destroy();
            ClearResourceCache();
        }

        protected virtual void AddUIListener()
        {

        }

        protected virtual void RemoveUIListener()
        {

        }

        protected virtual void ClearResourceCache()
        {
            if (cache != null)
            {
                ResourceCacheManager.Instance.Remove(window.Name, true);
            }
        }

        /// <summary>
        /// 设置嵌套子UI层级
        /// </summary>
        /// <param name="child">子UI游戏对象，必须有Canvas组件</param>
        /// <param name="relative_order"></param>
        protected virtual void SetCanvasOrder(UnityEngine.GameObject child, int relative_order)
        {
            UnityEngine.Canvas canvas = child.GetComponent<UnityEngine.Canvas>();
            if(canvas!=null)
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = base_order + relative_order;
            }
        }

        protected virtual void SetCanvasOrder(UnityEngine.Transform child, int relative_order)
        {
            SetCanvasOrder(child.gameObject, relative_order);
        }

        protected virtual void CloseSelf()
        {
            UIComponent.Instance.Close(this.window.Config);
        }

        protected virtual void DestroySelf()
        {
            UIComponent.Instance.Destroy(this.window.Config);
        }

        #region 动画
        public virtual void EnterAnim(System.Action callback)
        {
            // var group = gameObject.GetComponent<CanvasGroup>();
            // if (group == null)
            //     group = gameObject.AddComponent<CanvasGroup>();
            //
            // DOTween.Kill(group);
            // group.alpha = 0;
            // group.DOFade(1, window.Config.Duration).OnComplete(()=> {
            //     callback();
            // });
        }

        public virtual void OnCompleteEnterAnim()
        {

        }

        public virtual void ExitAnim(System.Action callback)
        {
            // var group = gameObject.GetComponent<CanvasGroup>();
            // if (group == null)
            //     group = gameObject.AddComponent<CanvasGroup>();
            //
            // DOTween.Kill(group);
            // group.alpha = 1;
            // group.DOFade(0, window.Config.Duration).OnComplete(() => {
            //     callback();
            // });
        }

        public virtual void OnCompleteExitAnim()
        {

        }
        #endregion
    }
}
