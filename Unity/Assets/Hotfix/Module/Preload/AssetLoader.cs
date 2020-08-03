using System;
using UnityEngine;
using ETModel;

namespace ETHotfix
{
    /// <summary>
    /// 预加载器
    /// </summary>
    public class AssetLoader
    {
        public string url;
        public Type type;

        public string cacheName;
        public bool background;
        public bool loadAll;

        public AssetLoader(string cacheName, string path, Type type, bool background=false, bool loadAll=false)
        {
            this.cacheName = cacheName;
            this.url = path;
            this.type = type;

            this.background = background;
            this.loadAll = loadAll;
        }

        public void LoadAsync(Action callback)
        {
            var cache = ResourceCacheManager.Instance.Get(cacheName);

            if (this.loadAll)
                cache.LoadAllAsync(url, type, (asset) =>
                {
                    if (!background)
                        callback?.Invoke();
                });
            else
                cache.LoadAsync(url, type, (asset) =>
                {
                    if (!background)
                        callback?.Invoke();
                    if (this.type == typeof (GameObject))
                    {
                        GameObjectPool.Instance.PreloadGameObject((asset as GameObject), 1);
                    }
                });
        }
    }
}
