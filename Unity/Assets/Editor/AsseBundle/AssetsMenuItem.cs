//
// AssetsMenuItem.cs
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

using System.IO;
using UnityEditor;
using UnityEngine;
using XAsset;

namespace ETEditor
{
    public static class AssetsMenuItem
    {
        private const string KMarkAssets = "标记资源";

        private const string KMarkAssetsWithDir = "AssetBundles/按目录标记";
        private const string KMarkAssetsWithFile = "AssetBundles/按文件标记";
        private const string KMarkAssetsWithName = "AssetBundles/按名称标记";

        private const string KRunCheckMarkFile = "AssetBundles/Run Checker";
        private const string KBuildManifest = "AssetBundles/生成Manifest配置";
        private const string KBuildAssetBundles = "AssetBundles/Build AssetBundles";

        private const string KCopyToStreamingAssets = "AssetBundles/拷贝到StreamingAssets";

        private const string KOpenOutputAssets = "AssetBundles/打开OutputAssets";
        private const string KOpenPersistentAssets = "AssetBundles/打开PersistentAssets";

        private const string KClearOutputAssets = "AssetBundles/清空OutputAssets";
        private const string KClearPersistentAssets = "AssetBundles/清空PersistentAssets";
        private const string KClearStreamingAssets = "AssetBundles/清空StreamingAssets";

        private const string KCopyPath = "Assets/复制路径";

        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            EditorUtility.ClearProgressBar();
            var settings = BuildScript.GetSettings();
            if (settings.assetbundleMode)
            {
                bool isRunning = LaunchLocalServer.IsRunning();
                if (!isRunning)
                {
                    LaunchLocalServer.Run();
                }
            }
            else
            {
                bool isRunning = LaunchLocalServer.IsRunning();
                if (isRunning)
                {
                    LaunchLocalServer.KillRunningAssetBundleServer();
                }
            }

            Utility.assetBundleMode = settings.assetbundleMode;
            Utility.getPlatformDelegate = BuildScript.GetPlatformName;
            Utility.loadDelegate = AssetDatabase.LoadAssetAtPath;
            Utility.loadAllDelegate = AssetDatabase.LoadAllAssetsAtPath;
        }

        public static string TrimedAssetBundleName(string assetBundleName)
        {
            if (string.IsNullOrEmpty(Utility.AssetRootPath))
                return assetBundleName;
            return assetBundleName.Replace(Utility.AssetRootPath, "");
        }

        //[MenuItem(KMarkAssetsWithDir,false,0)]
        private static void MarkAssetsWithDir()
        {
            var assetsManifest = BuildScript.GetManifest();
            var assets = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
            for (var i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                var path = AssetDatabase.GetAssetPath(asset);
                if (Directory.Exists(path) || path.EndsWith(".cs", System.StringComparison.CurrentCulture))
                    continue;
                if (EditorUtility.DisplayCancelableProgressBar(KMarkAssets, path, i * 1f / assets.Length))
                    break;
                var assetBundleName = TrimedAssetBundleName(Path.GetDirectoryName(path).Replace("\\", "/")) + "_g";
                BuildScript.SetAssetBundleNameAndVariant(path, assetBundleName.ToLower(), null);
            }
            EditorUtility.SetDirty(assetsManifest);
            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
        }

        //[MenuItem(KMarkAssetsWithFile, false, 0)]
        private static void MarkAssetsWithFile()
        {
            var assetsManifest = BuildScript.GetManifest();
            var assets = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
            for (var i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                var path = AssetDatabase.GetAssetPath(asset);
                if (Directory.Exists(path) || path.EndsWith(".cs", System.StringComparison.CurrentCulture))
                    continue;
                if (EditorUtility.DisplayCancelableProgressBar(KMarkAssets, path, i * 1f / assets.Length))
                    break;

                var dir = Path.GetDirectoryName(path);
                var name = Path.GetFileNameWithoutExtension(path);
                if (dir == null)
                    continue;
                dir = dir.Replace("\\", "/") + "/";
                if (name == null)
                    continue;

                var assetBundleName = TrimedAssetBundleName(Path.Combine(dir, name));
                BuildScript.SetAssetBundleNameAndVariant(path, assetBundleName.ToLower(), null);
            }
            EditorUtility.SetDirty(assetsManifest);
            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
        }

        //[MenuItem(KMarkAssetsWithName, false, 0)]
        private static void MarkAssetsWithName()
        {
            var assets = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
            var assetsManifest = BuildScript.GetManifest();
            for (var i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                var path = AssetDatabase.GetAssetPath(asset);
                if (Directory.Exists(path) || path.EndsWith(".cs", System.StringComparison.CurrentCulture))
                    continue;
                if (EditorUtility.DisplayCancelableProgressBar(KMarkAssets, path, i * 1f / assets.Length))
                    break;
                var assetBundleName = Path.GetFileNameWithoutExtension(path);
                BuildScript.SetAssetBundleNameAndVariant(path, assetBundleName.ToLower(), null);
            }
            EditorUtility.SetDirty(assetsManifest);
            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
        }

        [MenuItem(KRunCheckMarkFile, false, 50)]
        public static void RunChecker()
        {
            var buildTargetName = BuildScript.GetPlatformName();
            var channelName = BuildScript.GetChannelName();
            bool checkCopy = EditorUtility.DisplayDialog("Run Checkers Warning",
                string.Format("Run Checkers for : \n\nplatform : {0} \n\nchannel : {1} \n\nContinue ?", buildTargetName, channelName),
                "Confirm", "Cancel");
            if (!checkCopy)
            {
                return;
            }

            var start = System.DateTime.Now;
            AssetBundles.CheckAssetBundles.Run();
            Debug.Log("Finished CheckAssetBundles.Run! use " + (System.DateTime.Now - start).TotalSeconds + "s");
        }

