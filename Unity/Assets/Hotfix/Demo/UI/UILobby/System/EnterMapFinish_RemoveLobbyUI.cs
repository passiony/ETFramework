using ETModel;
using UnityEngine;

namespace ETHotfix
{
	[Event(EventIdType.EnterMapFinish)]
	public class EnterMapFinish_RemoveLobbyUI: AEvent
	{
		public override void Run()
		{
			//Game.Scene.GetComponent<UIComponent>().Remove(UIType.UILobby);
			//ETModel.Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle(UIType.UILobby.StringToAB());

            Game.Scene.GetComponent<UIComponent>().Close(UIType.UILobby);

            //创建unit
            UnitComponent unitComponent = ETModel.Game.Scene.GetComponent<UnitComponent>();
            Unit unit = UnitFactory.Create(ETModel.Game.Scene, 1);
            unit.Position = new Vector3(0, 0, 0);

        }
	}
}
