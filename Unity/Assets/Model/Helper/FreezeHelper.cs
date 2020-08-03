using UnityEngine;

/// <summary>
/// 冻结窗口、顶层遮罩，打开工具
/// </summary>
namespace ETModel
{
    public static class FreezeHelper
    {
        /// <summary>
        /// 透明层冻结窗口
        /// </summary>
        public static void FreezeUI(object key)
        {
            var window = UIComponent.Instance.Get(UIType.UIFreeze);
            if (window == null) return;
            var freeze = window.View as UIFreezeView;
            freeze.FreezeUI(key);
        }

        /// <summary>
        /// 指定颜色背景 冻结窗口
        /// </summary>
        public static void FreezeUIWithColor(object key, Color color)
        {
            var window = UIComponent.Instance.Get(UIType.UIFreeze);
            if (window == null) return;
            var freeze = window.View as UIFreezeView;
            freeze.FreezeUIWithColor(key, color);
        }

        /// <summary>
        /// 指定文本提示 冻结窗口
        /// </summary>
        public static void FreezeUIWithText(object key, string text)
        {
            var window = UIComponent.Instance.Get(UIType.UIFreeze);
            if (window == null) return;
            var freeze = window.View as UIFreezeView;
            freeze.FreezeUIWithText(key, text);
        }

        /// <summary>
        /// loading图片转动 冻结窗口
        /// </summary>
        public static void FreezeUIWithLoading(object key)
        {
            var window = UIComponent.Instance.Get(UIType.UIFreeze);
            if (window == null) return;
            var freeze = window.View as UIFreezeView;
            freeze.FreezeUIWithLoading(key);
        }

        /// <summary>
        /// 延迟自动关闭 冻结窗口
        /// </summary>
        public static void FreezeUIWithTime(object key, float time)
        {
            var window = UIComponent.Instance.Get(UIType.UIFreeze);
            if (window == null) return;
            var freeze = window.View as UIFreezeView;
            freeze.FreezeUIWithTime(key, time);
        }

        /// <summary>
        /// 解冻窗口
        /// </summary>
        /// <param name="key"></param>
        public static void UnFreezeUI(object key)
        {
            var window = UIComponent.Instance.Get(UIType.UIFreeze);
            if (window == null) return;
            var freeze = window.View as UIFreezeView;
            freeze.UnFreezeUI(key);
        }

        /// <summary>
        /// 关闭所有上层冻结窗口
        /// </summary>
        public static void UnFreezeAll()
        {
            var window = UIComponent.Instance.Get(UIType.UIFreeze);
            if (window == null) return;
            var freeze = window.View as UIFreezeView;
            freeze.UnFreezeAll();
        }
    }
}