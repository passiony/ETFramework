using System;
using ETModel;
using UnityEngine;
using UnityEngine.UI;

namespace ETHotfix
{
    public class UILoginView: UIBaseView
	{
		private InputField account; 
		private InputField password; 
        private GameObject loginBtn;

        protected UILoginCtrl Ctrl
        {
            get
            {
                return ctrl as UILoginCtrl;
            }
        }
        protected UILoginModel Model
        {
            get
            {
                return model as UILoginModel;
            }
        }

        public override void Awake()
		{
            account = GetChild<InputField>("Account");
            password = GetChild<InputField>("Password");

            account.text = Model.username;
            password.text = Model.password;

            loginBtn = GetChild("LoginBtn");
            loginBtn.GetComponent<Button>().onClick.Add(()=> {
                Ctrl.OnLogin(account.text,password.text);
            });
		}

        public override void Enable()
        {

        }

        public override void Disable()
        {

        }
    }
}
