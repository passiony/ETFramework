using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ETModel
{
    public static class NoticeTipHelper
    {
        public static void ShowZeroButtonTip(string title, string content)
        {
            UIWindow  ui = UIComponent.Instance.Open(UIType.UINoticeTip);
            var noticeTip = ui.View as UINoticeTipView;
            noticeTip.ShowZeroButtonTip(title, content);
        }
        
        public static void ShowOneButtonTip(string title, string content, string btnText, Action callback)
        {
            UIWindow  ui = UIComponent.Instance.Open(UIType.UINoticeTip);
            var noticeTip = ui.View as UINoticeTipView;
            noticeTip.ShowOneButtonTip(title, content, btnText, callback);
        }

        public static void ShowOneButtonTip(string title, string content, string btnText, Action callback,Action closeback)
        {
            UIWindow  ui = UIComponent.Instance.Open(UIType.UINoticeTip);
            var noticeTip = ui.View as UINoticeTipView;
            noticeTip.ShowOneButtonTip(title, content, btnText, callback, closeback);
        }

        public static void ShowTwoButtonTip(string title, string content, string btnText1, string btnText2, Action callback1, Action callback2)
        {
            UIWindow  ui = Game.Scene.GetComponent<UIComponent>().Open(UIType.UINoticeTip);
            var noticeTip = ui.View as UINoticeTipView;
            noticeTip.ShowTwoButtonTip(title, content, btnText1, btnText2, callback1, callback2);
        }

        public static void ShowThreeButtonTip(string title, string content, string btnText1, string btnText2, string btnText3, Action callback1, Action callback2, Action callback3)
        {
            UIWindow  ui = Game.Scene.GetComponent<UIComponent>().Open(UIType.UINoticeTip);
            var noticeTip = ui.View as UINoticeTipView;
            noticeTip.ShowThreeButtonTip(title, content, btnText1, btnText2, btnText3, callback1, callback2, callback3);
        }

        public static async ETTask WaitForNoticeTip(string title, string content, string btnText, Action callback)
        {
            UIWindow  ui = Game.Scene.GetComponent<UIComponent>().Open(UIType.UINoticeTip);
            var noticeTip = ui.View as UINoticeTipView;
            noticeTip.ShowOneButtonTip(title, content, btnText, callback);

            await noticeTip.WaitForResponse();
        }
        public static async ETTask WaitForNoticeTip(string title, string content, string btnText, Action callback, Action closeback)
        {
            UIWindow  ui = Game.Scene.GetComponent<UIComponent>().Open(UIType.UINoticeTip);
            var noticeTip = ui.View as UINoticeTipView;
            noticeTip.ShowOneButtonTip(title, content, btnText, callback, closeback);

            await noticeTip.WaitForResponse();
        }

        public static async ETTask WaitForNoticeTip(string title, string content, string btnText1, string btnText2, Action callback1, Action callback2)
        {
            UIWindow  ui = Game.Scene.GetComponent<UIComponent>().Open(UIType.UINoticeTip);
            var noticeTip = ui.View as UINoticeTipView;
            noticeTip.ShowTwoButtonTip(title, content, btnText1, btnText2, callback1, callback2);

            await noticeTip.WaitForResponse();
        }
        public static async ETTask WaitForNoticeTip(string title, string content, string btnText1, string btnText2, string btnText3, Action callback1, Action callback2, Action callback3)
        {
            UIWindow  ui = Game.Scene.GetComponent<UIComponent>().Open(UIType.UINoticeTip);
            var noticeTip = ui.View as UINoticeTipView;
            noticeTip.ShowThreeButtonTip(title, content, btnText1, btnText2, btnText3, callback1, callback2, callback3);

            await noticeTip.WaitForResponse();
        }

    }
}