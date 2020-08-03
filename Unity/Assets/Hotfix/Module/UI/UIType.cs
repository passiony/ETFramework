using ETModel;

namespace ETHotfix
{
    public static class UIType
    {
        /// <summary>
        /// 登录页面
        /// </summary>
        public static UIConfig UILogin = new UIConfig("UILogin",
            "UI/UILogin/UILogin.prefab",
            ELayer.Normal,
            typeof(UILoginModel),
            typeof(UILoginView),
            typeof(UILoginCtrl));

        /// <summary>
        /// 大厅
        /// </summary>
        public static UIConfig UILobby = new UIConfig("UILobby",
            "UI/UILobby/UILobby.prefab",
            ELayer.Normal,
            typeof(UILobbyModel),
            typeof(UILobbyView),
            typeof(UILobbyCtrl));

//*AppendCode

    }
}