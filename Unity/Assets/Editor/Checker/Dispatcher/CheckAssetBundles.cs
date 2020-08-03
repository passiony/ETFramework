using UnityEngine;
using UnityEditor;
using ETEditor;

/// <summary>
/// 功能：打包前的AB检测工作
/// </summary>
namespace AssetBundles
{
    public static class CheckAssetBundles
    {
        public const string DatabaseRoot = "Assets/Editor/AssetBundle/Database";

        public static void ClearAllAssetBundles()
        {
            var assebundleNames = AssetDatabase.GetAllAssetBundleNames();
            var length = assebundleNames.Length;
            var count = 0;
            foreach (var assetbundleName in assebundleNames)
            {
                count++;
                EditorUtility.DisplayProgressBar("Remove assetbundle name :", assetbundleName, (float)count / length);
                AssetDatabase.RemoveAssetBundleName(assetbundleName, true);
            }
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();

            assebundleNames = AssetDatabase.GetAllAssetBundleNames();
            if (assebundleNames.Length != 0)
            {
                Debug.LogError("Something wrong!!!");
            }
        }

        public static void RunAllCheckers()
        {
            var guids = AssetDatabase.FindAssets("t:AssetBundleDispatcherConfig", new string[] { DatabaseRoot });
            var length = guids.Length;
            var count = 0;
            foreach (var guid in guids)
            {
                count++;
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var config = AssetDatabase.LoadAssetAtPath<AssetBundleDispatcherConfig>(assetPath);
                config.Load();
                EditorUtility.DisplayProgressBar("Run checker :", config.PackagePath, (float)count / length);
                AssetBundleDispatcher.Run(config);
            }

            BuildScript.BuildManifest();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        public static void Run()
        {
            ClearAllAssetBundles();
            RunAllCheckers();
        }
    }
}