using XAsset;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace ETModel
{
	public static class BundleHelper
	{
        public const string NEWBIE_PREFS = "Tutorial_Newbie";

        public static async ETTask DownloadBundle()
        {
            try
            {
                using (AssetsUpdateComponent updater = Game.Scene.AddComponent<AssetsUpdateComponent>())
                {

                    if (!Utility.assetBundleMode)
                    {
                        Log.Warning("非bundle模式，跳过资源更新");
                        return;
                    }

                    if (!PlayerPrefs.HasKey(NEWBIE_PREFS))
                    {
                        Log.Warning("新手跳过资源更新");
                        return;
                    }

                    //无网直接跳过热更
                    if (Application.internetReachability == NetworkReachability.NotReachable)
                    {
                        Log.Warning("无网络，跳过资源更新");
                        return;
                    }

                    if (Utility.IsDebugMode())
                    {
                        URLSetting.RES_DOWNLOAD_URL = URLSetting.RES_CDN_URL + Utility.channelName + "/" + Utility.GetPlatform() + "/";
                    }
                    else
                    {
                        //请求外网
                        bool success = await OutnetGetUrlList();
                        if (!success)
                        {
                            Log.Warning("请求url失败，跳过资源更新");
                            return;
                        }
                    }

                    await updater.CheckUpdateOrDownloadGame();
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }


        public static ETTask<bool> OutnetGetUrlList()
        {
            var ettcsb = new ETTaskCompletionSource<bool>();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("platform", Utility.GetPlatform());
            dic.Add("channel", Utility.channelName);
            dic.Add("appVersion", Versions.clientAppVersion);
            dic.Add("resVersion", Versions.clientResVersion);
            // dic.Add("idfa", NativeBridgeManager.Instance.GetDeviceID());

            string args = JsonHelper.ToJson(dic);
            byte[] bytes = Encoding.UTF8.GetBytes(args);
            Log.Debug(URLSetting.START_UP_URL + args);
            NetworkHttp.Instance.Post(URLSetting.START_UP_URL, bytes, (json) =>
            {
                if (json == null)
                {
                    ettcsb.SetResult(false);
                    return;
                }

                var jsonData = LitJson.JsonMapper.ToObject(json);
                if (jsonData == null || jsonData["code"].ToString() != "0")
                {
                    ettcsb.SetResult(false);
                    return;
                }

                if (jsonData["data"] == null)
                {
                    ettcsb.SetResult(false);
                    return;
                }

                Versions.forceType = int.Parse(jsonData["data"]["forceType"].ToString());
                Versions.serverAppVersion = jsonData["data"]["appVersion"].ToString().Trim();
                Versions.serverResVersion = jsonData["data"]["resVersion"].ToString().Trim();
                URLSetting.APP_DOWNLOAD_URL = jsonData["data"]["appUrl"].ToString().Trim();
                URLSetting.RES_DOWNLOAD_URL = jsonData["data"]["resUrl"].ToString().Trim();
                Log.Debug(string.Format("server version = {0}|{1}", Versions.serverAppVersion, Versions.serverResVersion));
                ettcsb.SetResult(true);
            }, 5, null);

            return ettcsb.Task;

        }
    }
}
