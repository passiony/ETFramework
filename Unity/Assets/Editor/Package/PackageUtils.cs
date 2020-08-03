using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using ETModel;
using UnityEditor;
using UnityEngine;
using XAsset;
using Debug = UnityEngine.Debug;
using BuildSettingType = ETEditor.SettingsInspector.BuildSettingType;
using ChannelType = XAsset.ChannelType;

namespace ETEditor
{
    public static class PackageUtils
    {
        public const string TerminalPrefs = "EDITOR_PATH_TERMINAL";
        public const string BuildAppPrefs = "EDITOR_PATH_BUILD_APP";
        public const string LAST_RELEASE_IOS_PATH = "LastReleaseIosPath";

        public static void SaveCurSelectedChannel(ChannelType channelType)
        {
            EditorPrefs.SetString("ChannelName", channelType.ToString());
        }

        public static void OpenVersionUrl()
        {
            Application.OpenURL(URLSetting.BASE_URL + "/static/version");
        }

        public static void OpenAppOutputPath()
        {
            string path = PackageTool.BuildAppPath;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            EditorUtility.RevealInFinder(path);
        }

        public static void ADBInstallApp(string apkName)
        {
            string path = Application.dataPath.Replace("Unity/Assets", "Release");

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.Arguments = "adb install " + path + "/" + apkName;
            info.UseShellExecute = true;
            info.ErrorDialog = true;
            info.WorkingDirectory = path;
            info.CreateNoWindow = false;

            Debug.Log(info.Arguments);
            Process.Start(info);
        }

        private static void Build(AutoBuildType type)
        {
            string typeStr = type.ToString();
            try
            {
                if (typeStr.Contains("1"))
                {
                    EditorUtility.DisplayProgressBar("AutoBuild", "Auto Add Local App Version", 0.5f);
                    StartUpVersionHelper.AutoIncreaseAppVersion();
                }

                if (typeStr.Contains("2"))
                {
                    EditorUtility.DisplayProgressBar("AutoBuild", "Auto Add Local Res Version", 0.5f);
                    StartUpVersionHelper.AutoIncreaseResVersion(typeStr.Contains("1"));
                }

                if (typeStr.Contains("3"))
                {
                    EditorUtility.DisplayProgressBar("AutoBuild", "生成AwsCli File", 0.3f);
                    AwscliGeneratorEditor.GenerateAwsCliFile();
                }

                if (typeStr.Contains("4"))
                {
                    EditorUtility.DisplayProgressBar("AutoBuild", "开始Build Bundles", 0.2f);
                    BuildScript.BuildAssetBundles();
                }

                if (typeStr.Contains("5"))
                {
                    EditorUtility.DisplayProgressBar("AutoBuild", "Copy AssetBundles", 0.4f);
                    AssetsMenuItem.CopyAssetBundles(false);
                }

                EditorUtility.ClearProgressBar();
                if (typeStr.Contains("9"))
                {
                    BuildScript.BuildApp();
                }

                if (typeStr.Contains("7"))
                {
                    EditorUtility.DisplayProgressBar("AutoBuild", "Alter Server Res Version", 0.8f);
                    StartUpVersionHelper.SetServerResVersion();
                }

                if (typeStr.Contains("8"))
                {
                    EditorUtility.DisplayProgressBar("AutoBuild", "Alter Server resUrl And Res Version", 1);
                    StartUpVersionHelper.SaveVersionFile();
                }

                if (typeStr.Contains("6"))
                {
                    EditorUtility.DisplayProgressBar("AutoBuild", "Upload AWS Files", 0.8f);
                    RestCDNFlushFile(AwscliGeneratorEditor.ExcuteUpload);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void RestCDNFlushFile(Action callback)
        {
            StartUpVersionHelper.SelectCurrentActiveVersion((data) =>
            {
                string[] infoTemp = data.resUrl.Split('/');
                string resFile = infoTemp[infoTemp.Length - 1];
                string rootpath = Application.dataPath.Replace("Assets", Utility.AssetBundles);
                string flushDir = $"{rootpath}/flush/";
                string[] files = Directory.GetFiles(flushDir, "*.flush");
                string[] lines = File.ReadAllLines(files[files.Length - 1]);
                string content = string.Empty;
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    List<string> lineList = new List<string>(line.Split('/'));
                    lineList[3] = resFile;
                    content += string.Join("/", lineList) + "\n";
                }

                File.WriteAllText(files[files.Length - 1], content);

                callback?.Invoke();
            });
        }

        public static void RefreshAllCDN(ChannelType channelType)
        {
            string versionPath = EditorUtility.OpenFolderPanel("选取versions.txt父目录", Application.dataPath + "/../AssetBundles",
                string.Empty);
            if (string.IsNullOrEmpty(versionPath))
            {
                return;
            }

            versionPath += "/versions.txt";
            if (!File.Exists(versionPath))
            {
                throw new Exception($"文件不存在：{versionPath}");
            }

            string[] fileInfo = File.ReadAllLines(versionPath);
            List<string> infoList = new List<string>();
            foreach (string info in fileInfo)
            {
                if (!string.IsNullOrEmpty(info))
                {
                    infoList.Add(info.Split('|')[0]);
                }
            }

            string cdnStrTemp = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android? "Android" : "iOS";

            string flushPath = string.Format(Application.dataPath + "/../AssetBundles/flush/{0}{1}.flush",
                cdnStrTemp, DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss"));
            StartUpVersionHelper.SelectCurrentActiveVersion((data) =>
            {
                if (data == null)
                {
                    var manifest = BuildScript.GetManifest();
                    string channelName = manifest.channelType.ToString();
                    string platformName = BuildScript.GetPlatformName();
                    string appVersion = manifest.appVersion;
                    if (EditorUtility.DisplayDialog("error",
                        $"不存在此appVersion信息，请核实。\nchannelName:{channelName}\nplatformName:{platformName}\nappVersion:{appVersion}", "ok"))
                    {
                        OpenVersionUrl();
                    }

                    return;
                }

                string[] infoTemp = data.resUrl.Split('/');
                string resFile = infoTemp[infoTemp.Length - 1];
                string flushTemp = "";
                for (int i = 0; i < infoList.Count; i++)
                {
                    string temp = $"AssetBundles/{channelType}/{cdnStrTemp}/{resFile}/{infoList[i]}";
                    flushTemp += temp;
                    flushTemp += "\n";
                }

                File.WriteAllText(flushPath, flushTemp, Encoding.UTF8);
                AwscliGeneratorEditor.ExcuteUpload();
            });
        }

        /// <summary>
        /// 打包
        /// </summary>
        /// <param name="type">各种打包类型</param>
        /// <param name="isPrdMod">true:正式包 false:测试包</param>
        public static void BuildApp(AutoBuildType type, bool isPrdMod = false, Action<AutoBuildType> callback = null)
        {
            BuildSettingType buildSettingType;
            if (!isPrdMod)
            {
                buildSettingType = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android? BuildSettingType.AndroidDev
                        : BuildSettingType.IosDev;
            }
            else
            {
                buildSettingType = BuildSettingType.IosPrd;
            }

            if (!SettingsInspector.CheckDefine(buildSettingType))
            {
                if (EditorUtility.DisplayDialog("AddDefine", "need rest your define symbols.\nclick again after compiling.", "confirm", "cancel"))
                {
                    SettingsInspector.AddDefine(buildSettingType);
                }

                return;
            }

            StartUpVersionHelper.SyncLocalResVersion(() =>
            {
                Build(type);
                callback(type);
            });
        }
    }
}