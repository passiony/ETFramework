using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// UIEffect的扩展
/// 功能：保留子节点相对于特效根节点的sortingOrder
///       用法和UIEffect一样
/// </summary>
namespace ETHotfix
{
    public class UIEffectChildOrder : UIEffect
    {
        public int[] rendersOrder;

        protected override void getRenders()
        {
            renders = gameObject.GetComponentsInChildren<Renderer>(true);
            rendersOrder = new int[renders.Length];
            for (int i = 0; i < renders.Length; i++)
            {
                rendersOrder[i] = renders[i].sortingOrder;
            }
        }

        protected override void setSortingOrder(int order)
        {
            for (int i = 0; i < renders.Length; i++)
            {
                var render = renders[i];
                int childOrder = rendersOrder[i];

                render.sortingOrder = childOrder + order;
            }
        }
    }
}