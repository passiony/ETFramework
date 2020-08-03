using ETModel;
using System;

namespace ETHotfix
{
    //[Config((int)(AppType.ClientH | AppType.ClientM | AppType.Gate | AppType.Map))]
    //public partial class DBConfigCategory : DBCategory<DBConfig>
    //{
        
    //}

    public class DBConfig : IConfig
    {
        public long Id { get; set; }
        public string Name;
        public string Desc;
        public int Position;
        public int Height;
        public int Weight;
    }
}
