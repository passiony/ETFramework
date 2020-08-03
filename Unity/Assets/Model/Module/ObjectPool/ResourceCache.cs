using System;
using System.Collections.Generic;
using UnityEngine;
using UObject=UnityEngine.Object;
using XAsset;


/// <summary>
/// 1.只提供异步加载，逻辑层不需要关心是否加载完成
/// 2.dict做资源缓存,引用计数为1，每次获取不再增加
/// 3.图片名称带后缀
/// </summary>
namespace ETModel
{
    public class ResourceCacheManager : EntitySingleton<ResourceCacheManager>
    {
        private Dictionary<string, ResourceCache> pools = new Dictionary<string, ResourceCache>();

        public ResourceCache Get(UIConfig config)
        {
            return Get(config.Name);
        }

        public ResourceCache Get(string key)
        {
            if (pools.TryGetValue(key, out ResourceCache pool))
            {
                return pool;
            }

            pool = new ResourceCache();
            pools.Add(key, pool);
            return pool;
        }

        public bool Remove(string key,bool includeResident)
        {
            if (pools.TryGetValue(key, out ResourceCache pool))
            {
                if(pool.UnloadAll(includeResident))
                {
                    pools.Remove(key);
                    return true;
                }
            }
            return false;
        }
    }

    public class ResourceCache
    {
        private Dictionary<string, AssetRequest> poolDic = new Dictionary<string, AssetRequest>();

        private HashSet<string> residents = new HashSet<string>();

        #region Load

        public T Load<T>(string image_path, bool resident) where T : UObject
        {
            if (resident)
            {
                residents.Add(image_path);
            }
            return Load<T>(image_path);
        }

        public T Load<T>(string image_path) where T : UObject
        {
            return Load(image_path,typeof(T)) as T;
        }

        public UObject Load(string image_path,Type type)
        {
            AssetRequest atlas;
            if (poolDic.TryGetValue(image_path, out atlas))
            {
                return atlas.asset;
            }
            else
            {
                atlas = ResourcesComponent.Load(image_path, type);
                poolDic.Add(image_path, atlas);
                return atlas.asset;
            }
        }

        public UObject[] LoadAll(string image_path,Type type)
        {
            AssetRequest atlas;
            if (poolDic.TryGetValue(image_path, out atlas))
            {
                return atlas.allAssets;
            }
            else
            {
                atlas = ResourcesComponent.LoadAll(image_path, type);
                poolDic.Add(image_path, atlas);
                return atlas.allAssets;
            }
        }
        
        #endregion

        #region LoadAsync
        public void LoadAsync<T>(string image_path, bool resident, Action<UObject> callback) where T : UObject
        {
            if (resident)
            {
                residents.Add(image_path);
            }
            LoadAsync<T>(image_path, callback);
        }

        public void LoadAsync<T>(string image_path, Action<UObject> callback) where T : UObject
        {
            LoadAsync(image_path,typeof(T),(asset)=> {
                callback?.Invoke(asset);
            });
        }

        public void LoadAllAsync(string image_path, Type type,Action<UObject[]> callback)
        {
            AssetRequest atlas;
            if (poolDic.TryGetValue(image_path, out atlas))
            {
                if (atlas.asset != null)
                {
                    callback?.Invoke(atlas.allAssets);
                }
                else
                {
                    atlas.completed += delegate
                    {
                        callback?.Invoke(atlas.allAssets);
                    };
                }
            }
            else
            {
                atlas = ResourcesComponent.LoadAllAsync(image_path,type, (request) =>
                {
                    callback?.Invoke(request.allAssets);
                });
                
                if (!poolDic.ContainsKey(image_path))
                {
                    poolDic.Add(image_path, atlas);
                }
            }
        }
        
        public void LoadAsync(string image_path, Type type,Action<UObject> callback)
        {
            AssetRequest atlas;
            if (poolDic.TryGetValue(image_path, out atlas))
            {
                if (atlas.asset != null)
                {
                    callback?.Invoke(atlas.asset);
                }
                else
                {
                    atlas.completed += delegate
                    {
                        callback?.Invoke(atlas.asset);
                    };
                }
            }
            else
            {
                atlas = ResourcesComponent.LoadAsync(image_path,type, (request) =>
                {
                    callback?.Invoke(request.asset);
                });
                
                if (!poolDic.ContainsKey(image_path))
                {
                    poolDic.Add(image_path, atlas);
                }
            }
        }

        #endregion
        
        public bool AddResident(string image_path)
        {
            return residents.Add(image_path);
        }
        public bool RemoveResident(string image_path)
        {
            return residents.Remove(image_path);
        }
        public bool ChangeResident(string old_path, string new_path)
        {
            return RemoveResident(old_path) && AddResident(new_path);
        }

        /// <summary>
        /// 强制卸载单个资源
        /// </summary>
        /// <param name="path">资源路径</param>
        public void Unload(string path)
        {
            if (poolDic.TryGetValue(path, out AssetRequest atlas))
            {
                atlas.Release();
            }
            poolDic.Remove(path);
            residents.Remove(path);
        }

        List<string> unloadList=new List<string>();
        /// <summary>
        /// 卸载全部
        /// </summary>
        /// <param name="includeResident">是否卸载持久化列表</param>
        public bool UnloadAll(bool includeResident = false)
        {
            unloadList.Clear();
            foreach (var item in poolDic)
            {
                if (!includeResident && residents.Contains(item.Key))
                    continue;

                item.Value.Release();
                unloadList.Add(item.Key);
            }

            if (includeResident)
            {
                poolDic.Clear();
                residents.Clear();
            }
            else
            {
                foreach (var item in unloadList)
                {
                    poolDic.Remove(item);
                }
                unloadList.Clear();
            }
            
            Resources.UnloadUnusedAssets();
            return poolDic.Count > 0;
        }
        
        /// <summary>
        /// 卸载全部
        /// </summary>
        /// <param name="ignoreList">卸载忽略列表</param>
        /// <param name="includeResident">是否卸载持久化列表</param>
        public void UnloadAll(List<string> ignoreList, bool includeResident = false)
        {
            unloadList.Clear();
            foreach (var item in poolDic)
            {
                if (!includeResident && residents.Contains(item.Key))
                    continue;

                if (ignoreList.Contains(item.Key))
                    continue;
                
                item.Value.Release();
                unloadList.Add(item.Key);
            }

            if (includeResident)
            {
                poolDic.Clear();
                residents.Clear();
            }
            else
            {
                foreach (var item in unloadList)
                {
                    poolDic.Remove(item);
                }
                unloadList.Clear();
            }

            Resources.UnloadUnusedAssets();
        }
    }
}