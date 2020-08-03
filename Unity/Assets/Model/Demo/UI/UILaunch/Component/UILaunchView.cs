/**
* added by passion  @ 2020/1/30 23:01:44 
* UILoading视图层
* 
*/
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ETModel
{
    public class UILaunchView : UIBaseView
	{
        Text text;
        Slider slider;
        public override void Awake()
		{
            text = this.GetChild<Text>("Text");
            slider = this.GetChild<Slider>("Progress");
        }

        protected override void AddUIListener()
        {
            EventCenter.Register<string, float>(EventID.UI_LAUNCH_PROGRESS, OnLoadingProgress);
        }

        protected override void RemoveUIListener()
        {
            EventCenter.UnRegister<string, float>(EventID.UI_LAUNCH_PROGRESS, OnLoadingProgress);
        }

        private void OnLoadingProgress(string msg, float value)
        {
            slider.gameObject.SetActive(value > 0);

            slider.normalizedValue = value;
            text.text = msg;
        }
    }
}
