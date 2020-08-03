using UnityEditor;
using UnityEngine;

namespace ETEditor
{
    public enum AutoBuildType
    {
        App_245679,
        App_12345689,
        Res_23468,
        Res_2467,
    }

    public partial class PackageTool
    {
        private void DrawOneKeyAutoBuildGUI()
        {
            GUILayout.Space(5);
            GUILayout.Label("-------------[Auto Build]-------------");
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Label("1.Add Local App Version", GUILayout.Width(200));
            GUILayout.Label("2.Add Local Res Version", GUILayout.Width(200));
            GUILayout.Label("3.Save AWS Info", GUILayout.Width(200));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("4.Build Bundles", GUILayout.Width(200));
            GUILayout.Label("5.Copy To Streaming", GUILayout.Width(200));
            GUILayout.Label("6.Upload To AWS", GUILayout.Width(200));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("7.Alter Server Res Version", GUILayout.Width(200));
            GUILayout.Label("8.Alter ResUrl&Res Version", GUILayout.Width(200));
            GUILayout.Label("9.Build App", GUILayout.Width(200));
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUI.enabled = !EditorApplication.isCompiling;
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.green;
            GUILayout.Label("dev mini version:", GUILayout.Width(150));
            if (GUILayout.Button("Build " + AutoBuildType.App_245679, GUILayout.Width(200)))
            {
                EditorApplication.delayCall += () => { PackageUtils.BuildApp(AutoBuildType.App_245679, callback: RestVersionInfo); };
            }

            if (GUILayout.Button("Build " + AutoBuildType.Res_2467, GUILayout.Width(200)))
            {
                EditorApplication.delayCall += () => { PackageUtils.BuildApp(AutoBuildType.Res_2467, callback: RestVersionInfo); };
            }

            GUI.backgroundColor = Color.gray * 1.8f;

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("dev big version:", GUILayout.Width(150));
            if (GUILayout.Button("Build " + AutoBuildType.App_12345689, GUILayout.Width(200)))
            {
                EditorApplication.delayCall += () => { PackageUtils.BuildApp(AutoBuildType.App_12345689, callback: RestVersionInfo); };
            }

            if (GUILayout.Button("Build " + AutoBuildType.Res_23468, GUILayout.Width(200)))
            {
                EditorApplication.delayCall += () => { PackageUtils.BuildApp(AutoBuildType.Res_23468, callback: RestVersionInfo); };
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.red;
            GUILayout.Label("prd version(only for Ios):", GUILayout.Width(150));
            if (GUILayout.Button("Build" + AutoBuildType.App_12345689, GUILayout.Width(200)))
            {
                if (EditorUtility.DisplayDialog("build", "是否进行正式版本打包", "ok", "cancel"))
                {
                    EditorApplication.delayCall += () => { PackageUtils.BuildApp(AutoBuildType.App_12345689, true, callback: RestVersionInfo); };
                }
            }

            GUI.backgroundColor = Color.gray * 1.8f;
            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }

        private void RestVersionInfo(AutoBuildType autoBuildType)
        {
            var manifest = BuildScript.GetManifest();
            appVersion = manifest.appVersion;
            resVersion = manifest.resVersion;
            EditorUtility.DisplayDialog("success", "build success:" + autoBuildType, "ok");
        }
    }
}