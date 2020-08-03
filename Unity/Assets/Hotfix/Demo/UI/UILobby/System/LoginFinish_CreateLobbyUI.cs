using ETModel;

namespace ETHotfix
{
	[Event(EventIdType.LoginFinish)]
	public class LoginFinish_CreateLobbyUI: AEvent
	{
		public override void Run()
		{
            //UI ui = UILobbyFactory.Create();
            //Game.Scene.GetComponent<UIComponent>().Add(ui);

            Game.Scene.GetComponent<UIComponent>().Open(UIType.UILobby);
		}
	}
}
