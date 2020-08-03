//
// VersionManager.cs
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

using ETModel;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Log = ETModel.Log;

namespace XAsset
{
    public static class Versions
    {
        public const string versionFile = "versions.txt";

        public const string app_versions = "app_versions.bytes";
        //const string res_versions = "res_versions.bytes";

        public static string clientAppVersion = "0.0.0";
        public static string serverAppVersion = "0.0.0";

        public static string clientResVersion = "0.0.0";
        public static string serverResVersion = "0.0.0";

        public static int forceType;

        private const char splitKey = '|';

        private static Dictionary<string, string> clientData = new Dictionary<string, string>();
        private static Dictionary<string, string> serverData = new Dictionary<string, string>();
        private static Dictionary<string, string> tempData = new Dictionary<string, string>();

        private static Dictionary<string, int> BundleSize = new Dictionary<string, int>();

        public static bool hasUpdate;

        public static async ETTask<bool> InitLocalAppVersion()
        {
            var ettcsb = new ETTaskCompletionSource<bool>();

            if (!Utility.assetBundleMode)
            {
                ettcsb.SetResult(true);

                return await ettcsb.Task;
            }

            //client => app_version|res_version
            var persistentVersions = FileUtility.SafeReadAllText(Utility.GetPersistentDataPath(app_versions));
            ResourcesComponent.LoadAsync(Utility.GetStreamingFilePath(app_versions), typeof(TextAsset), (request) =>
            {
                if (request.error != null)
                {
                    Log.Error("load from url : " + request.path + "\n err : " + request.error);
                    ettcsb.SetResult(false); return;
                }
                if (request.text == null)
                {
                    Log.Error("load from url : " + request.path + "\n err : text == null");
                    ettcsb.SetResult(false); return;
                }

                var streamingVersions= request.text.Trim();
                if (string.IsNullOrEmpty(persistentVersions))
                {
                    persistentVersions = streamingVersions;
                    var arr = persistentVersions.Split(splitKey);
                    if (arr.Length == 2)
                    {
                        clientAppVersion = arr[0];
                        clientResVersion = arr[1];
                    }
                    FileUtility.SafeWriteAllText(Utility.GetPersistentDataPath(app_versions), persistentVersions);
                    Log.Debug(string.Format("client version = {0}", persistentVersions));
                    ettcsb.SetResult(true);
                }
                else
                {
                    var arr = streamingVersions.Split(splitKey);
                    if (arr.Length == 2)
                    {
                        clientAppVersion = arr[0];
                        clientResVersion = arr[1];
                    }

                    arr = persistentVersions.Split(splitKey);
                    if (arr.Length == 2)
                    {
                        //cover app :streaming > persistent
                        if (Utility.CheckIsNewVersion(arr[0], clientAppVersion))
                        {
                            ResourcesComponent.CleanUp();
                            FileUtility.SafeClearDirExcept(Utility.updatePath,Utility.ExtraFolderName);
                            FileUtility.SafeWriteAllText(Utility.GetPersistentDataPath(app_versions), streamingVersions);
                            Log.Warning($"new version app ({clientAppVersion}>{arr[0]}) recover install, restart resourcesMgr.");

                            ResourcesComponent.Initialize(() => {
                                ettcsb.SetResult(true);
                            }, (err) => {
                                ettcsb.SetResult(false);
                            });
                        }
                        else
                        {
                            clientResVersion = arr[1];
                            ettcsb.SetResult(true);
                        }
                    }
                    Log.Debug(string.Format("client version = {0}|{1}", clientAppVersion, clientResVersion));
                }
            });
            return await ettcsb.Task;
        }

        public static async ETTask<bool> DownloadServerAppVersion()
        {
            var ettcsb = new ETTaskCompletionSource<bool>();

            //server => app_version|res_version
            ResourcesComponent.LoadAsync(Utility.GetDownloadURL(app_versions), typeof(TextAsset), (request) =>
            {
                if (request.error != null)
                {
                    Log.Error("load from url : " + request.path + "\n err : " + request.error);
                    ettcsb.SetResult(false);
                    return;
                }
                if (request.text == null)
                {
                    Log.Error("load from url : " + request.path + "\n err : text == null");
                    ettcsb.SetResult(false);
                    return;
                }

                var serverVersions = request.text.Trim().Replace("\r", "");

                var arr = serverVersions.Split(splitKey);
                if (arr.Length == 2)
                {
                    serverAppVersion = arr[0];
                    serverResVersion = arr[1];
                }

                Log.Debug(string.Format("server version = {0}", serverVersions));
                ettcsb.SetResult(true);
                request.Release();
            });
            return await ettcsb.Task;
        }

