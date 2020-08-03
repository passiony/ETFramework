//
// BuildScript.cs
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEditor;
using UnityEngine;
using XAsset;
using Debug = UnityEngine.Debug;

namespace ETEditor
{
    public static class BuildScript
    {
        public const string BuildPath = "../Release";
        public static string outputPath;
        public static string overloadedDevelopmentServerURL = "";

        public static void CopyAssetBundlesTo(string outputPath)
        {
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            var outputFolder = GetPlatformName();
            var source = Path.Combine(Environment.CurrentDirectory, Utility.AssetBundles, GetChannelName(), outputFolder);

            if (!Directory.Exists(source))
                Debug.Log("No assetBundle output folder, try to build the assetBundles first.");
            var destination = Path.Combine(outputPath, outputFolder);
            if (Directory.Exists(destination))
                FileUtil.DeleteFileOrDirectory(destination);

            FileUtil.CopyFileOrDirectory(source, destination);
        }

        public static string GetChannelName()
        {
            return GetManifest().channelType.ToString();
        }

        public static string GetPlatformName()
        {
            return GetPlatformForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
        }

        public static string GetDownloadUrl()
        {
            return GetManifest().downloadURL;
        }
        public static BuildTargetGroup GetActiveTargetGroup()
        {
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return BuildTargetGroup.Standalone;
                case BuildTarget.Android:
                    return BuildTargetGroup.Android;
                case BuildTarget.iOS:
                    return BuildTargetGroup.iOS;
                default:
                    return BuildTargetGroup.Android;
            }
        }

        private static string GetPlatformForAssetBundles(BuildTarget target)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (target)
            {
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.iOS:
                    return "iOS";
                case BuildTarget.WebGL:
                    return "WebGL";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
#if UNITY_2017_3_OR_NEWER
                case BuildTarget.StandaloneOSX:
                    return "OSX";
#else
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
                case BuildTarget.StandaloneOSXUniversal:
                    return "OSX";
#endif
                default:
                    return null;
            }
        }

        private static string[] GetLevelsFromBuildSettings()
        {
            return EditorBuildSettings.scenes.Select(scene => scene.path).ToArray();
        }

        private static string GetAssetBundleManifestFilePath()
        {
            var relativeAssetBundlesOutputPathForPlatform = Path.Combine(Utility.AssetBundles, GetPlatformName());
            return Path.Combine(relativeAssetBundlesOutputPathForPlatform, GetPlatformName()) + ".manifest";
        }

        public static string BuildApp()
        {
            //var outputPath = EditorUtility.SaveFolderPanel("Choose Location of the Built Game", "", "");
            //if (outputPath.Length == 0)
            //    return;
            PlayerSettings.bundleVersion = GetManifest().appVersion;

            var levels = GetLevelsFromBuildSettings();
            if (levels.Length == 0)
            {
                Debug.Log("Nothing to build.");
                return null;
            }

            var targetName = GetBuildTargetName(EditorUserBuildSettings.activeBuildTarget);
            if (targetName == null)
                return null;
#if UNITY_5_4 || UNITY_5_3 || UNITY_5_2 || UNITY_5_1 || UNITY_5_0
			BuildOptions option = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
			BuildPipeline.BuildPlayer(levels, outputPath + targetName, EditorUserBuildSettings.activeBuildTarget, option);
#else
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                PlayerSettings.Android.keystoreName = "magical_letter.keystore";
                PlayerSettings.Android.keystorePass = "gillarstudio2020";
                PlayerSettings.Android.keyaliasName = "gillar-releaseKey";
                PlayerSettings.Android.keyaliasPass = "gillar@studio2020";
            }

            string locationPathName = PackageTool.BuildAppPath + targetName;

#if UNITY_EDITOR_WIN
            if (File.Exists(locationPathName))
            {
                File.Delete(locationPathName);
            }
#else
            string lastReleaseIosPath = EditorPrefs.GetString(PackageUtils.LAST_RELEASE_IOS_PATH, string.Empty);
            if (!string.IsNullOrEmpty(lastReleaseIosPath) && Directory.Exists(lastReleaseIosPath))
            {
                Directory.Delete(lastReleaseIosPath, true);
            }

            locationPathName += DateTime.Now.ToString("yyyyMMddHHmmss");
            EditorPrefs.SetString(PackageUtils.LAST_RELEASE_IOS_PATH, locationPathName);
#endif
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = levels,
                locationPathName = locationPathName,
                assetBundleManifestPath = GetAssetBundleManifestFilePath(),
                target = EditorUserBuildSettings.activeBuildTarget,
                options = EditorUserBuildSettings.development? BuildOptions.Development : BuildOptions.None
            };

            BuildPipeline.BuildPlayer(buildPlayerOptions);
            
            PackageUtils.OpenAppOutputPath();
