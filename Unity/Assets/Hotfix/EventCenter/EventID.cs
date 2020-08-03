using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 事件类型：枚举方式
/// 命名方式：
/// 1.统一大写，下划线连接单词
/// 2.根据类型_页面_功能，
/// 例如：UI_LOADING_PROGRESS
///       UI_LOGIN_CONNECT
/// </summary>
namespace ETHotfix
{
    public class EventID
    {
        public const string UIFRAME_ON_WINDOW_CREATE = "UIFrameOnWindowCreate";
        public const string UIFRAME_ON_WINDOW_ACTIVE = "UIFrameOnWindowActive";
        public const string UIFRAME_ON_WINDOW_DEACTIVE = "UIFrameOnWindowDeactive";
        public const string UIFRAME_ON_WINDOW_DESTROY = "UIFrameOnWindowDestroy";
        public const string UIFRAME_ON_WINDOW_ENTER_ANIM = "UIFrameOnWindowEnterAnim";
        public const string UIFRAME_ON_WINDOW_EXIT_ANIM = "UIFrameOnWindowExitAnim";
        public const string UIFRAME_ON_WINDOW_BACK = "UIFrameOnWindowBack";

        //UILoading
        public const string UILOADING_PROGRESS_CHANGE = "UILoadingProgressChange";

        //-- 模块消息添加到下面
        //-- UILogin模块
        public const string UILOGIN_ON_SELECTED_SVR_CHG = "UILoginOnSelectedSvrChg";

    }
}