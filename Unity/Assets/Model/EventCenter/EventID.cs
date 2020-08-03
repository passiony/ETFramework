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
namespace ETModel
{
    public class EventID
    {
        public const string UI_LAUNCH_PROGRESS = "UILaunchProgress";//热更进度
        public const string Animation_OnEventTrigger = "Animation_OnEventTrigger";
        public const string LanguageChanged = "LanguageChanged";

    }
}