using ETModel;
using UnityEngine;

/// <summary>
/// 冻结窗口、顶层遮罩，打开工具
/// </summary>
namespace ETHotfix
{
    public static class FreezeHelper
    {
        /// <summary>
        /// 透明层冻结窗口
        /// </summary>
        public static void FreezeUI(object key)
        {
            ETModel.FreezeHelper.FreezeUI(key);
        }

        /// <summary>
        /// 指定颜色背景 冻结窗口
        /// </summary>
        public static void FreezeUIWithColor(object key, Color color)
        {
            ETModel.FreezeHelper.FreezeUIWithColor(key, color);
        }

        /// <summary>
        /// 指定文本提示 冻结窗口
        /// </summary>
        public static void FreezeUIWithText(object key, string text)
        {
            ETModel.FreezeHelper.FreezeUIWithText(key, text);
        }

        /// <summary>
        /// loading图片转动 冻结窗口
        /// </summary>
        public static void FreezeUIWithLoading(object key)
        {
            ETModel.FreezeHelper.FreezeUIWithLoading(key);
        }

        /// <summary>
        /// 延迟自动关闭 冻结窗口
        /// </summary>
        public static void FreezeUIWithTime(object key, float time)
        {
            ETModel.FreezeHelper.FreezeUIWithTime(key, time);
        }

        /// <summary>
        /// 解冻窗口
        /// </summary>
        /// <param name="key"></param>
        public static void UnFreezeUI(object key)
        {
            ETModel.FreezeHelper.UnFreezeUI(key);
        }

        /// <summary>
        /// 关闭所有上层冻结窗口
        /// </summary>
        public static void UnFreezeAll()
        {
            ETModel.FreezeHelper.UnFreezeAll();
        }
    }
}