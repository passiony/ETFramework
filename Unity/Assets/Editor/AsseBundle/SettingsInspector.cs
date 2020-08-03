using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ETEditor
{
    [CustomEditor(typeof (Settings))]
    public class SettingsInspector: Editor
    {
        public enum BuildSettingType
        {
            AndroidDev,
            IosDev,
            IosPrd,
        }

        private static string DEVELOP_SYMBOLS = "DEVELOPMENT";
        private static string ENCRYPT_SYMBOLS = "ENCRYPT";
        private static string LOGGERON_SYMBOLS = "LOGGER_ON";
        private static string ILRUNTIME_SYMBOLS = "ILRuntime";

        private static readonly List<string> TotalDefine = new List<string>()
        {
            DEVELOP_SYMBOLS,
            ENCRYPT_SYMBOLS,
            LOGGERON_SYMBOLS,
            ILRUNTIME_SYMBOLS,
        };

        private static readonly List<string> AndroidDevDefine = new List<string>()
        {
            DEVELOP_SYMBOLS,
            ENCRYPT_SYMBOLS,
            LOGGERON_SYMBOLS,
            ILRUNTIME_SYMBOLS,
        };

        private static readonly List<string> IosDevDefine = new List<string>()
        {
            ENCRYPT_SYMBOLS,
            LOGGERON_SYMBOLS,
            ILRUNTIME_SYMBOLS,
        };

        private static readonly List<string> IosPrdDefine = new List<string>()
        {
            ENCRYPT_SYMBOLS,
            ILRUNTIME_SYMBOLS,
        };

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            GUILayout.Label("----AssetBundle Model----");
            GUILayout.Space(3);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("assetbundleMode"), true);

            GUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("developeMode"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("encryptMode"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("loggerOn"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ilruntimeMode"), true);
            serializedObject.ApplyModifiedProperties();
            
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply", GUILayout.Width(200)))
            {
                CompileSymbols(this.target);
            }

            GUILayout.EndHorizontal();
        }

        public static void CompileSymbols(object target)
        {
            BuildTargetGroup targetGroup = BuildScript.GetActiveTargetGroup();
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

            List<string> defineSymbols = new List<string>(symbols.Split(';'));
            Settings self = target as Settings;

            //debug
            if (self.developeMode)
            {
                defineSymbols.Add(DEVELOP_SYMBOLS);
            }
            else
            {
                defineSymbols.Remove(DEVELOP_SYMBOLS);
            }

            //encrypt
            if (self.encryptMode)
            {
                defineSymbols.Add(ENCRYPT_SYMBOLS);
            }
            else
            {
                defineSymbols.Remove(ENCRYPT_SYMBOLS);
            }

            //logger
            if (self.loggerOn)
            {
                defineSymbols.Add(LOGGERON_SYMBOLS);
            }
            else
            {
                defineSymbols.Remove(LOGGERON_SYMBOLS);
            }

            //ilruntime
            if (self.ilruntimeMode)
            {
                defineSymbols.Add(ILRUNTIME_SYMBOLS);
            }
            else
            {
                defineSymbols.Remove(ILRUNTIME_SYMBOLS);
            }

            HashSet<string> symbolSet = new HashSet<string>();
            foreach (var symbol in defineSymbols)
            {
                symbolSet.Add(symbol);
            }

            string result = string.Join(";", symbolSet.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, result);
            Debug.LogWarning("宏定义刷新成功:" + result);
        }

        public static bool CheckDefine(BuildSettingType buildSettingType)
        {
            List<string> needDefine = null;
            switch (buildSettingType)
            {
                case BuildSettingType.AndroidDev:
                    needDefine = AndroidDevDefine;
                    break;
                case BuildSettingType.IosDev:
                    needDefine = IosDevDefine;
                    break;
                case BuildSettingType.IosPrd:
                    needDefine = IosPrdDefine;
                    break;
                default: break;
            }

            if (needDefine == null)
            {
                throw new Exception("打包类型不存在");
            }

            BuildTargetGroup currentBuildTargetGroup = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android?
                    BuildTargetGroup.Android : BuildTargetGroup.iOS;

            var notExitDefine = TotalDefine.Except(needDefine);
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentBuildTargetGroup);
            List<string> defineSymbols = new List<string>(symbols.Split(';'));
            foreach (string defineTemp in needDefine)
            {
                if (!defineSymbols.Contains(defineTemp))
                {
                    return false;
                }
            }

            foreach (string defineTemp in notExitDefine)
            {
                if (defineSymbols.Contains(defineTemp))
                {
                    return false;
                }
            }

            return true;
        }

        public static void AddDefine(BuildSettingType buildSettingType)
        {
            List<string> needDefine = new List<string>();
            BuildTargetGroup currentBuildTargetGroup = BuildTargetGroup.Android;
            switch (buildSettingType)
            {
                case BuildSettingType.AndroidDev:
                    needDefine = AndroidDevDefine;
                    currentBuildTargetGroup = BuildTargetGroup.Android;
                    break;
                case BuildSettingType.IosDev:
                    needDefine = IosDevDefine;
                    currentBuildTargetGroup = BuildTargetGroup.iOS;
                    break;
                case BuildSettingType.IosPrd:
                    needDefine = IosPrdDefine;
                    currentBuildTargetGroup = BuildTargetGroup.iOS;
                    break;
            }

            string result = string.Join(";", needDefine.ToArray());
            var setting = BuildScript.GetSettings();

            setting.developeMode = result.Contains(DEVELOP_SYMBOLS);
            setting.encryptMode = result.Contains(ENCRYPT_SYMBOLS);
            setting.loggerOn = result.Contains(LOGGERON_SYMBOLS);
            setting.ilruntimeMode = result.Contains(ILRUNTIME_SYMBOLS);

            PlayerSettings.SetScriptingDefineSymbolsForGroup(currentBuildTargetGroup, result);
        }
    }
}