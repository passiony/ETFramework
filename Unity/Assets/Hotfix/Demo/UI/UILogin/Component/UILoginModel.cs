using System;
using ETModel;
using UnityEngine;

namespace ETHotfix
{
    public class UILoginModel: UIBaseModel
	{
        public string username;
        public string password;

        public override void Enable(object[] args)
		{
            this.username = PlayerPrefs.GetString("USERNAME","");
            this.password = PlayerPrefs.GetString("USERNAME","");
        }

        public void SaveData(string username, string password)
        {
            PlayerPrefs.SetString("USERNAME", username);
            PlayerPrefs.SetString("USERNAME", password);
        }
	}
}
