//
// Bundle.cs
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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace XAsset
{
	public class BundleRequest : AssetRequest
	{
		public readonly List<BundleRequest> dependencies = new List<BundleRequest>();

		public AssetBundle assetBundle
		{
			get { return asset as AssetBundle; }
			internal set { asset = value; }
		}

		internal override void Load()
		{
			asset = AssetBundle.LoadFromFile(path);
			if (assetBundle == null)
				error = path + " LoadFromFile failed.";
		}

		internal override void Unload()
		{
			if (assetBundle == null)
				return;
			assetBundle.Unload(false);
			assetBundle = null;
		}
		
		public override void Release()
		{
			this.refCount--;
			if (this.IsUnused())
			{
				Bundles.UnUse(this);
			}
		}
	}

	public class BundleAsyncRequest : BundleRequest
	{
		private AssetBundleCreateRequest _request;

		public override bool isDone
		{
			get
			{
				if (loadState == LoadState.Init)
					return false;

				if (loadState == LoadState.Loaded)
					return true;

				if (loadState == LoadState.LoadAssetBundle && _request.isDone)
				{
					asset = _request.assetBundle;
					if (_request.assetBundle == null) {
						error = string.Format ("unable to load assetBundle:{0}", path);
					}
					loadState = LoadState.Loaded;
				}

				return _request == null || _request.isDone;
			}
		}

		public override float progress
		{
			get { return _request != null ? _request.progress : 0f; }
		}

		internal override void Load()
		{
			_request = AssetBundle.LoadFromFileAsync(path);
			if (_request == null)
			{
				error = path + " LoadFromFile failed.";
				return;
			}
			loadState = LoadState.LoadAssetBundle;
		}

		internal override void Unload()
		{
			if (_request != null)
			{
				_request = null;
			}
			loadState = LoadState.Unload;
			base.Unload();
		}
	}

	public class WebBundleRequest : BundleRequest
	{
		#if UNITY_2018_3_OR_NEWER
        private UnityWebRequest _request;


#else
		private WWW _request;
		#endif
		public bool cache;
		public Hash128 hash;

		public override string error
		{
			get { return _request != null ? _request.error : null; }
		}

		public override bool isDone
		{
			get
			{
				if (loadState == LoadState.Init)
					return false;

				if (_request == null || loadState == LoadState.Loaded)
					return true;
#if UNITY_2018_3_OR_NEWER
				if (_request.isDone)
				{
					assetBundle = DownloadHandlerAssetBundle.GetContent(_request);
					loadState = LoadState.Loaded;
				}
#else
                if (_request.isDone)
                {
                    assetBundle = _request.assetBundle;
                    loadState = LoadState.Loaded;
                }
#endif

				return _request.isDone;
			}
		}

		public override float progress
		{
#if UNITY_2018_3_OR_NEWER
            get { return _request != null ? _request.downloadProgress : 0f; }
#else
			get { return _request != null ? _request.progress : 0f; }
#endif
		}

		internal override void Load()
		{
#if UNITY_2018_3_OR_NEWER
			_request = cache ? UnityWebRequestAssetBundle.GetAssetBundle(path,hash) : UnityWebRequestAssetBundle.GetAssetBundle(path);
			_request.SendWebRequest();
#else
            _request = cache ? WWW.LoadFromCacheOrDownload(name, hash) : new WWW(name);
#endif
			loadState = LoadState.LoadAssetBundle;

		}

		internal override void Unload()
		{
			if (_request != null)
			{
				_request.Dispose();
				_request = null;
			}
			loadState = LoadState.Unload;
			base.Unload();
		}
	}
}