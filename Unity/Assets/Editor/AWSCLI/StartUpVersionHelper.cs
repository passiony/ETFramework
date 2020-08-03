using UnityEngine;
using UnityEditor;
using System.Text;
using ETModel;
using System;
using LitJson;
using System.Collections.Generic;

///version/getAll 条件查询
///version/modifyBack 通过id修改数据
///version/modify 通过平台渠道修改数据 
///version/add 添加 
///version/del 删除
namespace ETEditor
{
    public class StartUpVersionHelper: Editor
    {
        public class UploadData
        {
            public string appUrl;
            public string resUrl;
            public string appVersion;
            public string resVersion;
            public string platform;
            public string channel;
            public int status = 1;
            public int forceType = 0;
        }

        public static Dictionary<string, string> Head
        {
            get
            {
                Dictionary<string,string> head=new Dictionary<string, string>();
                head.Add("Cookie", "token=" + URLSetting.TOKEN);
                return head;
            }
        }
        
        /// <summary>
        /// 资源版本自增
        /// </summary>
        public static void AutoIncreaseResVersion(bool isNewAppVer)
        {
            string versionInfo = BuildScript.GetManifest().resVersion;
            var arr = versionInfo.Split('.');
            int resVersion = !isNewAppVer? GetAddVersion(arr) : 1;

            string appVersion = BuildScript.GetManifest().appVersion;
            var appArray = appVersion.Split('.');
            int appversion = GetAddVersion(appArray);
            appversion--;

            BuildScript.GetManifest().resVersion = $"{arr[0]}.{appversion}.{resVersion}";
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// App版本自增
        /// </summary>
        public static void AutoIncreaseAppVersion()
        {
            string versionInfo = BuildScript.GetManifest().appVersion;
            var arr = versionInfo.Split('.');
            int version = GetAddVersion(arr);
            BuildScript.GetManifest().appVersion = $"{arr[0]}.{arr[1]}.{version}";
            AssetDatabase.Refresh();
        }

        private static int GetAddVersion(string[] versionInfo)
        {
            if (versionInfo.Length < 3)
            {
                Debug.LogError("Version 版本的格式错误，请修改为x.x.xxx");
                return -1;
            }

            //版本自增
            int version = int.Parse(versionInfo[2]);
            version++;
            return version;
        }

        public static void LoadVersionFile(Action callback)
        {
            SelectCurrentActiveVersion((version) =>
            {
                if (version == null)
                {
                    return;
                }

                var manifest = BuildScript.GetManifest();
                manifest.appVersion = version.appVersion;
                manifest.resVersion = version.resVersion;

                AssetDatabase.Refresh();

                callback?.Invoke();
            });
        }

        public static void SaveVersionFile()
        {
            SelectCurrentActiveVersion((version) =>
            {
                if (version == null)
                {
                    AddNewVersion();
                    return;
                }

                var manifest = BuildScript.GetManifest();
                if (version.resVersion != manifest.resVersion)
                {
                    version.resUrl = version.resUrl.Remove(version.resUrl.LastIndexOf("/")) + "/" + manifest.resVersion;
                    version.resVersion = manifest.resVersion;

                    string json = JsonMapper.ToJson(version);
                    byte[] bytes = Encoding.UTF8.GetBytes(json);
                    Debug.LogWarning("modify version:" + json);
                    
                    Dictionary<string,string> head=new Dictionary<string, string>();
                    head.Add("token",URLSetting.TOKEN);
                    NetworkHttp.Instance.Post(URLSetting.VERSION_URL + "/modify", bytes,
                        (result) => { Debug.LogWarning("版本信息修改成功"); }, 
                        0, Head);
                }
                else
                {
                    Debug.LogWarning("相同版本信息已存在");
                }
            });
        }

        public static void SetServerResVersion()
        {
            SelectCurrentActiveVersion((version) =>
            {
                if (version == null)
                {
                    Debug.LogError("服务器版本不存在");
                    return;
                }

                var manifest = BuildScript.GetManifest();
                Debug.Log(version.resVersion + "-->" + manifest.resVersion);
                if (version.resVersion != manifest.resVersion)
                {
                    version.resVersion = manifest.resVersion;

                    string json = JsonMapper.ToJson(version);
                    byte[] bytes = Encoding.UTF8.GetBytes(json);
                    Debug.LogWarning("modify version:" + json);
                    NetworkHttp.Instance.Post(URLSetting.VERSION_URL + "/modify", bytes, 
                        (result) => { Debug.LogWarning("资源版本信息修改成功"); }, 
                        0, Head);
                }
                else
                {
                    Debug.LogWarning("相同资源版本信息已存在");
                }
            });
        }

        static void AddNewVersion()
        {
            //刷新当前版本
            var manifest = BuildScript.GetManifest();
            var upload = new UploadData();

            string channelName = manifest.channelType.ToString();
            string platformName = BuildScript.GetPlatformName();

            upload.appUrl = URLSetting.APP_URL;
            upload.resUrl = $"{URLSetting.RES_CDN_URL}/{channelName}/{platformName}/{manifest.resVersion}";
            upload.appVersion = manifest.appVersion;
            upload.resVersion = manifest.resVersion;
            upload.platform = platformName;
            upload.channel = channelName;
            upload.status = 1;
            upload.forceType = 0;

            string json = JsonMapper.ToJson(upload);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            Debug.LogWarning("add version:" + json);

            NetworkHttp.Instance.Post(URLSetting.VERSION_URL + "/add", bytes, (result) => { }, 
                0, Head);
        }

        public static void SelectCurrentActiveVersion(Action<UploadData> callback)
        {
            //刷新当前版本
            var manifest = BuildScript.GetManifest();
            var upload = new UploadData();

            string channelName = manifest.channelType.ToString();
            string platformName = BuildScript.GetPlatformName();
            string appVersion = manifest.appVersion;

            upload.platform = platformName;
            upload.channel = channelName;
            upload.status = 1;
            upload.appVersion = appVersion;

            string json = JsonMapper.ToJson(upload);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            Debug.Log("post-> " + json);
            NetworkHttp.Instance.Post(URLSetting.VERSION_URL + "/getAll", bytes, (result) =>
            {
                if (result == null)
                {
                    callback?.Invoke(null);
                    return;
                }

                var dic = JsonMapper.ToObject(result);
                if (Convert.ToInt32(dic["code"].ToString()) != 0)
                {
                    callback?.Invoke(null);
                    return;
                }

                var data = JsonMapper.ToObject<List<UploadData>>(dic["data"]["data"].ToJson());
                callback?.Invoke(data.Count > 0? data[0] : null);
            }, 0, Head);
        }

        /// <summary>
        /// 根据App版本同步当前资源版本
        /// </summary>
        public static void SyncLocalResVersion(Action callback)
        {
            SelectCurrentActiveVersion((version) =>
            {
                if (version == null)
                {
                    Debug.LogError("当前服务器App版本不存在");
                    return;
                }
                AwscliGeneratorEditor.CreateWinUploadFileFromWeb(version);
                BuildScript.GetManifest().resVersion = version.resVersion;
                AssetDatabase.Refresh();
                callback?.Invoke();
            });
        }
    }
}