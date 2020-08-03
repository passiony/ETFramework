using System.IO;
using UnityEditor;
using UnityEngine;
using XAsset;
using ChannelType = XAsset.ChannelType;

namespace ETEditor
{
    /// <summary>
    /// Package 打包工具窗口
    /// </summary>
    public partial class PackageTool: EditorWindow
    {
        private static BuildTarget buildTarget = BuildTarget.Android;
        private static ChannelType channelType = ChannelType.Test;
        private static ServerMode serverMode = ServerMode.Local;

        private static string appVersion = "1.0.000";
        private static string resVersion = "1.0.000";
        private static string localServerUrl = "127.0.0.1";

        private static string terminalPath;
        private static string buildPath;

        public static string TerminalPath
        {
            get
            {
                if (string.IsNullOrEmpty(terminalPath))
                {
                    initPath();
                }
                return terminalPath;
            }
        }

        public static string BuildAppPath
        {
            get
            {
                if (string.IsNullOrEmpty(buildPath))
                {
                    initPath();
                }
                return buildPath;
            }
        }
        
        private static void initPath()
        {
            string DefaultTerminalPath = "/System/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal";
            string DefaultBuildPath = "../Release";
            terminalPath = EditorPrefs.GetString(PackageUtils.TerminalPrefs,DefaultTerminalPath);
            buildPath = EditorPrefs.GetString(PackageUtils.BuildAppPrefs,DefaultBuildPath);
        }
        
        [MenuItem("Tools/Package", false, 0)]
        static void Init()
        {
            GetWindow(typeof (PackageTool));
        }

        void OnEnable()
        {
            var manifest = BuildScript.GetManifest();

            buildTarget = EditorUserBuildSettings.activeBuildTarget;
            channelType = manifest.channelType;

            appVersion = manifest.appVersion;
            resVersion = manifest.resVersion;

            serverMode = manifest.serverMode;
            localServerUrl = manifest.downloadURL;

            initPath();
        }

        void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            buildTarget = (BuildTarget) EditorGUILayout.EnumPopup("Build Target : ", buildTarget);
            var settings = BuildScript.GetSettings();
            settings.assetbundleMode = GUILayout.Toggle(settings.assetbundleMode, "Assetbundle Mode");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            bool buildTargetSupport = false;
            if (buildTarget != BuildTarget.Android && buildTarget != BuildTarget.iOS)
            {
                GUILayout.Label("Error : Only android or iOS build target supported!!!");
            }
            else
            {
                buildTargetSupport = true;
            }

            GUILayout.EndHorizontal();

            if (buildTargetSupport)
            {
                if (GUI.changed)
                {
                    PackageUtils.SaveCurSelectedChannel(channelType);
                }

                DrawEditorPath(buildTarget);
                DrawConfigGUI();
                DrawAssetBundlesGUI();
                DrawAWSCLIGUI();
                DrawDefineSymbolsGUI();
                DrawILRuntimeGUI();
                DrawBuildPlayerGUI();
                DrawOneKeyAutoBuildGUI();
            }
        }

        #region 资源配置GUI

        void DrawEditorPath(BuildTarget buildTarget)
        {
            GUILayout.BeginHorizontal();
            buildPath = EditorGUILayout.TextField("Build Path", buildPath);
            if (GUILayout.Button("...",GUILayout.Width(50)))
            {
                string path = EditorUtility.OpenFolderPanel("Finder", buildPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    string root = Directory.GetCurrentDirectory();
                    root = FileUtility.FormatToUnityPath(root.Remove(root.Length-5));
                    
                    path = path.Replace(root, "../");
                    buildPath = path;
                    
                    EditorPrefs.SetString(PackageUtils.BuildAppPrefs,buildPath);
                }
            }
            GUILayout.EndHorizontal();
            
            if (buildTarget != BuildTarget.iOS)
                return;
            
            GUILayout.BeginHorizontal();
            terminalPath = EditorGUILayout.TextField("Terminal Path", terminalPath);
            if (GUILayout.Button("...",GUILayout.Width(50)))
            {
                string path = EditorUtility.OpenFolderPanel("Finder", terminalPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    terminalPath = path;
                    EditorPrefs.SetString(PackageUtils.TerminalPrefs,terminalPath);
                }
            }
            GUILayout.EndHorizontal();
        }

