using System;
using System.Collections.Generic;

namespace ETModel
{
    public static class UIType
    {
        public static UIConfig UILaunch = new UIConfig(
            "UILaunch",
            "UI/Common/UILaunch.prefab",
            ELayer.Top,
            null,
            typeof(UILaunchView),
            null);

        public static UIConfig UINoticeTip = new UIConfig(
            "UINoticeTip",
            "UI/Common/UINoticeTip.prefab",
            ELayer.Top,
            null,
            typeof(UINoticeTipView),
            null);
        
        /// <summary>
        /// 冻结窗口/顶层遮罩
        /// </summary>
        public static ETModel.UIConfig UIFreeze = new ETModel.UIConfig(
            "UIFreeze",
            "UI/Common/UIFreeze.prefab",
            ELayer.Top,
            EAnimType.None,
            null,
            typeof(UIFreezeView),
            null);
    }
}