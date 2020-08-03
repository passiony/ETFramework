using System;
using ETModel;
using UnityEngine;
using UnityEngine.UI;

namespace ETHotfix
{
    public class UILoginCtrl: UIBaseCtrl
	{
        protected UILoginModel Model
        {
            get
            {
                return model as UILoginModel;
            }
        }

        public void OnLogin(string account,string password)
		{
            Model.SaveData(account, password);
            LoginHelper.OnLoginAsync(account).Coroutine();
        }
	}
}