        void DrawConfigGUI()
        {
            GUILayout.Space(3);
            GUILayout.Label("-------------[Config]-------------");
            GUILayout.Space(3);

            GUILayout.BeginHorizontal();
            GUILayout.Label("app_version：", GUILayout.Width(100));
            string curBundleVersion = GUILayout.TextField(appVersion, GUILayout.Width(100));
            if (curBundleVersion != appVersion)
            {
                appVersion = curBundleVersion;
                BuildScript.GetManifest().appVersion = appVersion;
                PlayerSettings.bundleVersion = curBundleVersion;
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(3);

            GUILayout.BeginHorizontal();
            GUILayout.Label("res_version：", GUILayout.Width(100));
            string curResVersion = GUILayout.TextField(resVersion, GUILayout.Width(100));
            if (curResVersion != resVersion)
            {
                resVersion = curResVersion;
                BuildScript.GetManifest().resVersion = resVersion;
            }

            GUILayout.Label("Auto build will Auto increase sub version", GUILayout.Width(500));
            GUILayout.EndHorizontal();

            var curChannelType = (ChannelType) EditorGUILayout.EnumPopup("Build Channel : ", channelType);
            if (curChannelType != channelType)
            {
                channelType = curChannelType;
                BuildScript.GetManifest().channelType = channelType;
            }
            
            var curServerMode = (ServerMode) EditorGUILayout.EnumPopup("Server Mode : ", serverMode);
            if (curServerMode != serverMode)
            {
                serverMode = curServerMode;
                BuildScript.GetManifest().serverMode = serverMode;
            }

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Version From Server", GUILayout.Width(200)))
            {
                EditorApplication.delayCall += () =>
                {
                    StartUpVersionHelper.LoadVersionFile(() => { resVersion = BuildScript.GetManifest().resVersion; });
                };
            }

            if (GUILayout.Button("Save Version To Server", GUILayout.Width(200)))
            {
                EditorApplication.delayCall += () => { StartUpVersionHelper.SaveVersionFile(); };
            }

            if (GUILayout.Button("Open Version URL", GUILayout.Width(200)))
            {
                PackageUtils.OpenVersionUrl();
            }

            GUILayout.EndHorizontal();
        }

        #endregion

        #region Bundle打包GUI

        void DrawAssetBundlesGUI()
        {
            GUILayout.Space(5);
            GUILayout.Label("-------------[Build AssetBundles]-------------");
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Execute Build", GUILayout.Width(200)))
            {
                EditorApplication.delayCall += AssetsMenuItem.BuildAssetBundles;
            }

            if (GUILayout.Button("Copy To StreamingAsset", GUILayout.Width(200)))
            {
                EditorApplication.delayCall += AssetsMenuItem.CopyAssetBundles;
            }

            if (GUILayout.Button("Run All Checkers", GUILayout.Width(200)))
            {
                EditorApplication.delayCall += AssetsMenuItem.RunChecker;
            }

            GUILayout.EndHorizontal();
        }

        #endregion

        #region AWS命令GUI

        void DrawAWSCLIGUI()
        {
            GUILayout.Space(5);
            GUILayout.Label("-------------[AWS CLI]-------------");
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Upload To Aws Server", GUILayout.Width(200)))
            {
                AwscliGeneratorEditor.ExcuteUpload();
            }

            if (GUILayout.Button("RefreshAllCDN", GUILayout.Width(200)))
            {
                PackageUtils.RefreshAllCDN(channelType);
            }

            GUILayout.EndHorizontal();
        }

        #endregion

        #region 宏定义GUI

        void DrawDefineSymbolsGUI()
        {
            GUILayout.Space(5);
            GUILayout.Label("-------------[Define Symbols]-------------");
            GUILayout.Space(5);

            var settings = BuildScript.GetSettings();
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            settings.developeMode = GUILayout.Toggle(settings.developeMode, "Develope");
            settings.encryptMode = GUILayout.Toggle(settings.encryptMode, "Encrypt");
            settings.loggerOn = GUILayout.Toggle(settings.loggerOn, "LoggerOn");
            settings.ilruntimeMode = GUILayout.Toggle(settings.ilruntimeMode, "ILRuntime");
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.Space(5);

            if (GUILayout.Button("Apply", GUILayout.Width(200)))
            {
                EditorUtility.SetDirty(settings);
                SettingsInspector.CompileSymbols(settings);
            }
        }

        #endregion

        #region ILRuntime

        void DrawILRuntimeGUI()
        {
            var settings = BuildScript.GetSettings();
            if (settings.ilruntimeMode)
            {
                GUILayout.Space(5);
                GUILayout.Label("-------------[ILRuntime]-------------");
                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Generate ILBinding", GUILayout.Width(200)))
                {
#if ILRuntime
                    EditorApplication.delayCall += ILRuntimeCLRBinding.GenerateCLRBindingByAnalysis;
#endif
                }

                if (GUILayout.Button("Clear ILBinding", GUILayout.Width(200)))
                {
#if ILRuntime
                    EditorApplication.delayCall += ILRuntimeCLRBinding.ClearCLRBinding;
#endif
                }

                GUILayout.EndHorizontal();
            }
        }

        #endregion

        #region app打包GUI

        void DrawBuildPlayerGUI()
        {
            if (buildTarget == BuildTarget.Android)
            {
                DrawBuildAndroidPlayerGUI();
            } 
        }

        void DrawBuildAndroidPlayerGUI()
        {
            GUILayout.Space(5);
            GUILayout.Label("-------------[Build APP]-------------");
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Execute Build", GUILayout.Width(200)))
            {
                EditorApplication.delayCall += () => { BuildScript.BuildApp(); };
            }

            if (GUILayout.Button("Open Output Folder", GUILayout.Width(200)))
            {
                PackageUtils.OpenAppOutputPath();
            }

            // if (GUILayout.Button("ADB Install", GUILayout.Width(200)))
            // {
            //     if (!string.IsNullOrEmpty(currentBuildAPKName))
            //     {
            //         PackageUtils.ADBInstallApp(currentBuildAPKName);
            //     }
            //     else
            //     {
            //         Debug.LogWarning("did not build apk");
            //     }
            // }

            GUILayout.EndHorizontal();
        } 

        #endregion
  }
}