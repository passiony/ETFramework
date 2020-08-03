using ETModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ETHotfix
{
    public class UILayer : UIBaseComponent
    {
        public const int MaxOderPerWindow = 10;

        public int topWindowOrder;
        public int minWindowOrder;

        public override void Init(UIBaseComponent _holder, GameObject go, params object[] args)
        {
            base.Init(_holder, go, args);
            LayerConfig layer = args[0] as LayerConfig;

            this.topWindowOrder = layer.OrderInLayer;
            this.minWindowOrder = layer.OrderInLayer;
        }

        // pop window order
        public int PopWindowOder()
        {
            var cur = this.topWindowOrder;
            this.topWindowOrder += MaxOderPerWindow;
            return cur;
        }

        // push window order
        public void PushWindowOrder()
        {
            Log.Assert(topWindowOrder > minWindowOrder, this.name + "Window Order Error");
            this.topWindowOrder -= MaxOderPerWindow;
        }

    }
}