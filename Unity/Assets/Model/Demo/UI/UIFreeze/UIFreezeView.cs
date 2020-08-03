/**
* added by passion  @ 2020/2/16 11:21:43 
* UIFreeze视图层
* 冻结窗口，顶层遮罩
*/
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace ETModel
{
    public class UIFreezeView : UIBaseView
    {
        Image beijing;
        Image loadingImg;
        Text infoTxt;

        public List<object> freezeList = new List<object>();

        public override void Awake()
        {
            beijing = GetChild<Image>("Beijing");
            loadingImg = GetChild<Image>("Img");
            infoTxt = GetChild<Text>("Text");

            Deactive();
        }

        void Deactive()
        {
            beijing.gameObject.SetActive(false);
            loadingImg.gameObject.SetActive(false);
            infoTxt.gameObject.SetActive(false);
        }

        public void FreezeUI(object key)
        {
            ClearNullRef();
            if (freezeList.Contains(key))
                return;

            freezeList.Add(key);
            beijing.gameObject.SetActive(true);
            beijing.color = Color.clear;
            LogFreeze();
        }

        public void FreezeUIWithColor(object key, Color color)
        {
            ClearNullRef();
            if (freezeList.Contains(key))
                return;

            freezeList.Add(key);
            beijing.gameObject.SetActive(true);
            beijing.color = color;
            LogFreeze();
        }

        public void FreezeUIWithText(object key, string text)
        {
            ClearNullRef();
            if (freezeList.Contains(key))
                return;

            freezeList.Add(key);
            beijing.gameObject.SetActive(true);
            beijing.color = new Color(0, 0, 0, 0.3f);
            infoTxt.text = text;
            LogFreeze();
        }

        public void FreezeUIWithLoading(object key)
        {
            ClearNullRef();
            if (freezeList.Contains(key))
                return;

            freezeList.Add(key);
            beijing.gameObject.SetActive(true);
            beijing.color = new Color(0, 0, 0, 0.3f);
            loadingImg.gameObject.SetActive(true);
            LogFreeze();
        }

        public void FreezeUIWithTime(object key, float time)
        {
            ClearNullRef();
            if (freezeList.Contains(key))
                return;

            freezeList.Add(key);
            beijing.gameObject.SetActive(true);
            beijing.color = Color.clear;
            LogFreeze();

            GameUtility.DelayAction((int)(1000 * time), () =>
            {
                UnFreezeUI(key);
            }).Coroutine();
        }

        public void UnFreezeUI(object key)
        {
            if (!freezeList.Contains(key))
                return;

            freezeList.Remove(key);
            if (freezeList.Count == 0)
            {
                Deactive();
            }
            LogFreeze();
        }

        public void UnFreezeAll()
        {
            freezeList.Clear();
            Deactive();
            LogFreeze();
        }

        void ClearNullRef()
        {
            for (int i = 0; i < freezeList.Count; i++)
            {
                if (freezeList[i] == null)
                {
                    freezeList.RemoveAt(i);
                }
            }
        }

        [Conditional("LOGGER_ON")]
        public void LogFreeze()
        {
            Log.Debug("[UIFreeze] = " + freezeList.Count);
        }
    }
}
