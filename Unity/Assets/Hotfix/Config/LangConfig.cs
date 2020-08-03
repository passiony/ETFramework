
using ETModel;

namespace ETHotfix
{
    //[Config((int)(AppType.ClientH | AppType.ClientM | AppType.Gate | AppType.Map))]
    //public partial class LangConfigCategory : DBCategory<LangConfig>
    //{

    //}

    public class LangConfig : IConfig
    {
        public long Id { get; set; }
        public string type;
        public string chinese;
        public string english;
    }
}
