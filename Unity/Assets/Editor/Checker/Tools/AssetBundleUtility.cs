using UnityEngine;
using System.IO;
using XAsset;

/// <summary>
/// 功能： Assetbundle相关的通用静态函数，提供运行时，或者Editor中使用到的有关Assetbundle操作和路径处理的函数
/// TODO：
/// 1、做路径处理时是否考虑引入BetterStringBuilder消除GC问题
/// 2、目前所有路径处理不支持variant，后续考虑是否支持
/// </summary>

namespace AssetBundles
{
    public class AssetBundleUtility
    {
        public static string AssetBundlePathToAssetBundleName(string assetPath)
        {
            if (!string.IsNullOrEmpty(assetPath))
            {
                if (assetPath.StartsWith("Assets/"))
                {
                    assetPath = AssetsPathToPackagePath(assetPath);
                }
                //no " "
                assetPath = assetPath.Replace(" ", "");
                //there should not be any '.' in the assetbundle name
                //otherwise the variant handling in client may go wrong
                assetPath = assetPath.Replace(".", "_");
                //add after suffix ".assetbundle" to the end
                assetPath = assetPath + Utility.AssetBundleSuffix;
                return assetPath.ToLower();
            }
            return null;
        }

        public static string PackagePathToAssetsPath(string assetPath)
        {
            return "Assets/" + Utility.AssetsFolderName + "/" + assetPath;
        }

        public static bool IsPackagePath(string assetPath)
        {
            string path = "Assets/" + Utility.AssetsFolderName + "/";
            return assetPath.StartsWith(path);
        }

        public static string AssetsPathToPackagePath(string assetPath)
        {
            string path = "Assets/" + Utility.AssetsFolderName + "/";
            if (assetPath.StartsWith(path))
            {
                return assetPath.Substring(path.Length);
            }
            else
            {
                Debug.LogError("Asset path is not a package path!");
                return assetPath;
            }
        }

        static public bool CheckMaybeAssetBundleAsset(string assetPath)
        {
            return assetPath.StartsWith("Assets/" + Utility.AssetsFolderName);
        }

        static public string AssetPathToDatabasePath(string assetPath)
        {
            if (!CheckMaybeAssetBundleAsset(assetPath))
            {
                return null;
            }

            assetPath = assetPath.Replace("Assets/", "");
            return Path.Combine(Utility.DatabaseRoot, assetPath + ".asset");
        }
    }
}