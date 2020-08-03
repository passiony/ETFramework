//
// Assets.cs
//
// Author:
//       fjy <jiyuan.feng@live.com>
//
// Copyright (c) 2019 fjy
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;
using UObject = UnityEngine.Object;
using XAsset;

namespace ETModel
{
    [ObjectSystem]
    public class ResourcesComponentUpdateSystem : UpdateSystem<ResourcesComponent>
    {
        public override void Update(ResourcesComponent self)
        {
            self.Update();
        }
    }

    public class ResourcesComponent : Entity
    {
        private static string[] _bundles = new string[0];
        private static Dictionary<string, int> _bundleAssets = new Dictionary<string, int>();

        // ReSharper disable once InconsistentNaming
        private static readonly List<AssetRequest> _loadingAssets = new List<AssetRequest>();
        private static readonly Dictionary<string, AssetRequest> _assets = new Dictionary<string, AssetRequest>();
        private static readonly List<AssetRequest> _unusedAssets = new List<AssetRequest>();

        static ETTaskCompletionSource<bool> tcsb;

        public static ETTask<bool> InitAsync()
        {
            tcsb = new ETTaskCompletionSource<bool>();

            Initialize(delegate
            {
                tcsb.SetResult(true);
            }, delegate (string error)
            {
                tcsb.SetResult(false);
            });
            return tcsb.Task;
        }

        public static void Initialize(Action onSuccess, Action<string> onError)
        {
            ETModel.Log.Warning(string.Format("Init->assetBundleMode {0} | dataPath {1}", Utility.assetBundleMode, Utility.dataPath));

            if (Utility.assetBundleMode)
            {
                var platform = Utility.GetPlatform();
                var path = Utility.dataPath;

                Bundles.OverrideBaseDownloadingUrl += Bundles_overrideBaseDownloadingURL;
                Bundles.Initialize(path, platform, () =>
                {
                    LoadAsync(Utility.AssetsManifestAsset, typeof(AssetsManifest), (asset) =>
                    {
                        var manifest = asset.asset as AssetsManifest;
                        if (manifest == null)
                        {
                            if (onError != null) onError("manifest == null");
                            return;
                        }

                        if (manifest.serverMode == ServerMode.Local)
                        {
                            URLSetting.RES_DOWNLOAD_URL = manifest.downloadURL;
                        }

                        Utility.runMode = manifest.serverMode;
                        Utility.channelName = manifest.channelType.ToString();
                        LoggerHelper.channelName = Utility.channelName;

                        Bundles.activeVariants = manifest.activeVariants;
                        _bundles = manifest.bundles;
                        var dirs = manifest.dirs;
                        _bundleAssets = new Dictionary<string, int>(manifest.assets.Length);
                        for (int i = 0, max = manifest.assets.Length; i < max; i++)
                        {
                            var item = manifest.assets[i];
                            string key = string.Format("{0}/{1}", dirs[item.dir], item.name);
                            key = FileUtility.FormatToUnityPath(key);
                            key = key.Replace(Utility.AssetRootPath, "");

                            _bundleAssets[key] = item.bundle;
                        }

                        if (onSuccess != null)
                            onSuccess();
                        
                        asset.Release();
                    });
                }, onError);
            }
            else
            {
                if (onSuccess != null)
                    onSuccess();
            }
        }

        public static string[] GetAllDependencies(string path)
        {
            string assetBundleName;
            return GetAssetBundleName(path, out assetBundleName) ? Bundles.GetAllDependencies(assetBundleName) : null;
        }

        public static SceneAssetRequest LoadScene(string path, bool async, bool addictive)
        {
            var asset = async ? new SceneAssetAsyncRequest(path, addictive) : new SceneAssetRequest(path, addictive);
            GetAssetBundleName(path, out asset.assetBundleName);
            asset.key = path;
            asset.path = path;
            asset.Load();
            asset.Retain();
            _loadingAssets.Add(asset);
            _assets.Add(path, asset);
            return asset;
        }

        public static void UnloadScene(string path)
        {
            for (int i = 0, max = _loadingAssets.Count; i < max; i++)
            {
                var item = _loadingAssets[i];
                if (!item.path.Equals(path))
                    continue;
                Unload(item);
                break;
            }
        }

        public static void CleanUp()
        {
            Log("ResourceComponents.CleanUp");
            for (int i = 0, max = _loadingAssets.Count; i < max; i++)
            {
                var item = _loadingAssets[i];
                item.Unload();
            }

            _loadingAssets.Clear();
            _assets.Clear();
            _unusedAssets.Clear();
            Array.Clear(_bundles, 0, _bundles.Length);

            Bundles.CleanUp();
        }

        #region Load
        
        public static T Load<T>(string path) where T : class
        {
            var type = typeof(T);
            var asset = Load(path, type).asset;
            if (asset != null)
                return asset as T;

            Debug.LogError("load file is null :" + path);
            return null;
        }
        
        public static AssetRequest Load(string path, Type type = null)
        {
            type = type == null ? typeof(UObject) : type;
            return Load(path, type, false, false,null);
        }

        public static T LoadOnce<T>(string path) where T : class
        {
            var type = typeof(T);
            var asset = Load(path, type);
            var obj = asset.asset;
            asset.Release();

            if (obj != null)
                return obj as T;

            Debug.LogError("load file is null :" + path);
            return null;
        }
        
