using UnityEngine;

namespace ETModel
{
    [Event(EventIdType.LoadingBegin)]
    public class LaunchBeginEvent : AEvent
    {
        public override void Run()
        {
            Game.Scene.GetComponent<UIComponent>().Open(UIType.UILaunch);
        }
    }
}
