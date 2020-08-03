using ETModel;
using UnityEngine;
using UnityEngine.UI;

namespace ETHotfix
{
    public class UILobbyView : UIBaseView
	{
		private Button enterMap;

		public override void Awake()
		{
			enterMap =GetChild<Button>("EnterMap");
			enterMap.onClick.Add(this.EnterMap);
		}

        public override void Enable()
        {

        }

        public override void Disable()
        {

        }

        private void EnterMap()
		{
			MapHelper.EnterMapAsync("Map").Coroutine();
		}
		


	}
}