        [MenuItem(KBuildAssetBundles, false, 100)]
        public static void BuildAssetBundles()
        {
            BuildAssetBundles(true);
        }

        public static void BuildAssetBundles(bool isShowDialog)
        {
            var buildTargetName = BuildScript.GetPlatformName();
            var channelName = BuildScript.GetChannelName();
            if (isShowDialog)
            {
                bool checkCopy = EditorUtility.DisplayDialog("Build AssetBundles Warning",
                    string.Format("Build AssetBundles for : \n\nplatform : {0} \n\nchannel : {1} \n\nContinue ?", buildTargetName, channelName),
                    "Confirm", "Cancel");
                if (!checkCopy)
                {
                    return;
                }
            }

            var start = System.DateTime.Now;

            BuildScript.BuildAssetBundles();
            AwscliGeneratorEditor.GenerateAwsCliFile();
            if (isShowDialog)
            {
                EditorUtility.DisplayDialog("Success", string.Format(
                    "Build AssetBundles for : \n\nplatform : {0} \n\nchannel : {1} \n\ndone! use {2}s",
                    buildTargetName, channelName, (System.DateTime.Now - start).TotalSeconds), "Confirm");
            }
        }

        [MenuItem(KCopyPath)]
        public static void CopyPath()
        {
            var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            EditorGUIUtility.systemCopyBuffer = assetPath;
            Debug.Log(assetPath);
        }

        [MenuItem(KCopyToStreamingAssets, false, 200)]
        public static void CopyAssetBundles()
        {
            CopyAssetBundles(true);
        }

        public static void CopyAssetBundles(bool isShowDialog)
        {
            var buildTargetName = BuildScript.GetPlatformName();
            var channelName = BuildScript.GetChannelName();
            if (isShowDialog)
            {
                bool checkCopy = EditorUtility.DisplayDialog("Copy AssetBundles To StreamingAssets Warning",
                    string.Format("Copy AssetBundles to streamingAssets folder for : \n\nplatform : {0} \n\nchannel : {1}\n\nContinue ?",
                        buildTargetName, channelName),
                    "Confirm", "Cancel");
                if (!checkCopy)
                {
                    return;
                }
            }

            // 拷贝到StreamingAssets目录时，相当于执行大版本更新，那么沙盒目录下的数据就作废了
            // 真机上会对比这两个目录下的App版本号来删除，编辑器下暴力一点，直接删除
            ClearPersistentAssets(isShowDialog);

            BuildScript.CopyAssetBundlesTo(Path.Combine(Application.streamingAssetsPath, Utility.AssetBundles));

            // 删除指定的文件夹
            // BookShopResVersionHelper.DeleteExtraBookShopResFiles();

            AssetDatabase.Refresh();
            Debug.Log("Copy channel assetbundles to streaming assets done!");
        }

        [MenuItem(KOpenOutputAssets, false, 300)]
        public static void OpenOutputAssetPath()
        {
            string path = OutputPath;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            
            EditorUtility.RevealInFinder(path);
        }

        [MenuItem(KOpenPersistentAssets, false, 300)]
        private static void OpenPersistentAssets()
        {
            string path = Application.persistentDataPath;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            EditorUtility.RevealInFinder(path);
        }

        [MenuItem(KClearOutputAssets, false, 400)]
        private static void ClearOutputAssetsAssets()
        {
            var buildTargetName = BuildScript.GetPlatformName();
            var channelName = BuildScript.GetChannelName();
            bool checkClear = EditorUtility.DisplayDialog("ClearOutput Warning",
                string.Format("Clear output assetbundles will force to rebuild all : \n\nplatform : {0}  \n\nchannel : {1}\n\ncontinue ?", buildTargetName, channelName),
                "Yes", "No");
            if (!checkClear)
            {
                return;
            }
            string path = OutputPath;
            FileUtility.SafeClearDir(path);
            Debug.Log(string.Format("Clear done : {0}", path));
        }

        [MenuItem(KClearPersistentAssets, false, 400)]
        private static void ClearPersistentAssets()
        { 
            ClearPersistentAssets(true);
        }

        private static void ClearPersistentAssets(bool isShowDialog)
        {
            if (isShowDialog)
            {
                bool checkClear = EditorUtility.DisplayDialog("ClearPersistentAssets Warning",
                    "Clear persistent assetbundles will force to update all assetbundles that difference with streaming assets assetbundles, continue ?",
                    "Yes", "No");
                if (!checkClear)
                {
                    return;
                }
            }

            string outputPath = Path.Combine(Application.persistentDataPath, Utility.AssetBundles);
            FileUtility.SafeClearDir(outputPath);
            Debug.Log(string.Format("Clear {0} assetbundle persistent assets done!", Utility.GetPlatform()));
        }

        [MenuItem(KClearStreamingAssets, false, 400)]
        private static void ClearStreamingAssets()
        {
            bool checkClear = EditorUtility.DisplayDialog("ClearStreamingAssets Warning",
                "Clear streaming assets assetbundles will lost the latest player build info, continue ?",
                "Yes", "No");
            if (!checkClear)
            {
                return;
            }
            string outputPath = Path.Combine(Application.streamingAssetsPath, Utility.AssetBundles);
            FileUtility.SafeClearDir(outputPath);
            AssetDatabase.Refresh();
            Debug.Log(string.Format("Clear {0} assetbundle streaming assets done!", outputPath));
        }

        public static string OutputPath
        {
            get
            {
                return Path.Combine(Application.dataPath.Replace("Assets", Utility.AssetBundles), BuildScript.GetChannelName(), Utility.GetPlatform());
            }
        }
    }
}