        public static AssetRequest LoadAll(string path, Type type = null)
        {
            type = type == null ? typeof(UnityEngine.Object) : type;
            return Load(path, type, false,true,null);
        }
        
        #endregion

        #region LoadAsync

        public static void LoadOnceAsync<T>(string path, Action<object> callback)
        {
            var type = typeof(T);
            LoadAsync(path, type, (asset) =>
            {
                var obj = asset.asset;
                if (obj != null)
                {
                    callback?.Invoke(obj);
                }
                else
                {
                    callback?.Invoke(null);
                    Debug.LogError("load file is null :" + path);
                }
                asset.Release();
                
            });
        }

        public static AssetRequest LoadAsync<T>(string path, Action<AssetRequest> callback)
        {
            return Load(path, typeof(T), true,false, callback);
        }

        public static AssetRequest LoadAsync(string path, Type type, Action<AssetRequest> callback)
        {
            return Load(path, type, true,false,callback);
        }

        public static AssetRequest LoadAllAsync(string path, Type type, Action<AssetRequest> callback)
        {
            return Load(path, type, true,true,callback);
        }

        #endregion
       
        
        public static void UnUse(AssetRequest asset)
        {
            _unusedAssets.Add(asset);
        }
        
        public static void Unload(AssetRequest asset)
        {
            asset.Unload();
            _assets.Remove(asset.key);
            
            Log("unload Asset :" + asset.path);
        }
        
        public static void Unload(string key)
        {
            if (_assets.TryGetValue(key, out AssetRequest asset))
            {
                Unload(asset);
            }
            else
            {
                Debug.LogError("unload file is null :" + key);
            }
        }

        public void Update()
        {
            for (var i = 0; i < _loadingAssets.Count; i++)
            {
                var item = _loadingAssets[i];
                if (item.loadState == LoadState.Loaded || item.loadState == LoadState.Unload || item.error != null)
                {
                    _loadingAssets.RemoveAt(i);
                    i--;
                    continue;
                }

                item.Update();
            }

            for (var i = 0; i < _unusedAssets.Count; i++)
            {
                var item = _unusedAssets[i];
                item.Unload();
                _assets.Remove(item.key);
                Log("Unload->" + item.key);
            }
            _unusedAssets.Clear();

            Bundles.Update();
        }

        [Conditional("LOG_DEBUG")]
        private static void Log(string s)
        {
            ETModel.Log.Debug(string.Format("[Assets]{0}", s));
        }

        private static AssetRequest Load(string path, Type type, bool async, bool loadAll, Action<AssetRequest> callback)
        {
            var start = DateTime.Now; 
            string key = path;
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("invalid path");
                return null;
            }

            if ((type.Name == "Object" || type.Name == "GameObject") && !path.EndsWith(".prefab"))
            {
                path = path + ".prefab";
            }
            
            if (_assets.TryGetValue(key, out AssetRequest item))
            {
                item.Retain();
                if (item.isDone)
                {
                    callback?.Invoke(item);
                }
                else
                {
                    item.completed += callback;
                }
                
                Log($"<color=green>Hit On Asset :{key}</color>");
                return item;
            }

            string assetBundleName = "";
            AssetRequest asset;
            if (GetAssetBundleName(path, out assetBundleName))
            {
                asset = async ? new BundleAssetAsyncRequest(assetBundleName) : new BundleAssetRequest(assetBundleName);
            }
            else
            {
                if (path.StartsWith("http://", StringComparison.Ordinal) ||
                    path.StartsWith("https://", StringComparison.Ordinal) ||
                    path.StartsWith("file://", StringComparison.Ordinal) ||
                    path.StartsWith("ftp://", StringComparison.Ordinal) ||
                    path.StartsWith("jar:file://", StringComparison.Ordinal))
                {
                    asset = new WebAssetRequest();
                }
                else
                {
                    asset = new AssetRequest();
                    if (!path.StartsWith(Utility.AssetRootPath))
                        path = Utility.AssetRootPath + path;
                }
            }

            asset.key = key;
            asset.path = path;
            asset.loadAll = loadAll;
            asset.assetType = type;
            _loadingAssets.Add(asset);
            _assets.Add(key, asset);
            asset.completed += callback;
            asset.Load();
            asset.Retain();

            Log(string.Format("Load-> path:{0}|assetBundleName:{1} ->use {2}ms", path, assetBundleName,(DateTime.Now - start).TotalMilliseconds));
            return asset;
        }

        private static bool GetAssetBundleName(string path, out string assetBundleName)
        {
            if (path.Equals(Utility.AssetsManifestAsset))
            {
                assetBundleName = Path.GetFileNameWithoutExtension(path).ToLower();
                return true;
            }

            assetBundleName = null;
            int bundle;
            if (!_bundleAssets.TryGetValue(path, out bundle))
                return false;
            assetBundleName = _bundles[bundle];
            return true;
        }

        private static string Bundles_overrideBaseDownloadingURL(string bundleName)
        {
            string path = Path.Combine(Utility.updatePath, bundleName);
            if (File.Exists(path))
            {
                return path;
            }
            return Path.Combine(Utility.dataPath, bundleName);
        }
    }
}