#if !UNITY_EDITOR_WIN
            PodeInstall(locationPathName);
#endif
            return targetName;
#endif
        }

        public static void PodeInstall(string destFilePath)
        {
            return;
            File.Copy(BuildPath + "Ios/Podfile", destFilePath, true);
            string command = "/System/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal";

            string rootPath = Directory.GetCurrentDirectory();
            string shell = rootPath + "/AssetBundles/mac_update_aws.sh";

            Process.Start(command, shell);
        }

        public static string CreateAssetBundleDirectory()
        {
            // Choose the output path according to the build target.
            var outputPath = Path.Combine(Utility.AssetBundles, GetChannelName(), GetPlatformName());
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            return FileUtility.FormatToUnityPath(outputPath);
        }

        private static Dictionary<string, string> GetVersions(AssetBundleManifest manifest)
        {
            var items = manifest.GetAllAssetBundles();
            return items.ToDictionary(item => item, item => manifest.GetAssetBundleHash(item).ToString());
        }

        private static void LoadVersions(string versionsTxt, IDictionary<string, string> versions)
        {
            if (versions == null)
                throw new ArgumentNullException("versions");
            if (!File.Exists(versionsTxt))
                return;
            using (var s = new StreamReader(versionsTxt))
            {
                string line;
                while ((line = s.ReadLine()) != null)
                {
                    if (line == string.Empty)
                        continue;
                    var fields = line.Split('|');
                    if (fields.Length > 1)
                        versions.Add(fields[0], fields[1]);
                }
            }
        }

        public static void SaveVersions(string versionsTxt, Dictionary<string, string> versions)
        {
            if (File.Exists(versionsTxt))
                File.Delete(versionsTxt);

            StringBuilder sb = new StringBuilder();

            foreach (var item in versions)
            {
                string path = Path.Combine(outputPath, item.Key);
                FileInfo fileInfo = new FileInfo(path);

                int size = (int) (fileInfo.Length / 1024) + 1;
                sb.AppendLine(string.Format("{0}|{1}|{2}", item.Key, item.Value, size));
            }

            FileUtility.SafeWriteAllText(versionsTxt, sb.ToString());
        }

        public static void RemoveUnusedAssetBundleNames()
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();
        }

        public static void SetAssetBundleNameAndVariant(string assetPath, string bundleName, string variant)
        {
            var importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null) return;
            importer.assetBundleName = bundleName;
            importer.assetBundleVariant = variant;
        }

        public static void BuildManifest()
        {
            var manifest = GetManifest();

            AssetDatabase.RemoveUnusedAssetBundleNames();
            var bundles = AssetDatabase.GetAllAssetBundleNames();

            List<string> dirs = new List<string>();
            List<AssetData> assets = new List<AssetData>();

            for (int i = 0; i < bundles.Length; i++)
            {
                var paths = AssetDatabase.GetAssetPathsFromAssetBundle(bundles[i]);
                foreach (var path in paths)
                {
                    var dir = Path.GetDirectoryName(path);
                    var index = dirs.FindIndex((o) => o.Equals(dir));
                    if (index == -1)
                    {
                        index = dirs.Count;
                        dirs.Add(dir);
                    }

                    var asset = new AssetData();
                    asset.bundle = i;
                    asset.dir = index;
                    asset.name = Path.GetFileName(path);

                    assets.Add(asset);
                }
            }

            manifest.bundles = bundles;
            manifest.dirs = dirs.ToArray();
            manifest.assets = assets.ToArray();

            var assetPath = AssetDatabase.GetAssetPath(manifest);
            var bundleName = Path.GetFileNameWithoutExtension(assetPath).ToLower();
            SetAssetBundleNameAndVariant(assetPath, bundleName, null);

            EditorUtility.SetDirty(manifest);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void BuildAssetBundles()
        {
            BuildManifest();

            // Choose the output path according to the build target.
            outputPath = CreateAssetBundleDirectory();

            const BuildAssetBundleOptions options = BuildAssetBundleOptions.ChunkBasedCompression;

            var manifest = BuildPipeline.BuildAssetBundles(outputPath, options, EditorUserBuildSettings.activeBuildTarget);

            var versionsTxt = outputPath + "/versions.txt";
            var versions = new Dictionary<string, string>();
            LoadVersions(versionsTxt, versions);
            // BookShopResVersionHelper.LoadBookShopVersions(outputPath, versions);
            // BooksVersionHelper.LoadBooksVersions(outputPath, versions);

            var buildVersions = GetVersions(manifest);

            var updates = new List<string>();

            foreach (var item in buildVersions)
            {
                string hash;
                var isNew = true;
                if (versions.TryGetValue(item.Key, out hash))
                    if (hash.Equals(item.Value))
                        isNew = false;
                if (isNew)
                    updates.Add(item.Key);
            }

            string app_version = "0.0.0";
            string res_version = "0.0.0";
            ReadAppAndResVersions(ref app_version, ref res_version);
            if (!string.Equals(app_version, GetManifest().appVersion) ||
                !string.Equals(res_version, GetManifest().resVersion))
            {
                updates.Add(Utility.app_versions);
            }

            if (updates.Count > 0)
            {
                SaveVersions(versionsTxt, buildVersions);
                SaveAppAndResVersions();

                updates.Add("versions.txt");
                SaveCDNFlushFile(updates);
            }
            else
            {
                Debug.Log("nothing to update.");
            }

            //clear no use files
            string[] ignoredFiles = { GetPlatformName(), "versions.txt", "manifest", "app_versions.bytes" };
            var files = Directory.GetFiles(outputPath, "*", SearchOption.AllDirectories);
            var deletes = (from t in files
                let file = t.Replace('\\', '/').Replace(outputPath.Replace('\\', '/') + '/', "")
                where !file.EndsWith(".manifest", StringComparison.Ordinal) && !Array.Exists(ignoredFiles, s => s.Equals(file))
                where !buildVersions.ContainsKey(file)
                select t).ToList();

            foreach (var delete in deletes)
            {
                if (!File.Exists(delete))
                    continue;
                File.Delete(delete);
                File.Delete(delete + ".manifest");
            }

            deletes.Clear();
        }

        private static string GetBuildTargetName(BuildTarget target)
        {
            var name = "Letters" + "_" + GetChannelName() + "_" + PlayerSettings.bundleVersion;
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (target)
            {
                case BuildTarget.Android:
                    return "/" + name + ".apk";

                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "/" + name + ".exe";

#if UNITY_2017_3_OR_NEWER
                case BuildTarget.StandaloneOSX:
                    return "/" + name + ".app";
#else
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
                case BuildTarget.StandaloneOSXUniversal:
                    return "/" + name + ".app";
#endif

                case BuildTarget.WebGL:
                case BuildTarget.iOS:
                    return "";
                // Add more build targets for your own.
                default:
                    Debug.Log("Target not implemented.");
                    return null;
            }
        }

        private static T GetAsset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
            }

            return asset;
        }

        public static Settings GetSettings()
        {
            return GetAsset<Settings>(Utility.AssetsSettingAsset);
        }

        public static AssetsManifest GetManifest()
        {
            return GetAsset<AssetsManifest>(Utility.AssetsManifestAsset);
        }

        public static string GetServerURL()
        {
            string downloadURL;
            if (string.IsNullOrEmpty(overloadedDevelopmentServerURL) == false)
            {
                downloadURL = overloadedDevelopmentServerURL;
            }
            else
            {
                IPHostEntry host;
                string localIP = "";
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }

                downloadURL = "http://" + localIP + ":7888/";
            }

            return downloadURL;
        }

        public static void SaveAppAndResVersions()
        {
            var app_path = outputPath + "/" + Utility.app_versions;
            app_path = FileUtility.FormatToUnityPath(app_path);

            string content = GetManifest().appVersion + "|" + GetManifest().resVersion;
            Debug.Log("save app_version.bytes->" + content);

            FileUtility.SafeWriteAllText(app_path, content);
        }

        public static void ReadAppAndResVersions(ref string app_version, ref string res_version)
        {
            var app_path = outputPath + "/" + Utility.app_versions;
            app_path = FileUtility.FormatToUnityPath(app_path);
            app_version = "0.0.0";
            res_version = "0.0.0";

            string content = FileUtility.SafeReadAllText(app_path);
            if (content == null)
                return;

            var arr = content.Split('|');
            if (arr.Length == 2)
            {
                app_version = arr[0];
                res_version = arr[1];
            }
        }

        public static void SaveCDNFlushFile(List<string> updates)
        {
            string rootpath = Application.dataPath.Replace("Assets", Utility.AssetBundles);
            string flush_name = GetPlatformName() + DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss");
            string flush_path = string.Format("{0}/flush/{1}.flush", rootpath, flush_name);
            StringBuilder sb = new StringBuilder();

            foreach (var item in updates)
            {
                sb.AppendLine(string.Format("{0}/{1}/{2}/{3}/{4}", Utility.AssetBundles, GetChannelName(), GetPlatformName(),
                    GetManifest().resVersion, item));
            }

            Debug.Log("save to flush_path:" + flush_path + "\n" + sb.ToString());
            FileUtility.SafeWriteAllText(flush_path, sb.ToString());
        }
    }
}