        public static ETTask<bool> LoadClientVersionHash()
        {
            var tcsb = new ETTaskCompletionSource<bool>();
            ResourcesComponent.LoadAsync(Utility.GetStreamingFilePath(versionFile), typeof(TextAsset), (request) =>
            {
                if (request.error != null)
                {
                    Debug.LogError("load local version file exception:" + request.error);
                    tcsb.SetResult(false); return;
                }
                if (request.text == null)
                {
                    Debug.LogError("load local version file exception: text == null");
                    tcsb.SetResult(false); return;
                }

                string version_file = FileUtility.SafeReadAllText(Utility.GetPersistentDataPath(versionFile));
                if (string.IsNullOrEmpty(version_file))
                {
                    version_file = request.text;
                    FileUtility.SafeWriteAllText(Utility.GetPersistentDataPath(versionFile), version_file);
                }

                LoadText2Map(version_file,ref clientData);
                request.Release();
                tcsb.SetResult(true);
            });

            return tcsb.Task;
        }

        public static bool LoadTempVersionHash()
        {
            string version_file = FileUtility.SafeReadAllText(Utility.GetTempPath4Update(versionFile));
            if (!string.IsNullOrEmpty(version_file))
            {
                LoadText2Map(version_file,ref tempData);
                return true;
            }
            return false;
        }
        
        public static ETTask<bool> LoadServerVersionHash()
        {
            var tcsb = new ETTaskCompletionSource<bool>();
            ResourcesComponent.LoadAsync(Utility.GetDownloadURL(versionFile), typeof(TextAsset), (request) =>
            {
                if (request.error != null)
                {
                    Log.Error("load server version file exception:" + request.error);
                    tcsb.SetResult(false);
                    return;
                }

                if (request.text == null)
                {
                    Log.Error("load server version file exception: text == null");
                    tcsb.SetResult(false);
                    return;
                }

                LoadText2Map(request.text, ref serverData);
                LoadText2SizeMap(request.text, ref BundleSize);

                tcsb.SetResult(true);
                request.Release();
            });

            return tcsb.Task;
        }

        public static List<Download> CheckUpdate()
        {
            var updateList = new List<Download>();
            foreach (var item in serverData)
            {
                bool has1 = clientData.TryGetValue(item.Key, out string ver1);
                bool has2 = tempData.TryGetValue(item.Key, out string ver2);
                //Log.Warning(item.Key+"->"+item.Value+"|"+ver);
                if ((!has1 || !ver1.Equals(item.Value)) && (!has2 || !ver2.Equals(item.Value)))
                {
                    var downloader = new Download();
                    downloader.url = Utility.GetDownloadURL(item.Key);
                    downloader.path = item.Key;
                    downloader.version = item.Value;
                    downloader.savePath = Utility.GetTempPath4Update(item.Key);
                    updateList.Add(downloader);
                }
            }

            if (updateList.Count > 0)
            {
                var downloader = new Download();
                downloader.url = Utility.GetDownloadURL(Utility.GetPlatform());
                downloader.path = Utility.GetPlatform();
                downloader.savePath = Utility.GetTempPath4Update(Utility.GetPlatform());
                updateList.Add(downloader);
            }

            Log.Warning(string.Format("updateList count = {0}", updateList.Count));
            return updateList;
        }

        public static void SaveAllFiles()
        {
            WriteToDisk(serverData);
        }

        public static void SaveTempFiles(List<Download> downloaded)
        {
            foreach (var item in downloaded)
            {
                if(tempData.ContainsKey(item.path))
                {
                    tempData[item.path] = serverData[item.path];
                }
                else
                {
                    tempData.Add(item.path,serverData[item.path]);
                }
            }

            WriteToDisk(tempData, true);
        }

        private static void WriteToDisk(Dictionary<string,string> data,bool temp=false)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in data)
            {
                sb.AppendLine(string.Format("{0}|{1}", item.Key, item.Value));
            }

            //save to disk
            if(temp)
                FileUtility.SafeWriteAllText(Utility.GetTempPath4Update(versionFile), sb.ToString());
            else
                FileUtility.SafeWriteAllText(Utility.GetRelativePath4Update(versionFile), sb.ToString());
        }
        
        public static void SaveResVersion()
        {
            clientResVersion = serverResVersion;
            string content = $"{clientAppVersion}|{serverResVersion}";

            FileUtility.SafeWriteAllText(Utility.GetRelativePath4Update(app_versions), content);
        }
        
        public static void LoadText2Map(string text, ref Dictionary<string, string> map)
        {
            map.Clear();
            if (string.IsNullOrEmpty(text)) 
                return;

            using (var reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var fields = line.Split(splitKey);
                    if (fields.Length > 1)
                    {
                        map.Add(fields[0], fields[1]);
                    }
                }
            }
        }

        public static void LoadText2SizeMap(string text, ref Dictionary<string, int> map)
        {
            map.Clear();
            if (string.IsNullOrEmpty(text))
                return;

            using (var reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var fields = line.Split(splitKey);
                    if (fields.Length > 2)
                    {
                        map.Add(fields[0], int.Parse(fields[2]));
                    }
                }
            }
        }

        public static bool GetSize(string path,out int size)
        {
            return BundleSize.TryGetValue(path, out size);
        }

        public static void Clear()
        {
            clientData.Clear();
            serverData.Clear();
        }
    }
}