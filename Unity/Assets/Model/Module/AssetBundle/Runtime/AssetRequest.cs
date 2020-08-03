//
// Asset.cs
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ETModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace XAsset
{
    public enum LoadState
    {
        Init,
        LoadAssetBundle,
        LoadAsset,
        Loaded,
        Unload,
    }

    public class AssetRequest : Reference, IEnumerator
    {
        public Type assetType;
        public string path;
        public string key;
        public bool loadAll;
        
        public LoadState loadState { get; protected set; }

        public AssetRequest()
        {
            asset = null;
            loadState = LoadState.Init;
        }

        public virtual bool isDone
        {
            get { return true; }
        }

        public virtual float progress
        {
            get { return 1; }
        }

        public virtual string error { get; protected set; }

        // ReSharper disable once MemberCanBeProtected.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string text { get; protected set; }

        // ReSharper disable once MemberCanBeProtected.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public byte[] bytes { get; protected set; }

        public Object asset { get; internal set; }
        
        public Object[] allAssets { get; internal set; }
        

        internal virtual void Load()
        {
            if (!Utility.assetBundleMode && Utility.loadDelegate != null)
            {
                if (loadAll)
                {
                    allAssets = Utility.loadAllDelegate(path);
                    
                }
                else
                {
                    asset = Utility.loadDelegate(path, assetType);
                }
            }
            
            if (asset == null)
            {
                error = string.Format("Asset {0} not found", path);
            }
        }

        internal virtual void Unload()
        {
            if (asset == null)
                return;

            if (!Utility.assetBundleMode && !(asset is GameObject))
            {
                if (loadAll)
                {
                    foreach (var ast in this.allAssets)
                    {
                        Resources.UnloadAsset(ast);
                    }
                }
                else
                {
                    Resources.UnloadAsset(asset);
                }
            }

            asset = null;
        }

        internal bool Update()
        {
            if (!isDone)
                return true;
            if (completed == null)
                return false;
            try
            {
                completed.Invoke(this);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            
            completed = null;
            return false;
        }

        public override void Release()
        {
            this.refCount--;
            if (this.IsUnused())
            {
                ResourcesComponent.UnUse(this);
            }
        }

        public event Action<AssetRequest> completed;

        #region IEnumerator implementation

        public bool MoveNext()
        {
            return !isDone;
        }

        public void Reset()
        {
        }

        public object Current
        {
            get { return null; }
        }

        #endregion
    }

    public class BundleAssetRequest : AssetRequest
    {
        protected readonly string assetBundleName;
        protected BundleRequest bundle;

        public BundleAssetRequest(string bundle)
        {
            assetBundleName = bundle;
        }

        internal override void Load()
        {
            bundle = Bundles.Load(assetBundleName);
            var assetName = Path.GetFileName(path);
            
            if (loadAll)
            {
                allAssets = bundle.assetBundle.LoadAssetWithSubAssets(assetName);
            }
            else
            {
                asset = bundle.assetBundle.LoadAsset(assetName, assetType);
            }
        }

        internal override void Unload()
        {
            base.Unload();

            if (bundle != null)
            {
                bundle.Release();
            }
        }
    }

    public class BundleAssetAsyncRequest : BundleAssetRequest
    {
        private AssetBundleRequest _request;

        public BundleAssetAsyncRequest(string bundle)
            : base(bundle)
        {
        }

        public override bool isDone
        {
            get
            {
                if (error != null || bundle.error != null)
                    return true;

                for (int i = 0, max = bundle.dependencies.Count; i < max; i++)
                {
                    var item = bundle.dependencies[i];
                    if (item.error != null)
                        return true;
                }

                switch (loadState)
                {
                    case LoadState.Init:
                        return false;
                    case LoadState.Loaded:
                        return true;
                    case LoadState.LoadAssetBundle:
                        {
                            if (!bundle.isDone)
                                return false;

                            for (int i = 0, max = bundle.dependencies.Count; i < max; i++)
                            {
                                var item = bundle.dependencies[i];
                                if (!item.isDone)
                                    return false;
                            }

                            if (bundle.assetBundle == null)
                            {
                                error = "assetBundle == null";
                                return true;
                            }

                            var assetName = Path.GetFileName(path);
                            if (this.loadAll)
                            {
                                _request = bundle.assetBundle.LoadAssetWithSubAssetsAsync(assetName);
                            }
                            else
                            {
                                _request = bundle.assetBundle.LoadAssetAsync(assetName, assetType);
                            }
                            
                            loadState = LoadState.LoadAsset;
                            break;
                        }
                    case LoadState.Unload:
                        break;
                    case LoadState.LoadAsset:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (loadState != LoadState.LoadAsset)
                    return false;
                if (!_request.isDone)
                    return false;
                
                if (this.loadAll)
                {
                    allAssets = _request.allAssets;
                }
                else
                {
                    asset = _request.asset;
                }
                loadState = LoadState.Loaded;
                return true;
            }
        }

        public override float progress
        {
            get
            {
                var bundleProgress = bundle.progress;
                if (bundle.dependencies.Count <= 0)
                    return bundleProgress * 0.3f + (_request != null ? _request.progress * 0.7f : 0);
                for (int i = 0, max = bundle.dependencies.Count; i < max; i++)
                {
                    var item = bundle.dependencies[i];
                    bundleProgress += item.progress;
                }

                return bundleProgress / (bundle.dependencies.Count + 1) * 0.3f +
                       (_request != null ? _request.progress * 0.7f : 0);
            }
        }

        internal override void Load()
        {
            bundle = Bundles.LoadAsync(assetBundleName,null);
            loadState = LoadState.LoadAssetBundle;
        }

        internal override void Unload()
        {
            _request = null;
            loadState = LoadState.Unload;
            base.Unload();
        }
    }

    public class SceneAssetRequest : AssetRequest
    {
        protected readonly LoadSceneMode loadSceneMode;
        protected readonly string sceneName;
        public string assetBundleName;
        protected BundleRequest bundle;

        public SceneAssetRequest(string path, bool addictive)
        {
            base.path = path;
            sceneName = Path.GetFileNameWithoutExtension(base.path);
            loadSceneMode = addictive ? LoadSceneMode.Additive : LoadSceneMode.Single;
        }

        public override float progress
        {
            get { return 1; }
        }

        internal override void Load()
        {
            if (!string.IsNullOrEmpty(assetBundleName))
            {
                bundle = Bundles.Load(assetBundleName);
                if (bundle != null)
                    SceneManager.LoadScene(sceneName, loadSceneMode);
            }
            else
            {
                SceneManager.LoadScene(sceneName, loadSceneMode);
            }
        }

        internal override void Unload()
        {
            if (bundle != null)
                bundle.Release();

            if (SceneManager.GetSceneByName(sceneName).isLoaded)
                SceneManager.UnloadSceneAsync(sceneName);

            bundle = null;
        }
    }

    public class SceneAssetAsyncRequest : SceneAssetRequest
    {
        private AsyncOperation _request;

        public SceneAssetAsyncRequest(string path, bool addictive)
            : base(path, addictive)
        {
        }

        public override float progress
        {
            get
            {
                if (bundle == null)
                    return _request == null ? 0 : _request.progress;

                var bundleProgress = bundle.progress;
                if (bundle.dependencies.Count <= 0)
                    return bundleProgress * 0.3f + (_request != null ? _request.progress * 0.7f : 0);
                for (int i = 0, max = bundle.dependencies.Count; i < max; i++)
                {
                    var item = bundle.dependencies[i];
                    bundleProgress += item.progress;
                }

                return bundleProgress / (bundle.dependencies.Count + 1) * 0.3f +
                       (_request != null ? _request.progress * 0.7f : 0);
            }
        }

        public override bool isDone
        {
            get
            {
                switch (loadState)
                {
                    case LoadState.Loaded:
                        return true;
                    case LoadState.LoadAssetBundle:
                        {
                            if (bundle == null || bundle.error != null)
                                return true;

                            for (int i = 0, max = bundle.dependencies.Count; i < max; i++)
                            {
                                var item = bundle.dependencies[i];
                                if (item.error != null)
                                    return true;
                            }

                            if (!bundle.isDone)
                                return false;

                            for (int i = 0, max = bundle.dependencies.Count; i < max; i++)
                            {
                                var item = bundle.dependencies[i];
                                if (!item.isDone)
                                    return false;
                            }

                            _request = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
                            loadState = LoadState.LoadAsset;
                            break;
                        }
                    case LoadState.Unload:
                        break;
                    case LoadState.LoadAsset:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (loadState != LoadState.LoadAsset)
                    return false;
                if (!_request.isDone)
                    return false;
                loadState = LoadState.Loaded;
                return true;
            }
        }

        internal override void Load()
        {
            if (!string.IsNullOrEmpty(assetBundleName))
            {
                bundle = Bundles.LoadAsync(assetBundleName,null);
                loadState = LoadState.LoadAssetBundle;
            }
            else
            {
                _request = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
                loadState = LoadState.LoadAsset;
            }
        }

        internal override void Unload()
        {
            base.Unload();
            _request = null;
        }
    }

    public class WebAssetRequest : AssetRequest
    {
#if UNITY_2018_3_OR_NEWER
        private UnityWebRequest _www;
#else
        private WWW _www;
#endif

        public override bool isDone
        {
            get
            {
                if (loadState == LoadState.Init)
                    return false;
                if (loadState == LoadState.Loaded)
                    return true;

                if (loadState == LoadState.LoadAsset)
                {
                    if (_www == null || !string.IsNullOrEmpty(_www.error))
                        return true;

                    if (_www.isDone)
                    {
#if UNITY_2018_3_OR_NEWER
                        if (assetType != typeof(Texture2D))
                        {
                            if (assetType != typeof(TextAsset))
                            {
                                if (assetType != typeof(AudioClip))
                                    bytes = _www.downloadHandler.data;
                                else
                                    asset = DownloadHandlerAudioClip.GetContent(_www);
                            }
                            else
                            {
                                text = _www.downloadHandler.text;
                            }
                        }
                        else
                        {
                            asset = DownloadHandlerTexture.GetContent(_www);
                        }
#else
                        if (assetType != typeof(Texture2D))
                        {
                            if (assetType != typeof(TextAsset))
                            {
                                if (assetType != typeof(AudioClip))
                                    bytes = _www.bytes;
                                else
                                    asset = _www.GetAudioClip();
                            }
                            else
                            {
                                text = _www.text;
                            }
                        }
                        else
                        {
                            asset = _www.texture;
                        } 
#endif
                        loadState = LoadState.Loaded;
                        return true;
                    }
                    return false;
                }

                return true;
            }
        }

        public override string error
        {
            get { return _www.error; }
        }

        public override float progress
        {
#if UNITY_2018_3_OR_NEWER
            get { return _www.downloadProgress; }
#else
            get { return _www.progress;}
#endif
        }

        internal override void Load()
        {
#if UNITY_2018_3_OR_NEWER
            if (assetType == typeof(AudioClip))
            {
                _www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV);
            }
            else if (assetType == typeof(Texture2D))
            {
                _www = UnityWebRequestTexture.GetTexture(path);
            }
            else
            {
                _www = new UnityWebRequest(path);
                _www.downloadHandler = new DownloadHandlerBuffer();
            }
            _www.timeout = 10;
            _www.SendWebRequest();
#else
            _www = new WWW(name);
#endif
            loadState = LoadState.LoadAsset;
        }

        internal override void Unload()
        {
            if (asset != null)
            {
                Object.Destroy(asset);
                asset = null;
            }
            if (_www != null)
                _www.Dispose();

            bytes = null;
            text = null;
        }
    }
}