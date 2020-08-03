using ETModel;
using UnityEngine;

/// <summary>
/// UI特效的封装，方便处理相对层级
/// </summary>
namespace ETHotfix
{
    public class UIEffect : UIBaseComponent
    {
        protected Renderer[] renders;
        protected int relativeOrder;

        protected int sortingOrder;
        protected string sortingLayerName;

        public override void Init(UIBaseComponent _holder, GameObject go, params object[] args)
        {
            base.Init(_holder, go, args);
            if (args.Length < 1)
            {
                Log.Error("raletive order is needed!!!");
                return;
            }

            relativeOrder = System.Convert.ToInt32(args[0]);
            getRenders();
            SetOrder(relativeOrder);
        }

        public override void Enable()
        {
            base.Enable();
            SetOrder(relativeOrder);
        }

        public virtual void SetOrder(int raletive_order)
        {
            Log.Assert(raletive_order >= 0, "Relative order must be nonnegative number!");
            Log.Assert(raletive_order < UILayer.MaxOderPerWindow, "Relative order larger then MaxOderPerWindow!");

            this.relativeOrder = raletive_order;
            setSortingOrder(this.view.base_order + relativeOrder);
        }

        public virtual int GetOrder()
        {
            return this.relativeOrder;
        }
        
        public virtual void SetSortingName(string name)
        {
            sortingLayerName = name;
            foreach (var render in renders)
            {
                render.sortingLayerName = name;
            }
        }

        public virtual string GetSortingName()
        {
            return sortingLayerName;
        }

        protected virtual void setSortingOrder(int order)
        {
            foreach (var render in renders)
            {
                render.sortingOrder = order;
            }
        }

        protected virtual void getRenders()
        {
            renders = gameObject.GetComponentsInChildren<Renderer>();
        }

        /// <summary>
        /// 克隆一个一模一样的特效游戏对象
        /// </summary>
        /// <param name="fromPool">是否从对象池取</param>
        /// <returns>克隆的对象</returns>
        public virtual GameObject CloneEffect(bool fromPool,Transform parent)
        {
            GameObject clone = null;
            if(fromPool)
            {
                clone = GameObjectPool.Instance.GetGameObject(gameObject, parent);
            }
            else
            {
                clone = GameObject.Instantiate(gameObject);
                clone.transform.SetParent(parent, false);
            }

            //set order
            var childs = clone.GetComponentsInChildren<Renderer>();
            foreach (var item in childs)
            {
                item.sortingOrder = this.view.base_order + relativeOrder;
            }
            clone.SetActive(true);
            return clone;
        }
    }
}