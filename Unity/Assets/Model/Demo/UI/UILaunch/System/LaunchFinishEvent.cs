namespace ETModel
{
    [Event(EventIdType.LoadingFinish)]
    public class LaunchFinishEvent : AEvent
    {
        public override void Run()
        {
			//Game.Scene.GetComponent<UIComponent>().Remove(UIType.UILoading);

            Game.Scene.GetComponent<UIComponent>().Close(UIType.UILaunch);
        }
    }
}
