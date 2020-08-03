using ETModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ETHotfix
{
    public class UILine : UIBaseComponent
    {
        protected LineRenderer render;
        protected int relativeOrder;

        protected int cornerVertices = 20;//线段拐角的点数(平滑度)
        protected int capVertices = 50;//线段两点的点数(圆滑度)
        protected float lineWidth = 0.6f;//线段的宽度
        protected Color lineColor = Color.black;
        protected string sortingLayerName = "Default";

        public override void Init(UIBaseComponent _holder, GameObject go, params object[] args)
        {
            base.Init(_holder, go, args);
            Log.Assert(args.Length >= 1, "raletiveOrder is needed");

            this.relativeOrder = System.Convert.ToInt32(args[0]);
        }

        public override void Awake()
        {
            base.Awake();

            this.render = gameObject.GetComponent<LineRenderer>();
            if (render == null)
                render = gameObject.AddComponent<LineRenderer>();

            // 设置layer
            gameObject.layer = LayerMask.NameToLayer("UI");
            render.sortingLayerName = sortingLayerName;
            render.numCornerVertices = cornerVertices;
            render.numCapVertices = capVertices;
            render.startWidth = lineWidth;
            render.endWidth = lineWidth;
            render.startColor = lineColor;
            render.endColor = lineColor;
            render.useWorldSpace = true;
            render.alignment = LineAlignment.TransformZ;
            render.positionCount = 0;

            SetOrder(relativeOrder);
        }

        public override void Enable()
        {
            base.Enable();
            SetOrder(this.relativeOrder);
        }

        public virtual void SetOrder(int relative_order)
        {
            Log.Assert(relativeOrder >= 0, "Relative order must be nonnegative number!");
            Log.Assert(relativeOrder < UILayer.MaxOderPerWindow, "Relative order larger then MaxOderPerWindow!");

            this.relativeOrder = relative_order;
            render.sortingOrder = this.view.base_order + relative_order;
        }

        public virtual int GetOrder()
        {
            return relativeOrder;
        }

        public virtual void SetColor(Color color)
        {
            lineColor = color;

            render.startColor = color;
            render.endColor = color;
        }

        public virtual void SetLineWidth(float width)
        {
            lineWidth = width;

            render.startWidth = width;
            render.endWidth = width;
        }

        //一条线绘制多个点
        public virtual void DrawPoints(Vector3[] posArry)
        {
            if (posArry == null || posArry.Length < 2)
            {
                render.positionCount = 0;
                return;
            }

            render.positionCount = posArry.Length;
            render.SetPositions(posArry);
        }

        public virtual void DrawPoints(GameObject[] goes)
        {
            if (goes == null || goes.Length < 2)
            {
                render.positionCount = 0;
                return;
            }

            List<Vector3> posArry = new List<Vector3>();
            foreach (var item in goes)
            {
                item.transform.position = new Vector3(item.transform.position.x, item.transform.position.y, item.transform.position.z);
                posArry.Add(item.transform.position);
            }
            DrawPoints(posArry.ToArray());
        }

        public virtual void Clear()
        {
            render.positionCount = 0;
        }

        public override void Destroy()
        {
            base.Destroy();

            render.positionCount = 0;
            GameObjectPool.Instance.RecycleGameObject(gameObject);
        }
    }
}