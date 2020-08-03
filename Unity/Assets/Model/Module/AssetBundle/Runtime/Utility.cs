//
// Utility.cs
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
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace XAsset
{
    public static class Utility
    {
        public const string AssetBundles = "AssetBundles";
        public static string AssetRootPath = "Assets/Bundles/";
        public const string AssetsSettingAsset = "Assets/Bundles/Settings.asset";
        public const string AssetsManifestAsset = "Assets/Bundles/Manifest.asset";

        public const string AssetBundleSuffix = ".assetbundle";
        public const string AssetsFolderName = "Bundles";
        public const string ExtraFolderName = "Extra";
        public const string TempFolderName = "Temp";
        public const string DatabaseRoot = "Assets/Editor/AssetBundle/Database";

        public const string app_versions = "app_versions.bytes";

        public static bool assetBundleMode = true;
        public static ServerMode runMode = ServerMode.Local;
        public static string channelName = "Release";
        public static Func<string,Type,Object> loadDelegate = null;
        public static Func<string,Object[]> loadAllDelegate = null;
        
        public static Func<string> getPlatformDelegate = null;

        public static string downloadURL
        {
            get
            {
                return ETModel.URLSetting.RES_DOWNLOAD_URL;
            }
        }

        public static string GetPlatform()
        {
            return getPlatformDelegate != null
                ? getPlatformDelegate()
                : GetPlatformForAssetBundles(Application.platform);
        }

        public static string DeviceModel()
        {
#if UNITY_EDITOR
            return "Windows";
#else
            return SystemInfo.deviceModel;
#endif
        }
        
        public static bool IsDebugMode()
        {
            return runMode == ServerMode.Local;
        }

        /// <summary>
        /// app发布是否为产品环境（线上环境）
        /// </summary>
        /// <returns></returns>
        public static bool IsProductionEnv()
        {
            return channelName != null && (channelName.Equals(ChannelType.AppStore.ToString()) || channelName.Equals(ChannelType.GooglePlay.ToString()));
        }

        private static string GetPlatformForAssetBundles(RuntimePlatform platform)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                case RuntimePlatform.WebGLPlayer:
                    return "WebGL";
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return "Windows";
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    return "OSX";
                default:
                    return null;
            }
        }

        public static string TempPath
        {
            get
            {
                return Path.Combine(Application.persistentDataPath, AssetBundles, TempFolderName) +
                        Path.DirectorySeparatorChar;
            }
        }
        
        public static string updatePath
        {
            get
            {
                return Path.Combine(Application.persistentDataPath, AssetBundles, GetPlatform()) +
                       Path.DirectorySeparatorChar;
            }
        }

        public static string dataPath
        {
            get
            {
                return Path.Combine(Application.streamingAssetsPath, AssetBundles, GetPlatform()) +
                          Path.DirectorySeparatorChar;
            }
        }

        public static string GetTempPath4Update(string filepath)
        {
            var path = Path.Combine(TempPath, filepath);

            return path;
        }
        
        public static string GetRelativePath4Update(string filepath)
        {
            var path = Path.Combine(updatePath, filepath);

            return path;
        }

        public static string GetDownloadURL(string filename)
        {
            return Path.Combine(downloadURL, filename);
        }

        public static string GetPersistentFilePath(string filename)
        {
            var path = "file://" + GetPersistentDataPath(filename);

            return path;
        }

        public static string GetPersistentDataPath(string filename)
        {
            var path = Path.Combine(updatePath, filename);

            return path;
        }

        public static string GetStreamingFilePath(string filename)
        {
            var path = Path.Combine(dataPath, filename);
#if UNITY_IOS || UNITY_EDITOR
            path = "file://" + path;
#elif UNITY_STANDALONE_WIN
            path = "file:///" + path;
#endif
            return path;
        }

        public static string GetStreamingDataPath(string filename)
        {
            var path = Path.Combine(dataPath, filename);

#if UNITY_STANDALONE_WIN
            return FileUtility.FormatToSysFilePath(path);
#endif
            return path;
        }

        /// <summary>
        /// 判断沙盒有误指定文件，没有返回streamingAsset路径（注意：streaming路径在移动端不能使用file读取，必须使用www）
        /// </summary>
        /// <param name="filePath">文件相对路径</param>
        /// <returns>沙盒路径或者streaming路径</returns>
        public static string GetFileUrl(string filePath)
        {
            string persistentPath = GetPersistentDataPath(filePath);
            if (File.Exists(persistentPath))
            {
                return persistentPath;
            }
            else
            {
                return GetStreamingDataPath(filePath);
            }
        }

        public static bool CheckIsNewVersion(string sourceVersion, string targetVersion)
        {
            if (string.IsNullOrEmpty(sourceVersion) || string.IsNullOrEmpty(targetVersion))
            {
                return false;
            }

            string[] sVerList = sourceVersion.Split('.');
            string[] tVerList = targetVersion.Split('.');

            if (sVerList.Length >= 3 && tVerList.Length >= 3)
            {
                try
                {
                    int sV0 = int.Parse(sVerList[0]);
                    int sV1 = int.Parse(sVerList[1]);
                    int sV2 = int.Parse(sVerList[2]);
                    int tV0 = int.Parse(tVerList[0]);
                    int tV1 = int.Parse(tVerList[1]);
                    int tV2 = int.Parse(tVerList[2]);

                    if (tV0 > sV0)
                    {
                        return true;
                    }
                    else if (tV0 < sV0)
                    {
                        return false;
                    }

                    if (tV1 > sV1)
                    {
                        return true;
                    }
                    else if (tV1 < sV1)
                    {
                        return false;
                    }

                    if (tV2 > sV2)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(string.Format("parse version error. clientversion: {0} serverversion: {1}\n {2}\n{3}", sourceVersion, targetVersion, ex.Message, ex.StackTrace));
                    return false;
                }
            }

            return false;
        }

        public static bool CheckIsSameVersion(string sourceVersion, string targetVersion)
        {
            if (string.IsNullOrEmpty(sourceVersion) || string.IsNullOrEmpty(targetVersion))
            {
                return false;
            }

            string[] sVerList = sourceVersion.Split('.');
            string[] tVerList = targetVersion.Split('.');

            if (sVerList.Length >= 3 && tVerList.Length >= 3)
            {
                try
                {
                    int sV0 = int.Parse(sVerList[0]);
                    int sV1 = int.Parse(sVerList[1]);
                    int sV2 = int.Parse(sVerList[2]);
                    int tV0 = int.Parse(tVerList[0]);
                    int tV1 = int.Parse(tVerList[1]);
                    int tV2 = int.Parse(tVerList[2]);

                    if (tV0 == sV0 && tV1 == sV1 && tV2 == sV2)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(string.Format("parse version error. clientversion: {0} serverversion: {1}\n {2}\n{3}", sourceVersion, targetVersion, ex.Message, ex.StackTrace));
                    return false;
                }
            }

            return false;
        }
        
        public static string KBSizeToString(int kbSize)
        {
            string sizeStr = string.Empty;
            if (kbSize >= 1024)
            {
                sizeStr = (kbSize / 1024.0f).ToString("0.0") + "MB";
            }
            else
            {
                sizeStr = kbSize + "KB";
            }

            return sizeStr;
        }
    }
}