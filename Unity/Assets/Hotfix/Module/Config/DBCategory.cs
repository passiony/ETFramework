using ETModel;
using System;
using System.Collections.Generic;

namespace ETHotfix
{
    public abstract class DBCategory<T> : ACategory<T> where T : IConfig,new()
    {
        public override void BeginInit()
        {
            this.dict = new Dictionary<long, IConfig>();

            var list = Game.Scene.GetComponent<SqliteComponent>().SelectTable<T>();

            foreach (var item in list)
            {
                dict.Add(item.Id,item);
            }
        }

        public override void EndInit()
        {

        }
    }
}
