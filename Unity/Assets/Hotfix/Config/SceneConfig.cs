using ETModel;

namespace ETHotfix
{
    //[Config((int)(AppType.ClientH | AppType.ClientM | AppType.Gate | AppType.Map))]
    //public partial class SceneConfigCategory : DBCategory<SceneConfig>
    //{

    //}

    public class SceneConfig : IConfig
    {
        public long Id { get; set; }
        public string name;
    }
}
