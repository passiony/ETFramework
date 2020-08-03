/**
* added by passion  @ 2020/1/31 17:39:15 
* UINoticeTip视图层
* 
*/
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ETModel
{
    public class UINoticeTipView: UIBaseView
	{
        Text titleText;
        Text noticeText;

        Text buttonOneText;
        Text buttonTwoText;
        Text buttonThreeText;
        Button buttonOne;
        Button buttonTwo;
        Button buttonThree;
        Button closeBtn;

        ETTaskCompletionSource tcs;

        static public int LastClickIndex
        {
            get;
            protected set;
        }

        public bool IsShowing
        {
            get;
            protected set;
        }

        public override void Awake()
		{
            titleText = transform.Find("BgRoot/BgParent/WinTitle").GetComponent<Text>();
            closeBtn = transform.Find("BgRoot/BgParent/closeBtn").GetComponent<Button>();

            var root = transform.Find("BgRoot/BgParent/ContentRoot");
            noticeText = root.Find("NoticeInfo").GetComponent<Text>();
            buttonOneText = root.Find("ButtonOne/Button/text").GetComponent<Text>();
            buttonTwoText = root.Find("ButtonTwo/Button/text").GetComponent<Text>();
            buttonThreeText = root.Find("ButtonThree/Button/text").GetComponent<Text>();

            buttonOne = root.Find("ButtonOne/Button").GetComponent<Button>();
            buttonTwo = root.Find("ButtonTwo/Button").GetComponent<Button>();
            buttonThree = root.Find("ButtonThree/Button").GetComponent<Button>();

            ResetView(IsShowing);
        }

        public override void Enable()
        {
            base.Enable();
        }

        private void ResetView(bool isShow)
        {
            IsShowing = isShow;
            tcs = null;
            if (isShow)
            {
                LastClickIndex = -1;
            }

            if (gameObject != null)
            {
                gameObject.SetActive(isShow);
                buttonOne.gameObject.SetActive(false);
                buttonTwo.gameObject.SetActive(false);
                buttonThree.gameObject.SetActive(false);
                closeBtn.gameObject.SetActive(false);

                buttonOne.onClick.RemoveAllListeners();
                buttonTwo.onClick.RemoveAllListeners();
                buttonThree.onClick.RemoveAllListeners();
                closeBtn.onClick.RemoveAllListeners();
            }
        }

        void BindCallback(int index, Button button, Action callback)
        {
            button.onClick.AddListener(() =>
            {
                callback?.Invoke();
                tcs?.SetResult();

                LastClickIndex = index;
                ResetView(false);
            });
        }

        public void ShowZeroButtonTip(string title, string content)
        {
            if (gameObject == null)
            {
                Log.Error("You should set UIGameObject first!");
                return;
            }

            ResetView(true);
            closeBtn.gameObject.SetActive(true);

            titleText.text = title;
            // noticeText.SetLeftText(content);
            noticeText.text = content;
            BindCallback(3, closeBtn, null);
        }
        
        public void ShowOneButtonTip(string title, string content, string btnText, Action callback,Action closeback)
        {
            if (gameObject == null)
            {
                Log.Error("You should set UIGameObject first!");
                return;
            }

            ResetView(true);
            buttonTwo.gameObject.SetActive(true);
            closeBtn.gameObject.SetActive(true);

            titleText.text = title;
            // noticeText.SetLeftText(content);
            noticeText.text = content;
            buttonTwoText.text = btnText;
            BindCallback(0, buttonTwo, callback);
            BindCallback(3, closeBtn, closeback);
        }

        public void ShowOneButtonTip(string title, string content, string btnText, Action callback)
        {
            if (gameObject == null)
            {
                Log.Error("You should set UIGameObject first!");
                return;
            }

            ResetView(true);
            buttonTwo.gameObject.SetActive(true);

            titleText.text = title;
            // noticeText.SetLeftText(content);
            noticeText.text = content;
            buttonTwoText.text = btnText;
            BindCallback(0, buttonTwo, callback);
        }

        public void ShowTwoButtonTip(string title, string content, string btnText1, string btnText2, Action callback1, Action callback2)
        {
            if (gameObject == null)
            {
                Log.Error("You should set UIGameObject first!");
                return;
            }

            ResetView(true);
            buttonOne.gameObject.SetActive(true);
            buttonThree.gameObject.SetActive(true);
            titleText.text = title;
            noticeText.text = content;
            // noticeText.SetLeftText(content);
            //Cancel按钮居左，按钮颜色更改
            string blue = "Atlas/Comm/btn_blue.png";
            string green = "Atlas/Comm/btn_green.png";
            if (btnText1 == Language.Cancel_BtnText || btnText2 == Language.Cancel_BtnText)
            {
                string otherTxt = btnText1 == Language.Cancel_BtnText? btnText2 : btnText1;
                buttonOne.GetComponent<Image>().sprite = ResourcesComponent.Load<Sprite>(blue);
                buttonOneText.text = Language.Cancel_BtnText;
                buttonThreeText.text = otherTxt;
                BindCallback(0,buttonOne,callback2);
                BindCallback(1,buttonThree,callback1);
            }
            else
            {
                buttonOne.GetComponent<Image>().sprite = ResourcesComponent.Load<Sprite>(green);
                buttonOneText.text = btnText1;
                buttonThreeText.text = btnText2;
                BindCallback(0, buttonOne, callback1);
                BindCallback(1, buttonThree, callback2);
            }
        }

        public void ShowThreeButtonTip(string title, string content, string btnText1, string btnText2, string btnText3, Action callback1, Action callback2, Action callback3)
        {
            if (gameObject == null)
            {
                Log.Error("You should set UIGameObject first!");
                return;
            }

            ResetView(true);
            buttonOne.gameObject.SetActive(true);
            buttonTwo.gameObject.SetActive(true);
            buttonThree.gameObject.SetActive(true);

            titleText.text = title;
             // noticeText.SetLeftText(content);
            noticeText.text = content;
            buttonOneText.text = btnText1;
            buttonTwoText.text = btnText2;
            buttonThreeText.text = btnText3;

            BindCallback(0, buttonOne, callback1);
            BindCallback(1, buttonTwo, callback2);
            BindCallback(2, buttonThree, callback3);
        }

        public void HideTip()
        {
            if (gameObject != null)
            {
                gameObject.SetActive(false);
            }
        }

        public ETTask WaitForResponse()
        {
            tcs = new ETTaskCompletionSource();

            return tcs.Task;
        }
    }
}
