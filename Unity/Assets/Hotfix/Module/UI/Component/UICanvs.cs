using UnityEngine;
using UnityEngine.UI;

//
// UICanvas：对UI子窗口的Canvas的封装，方便控制相对层级
// 注意：
// 1、为了调整UI层级，所以这里的overrideSorting设置为true
// 2、如果只是类似NGUI的Panel那样划分drawcall管理，直接在预设上添加Canvas，并设置overrideSorting为false
// 3、这里的order是相对于window.view中base_order的差量，窗口内的order最多为10个//-UIManager中配置
// 4、旧窗口内所有canvas的real_order都应该在新窗口之下，即保证旧窗口内包括UI特效在内的所有组件，不会跑到新窗口之上
// 5、UI逻辑代码禁止手动直接设置Unity侧Cavans组件的orderInLayer，全部使用本脚本接口调整层级，避免层级混乱
//

namespace ETHotfix
{
    public class UICanvas : UIBaseComponent
    {
        public Canvas canvas { get; private set; }
        public GraphicRaycaster raycaster { get; private set; }
        public int relativeOrder { get; private set; }

        public override void Init(UIBaseComponent _holder, GameObject go, params object[] args)
        {
            base.Init(_holder, go, args);
            Log.Assert(args.Length >= 1,"raletiveOrder is needed");

            this.relativeOrder = System.Convert.ToInt32(args[0]);
        }

        public override void Awake()
        {
            base.Awake();

            //canvas
            this.canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null)
                canvas = gameObject.AddComponent<Canvas>();

            // raycaster
            raycaster = gameObject.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
                raycaster = gameObject.AddComponent<GraphicRaycaster>();

            canvas.overrideSorting = true;
            SetOrder(relativeOrder);
        }

        public override void Enable()
        {
            base.Enable();
            SetOrder(this.relativeOrder);
        }

        public virtual void SetOrder(int relative_order)
        {
            Log.Assert(relativeOrder>=0, "Relative order must be nonnegative number!");
            Log.Assert(relativeOrder < UILayer.MaxOderPerWindow, "Relative order larger then MaxOderPerWindow!");

            this.relativeOrder = relative_order;
            canvas.sortingOrder = this.view.base_order + relative_order;
        }

        public virtual int GetOrder()
        {
            return relativeOrder;
        }

        public override void Destroy()
        {
            base.Destroy();
            // raycaster
            if (raycaster != null)
                GameObject.Destroy(raycaster);
            
            //canvas
            if (canvas != null)
                GameObject.Destroy(this.canvas);
        }
    }
}