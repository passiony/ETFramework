using XAsset;
using System.IO;
using ETModel;
using UnityEditor;

namespace ETEditor
{
    [InitializeOnLoad]
    public class Startup
    {
        private const string ScriptAssembliesDir = "Library/ScriptAssemblies";
        private const string ReleaseAssembliesDir = "Temp/bin/Release";
        
        private const string CodeDir = "Assets/Bundles/Code/";
        
        private const string HotfixDll = "Unity.Hotfix.dll";
        private const string HotfixPdb = "Unity.Hotfix.pdb";

        static Startup()
        {
            CopyDebugDLLToBundles();
        }

        // [MenuItem("ILRuntime/Copy Debug.DLL To Bundles",false,400)]
        static void CopyDebugDLLToBundles()
        {
            if (!File.Exists(CodeDir))
            {
                Directory.CreateDirectory(CodeDir);
            }
            //File.Copy(Path.Combine(ScriptAssembliesDir, HotfixDll), Path.Combine(CodeDir, "Hotfix.dll.bytes"), true);
            //File.Copy(Path.Combine(ScriptAssembliesDir, HotfixPdb), Path.Combine(CodeDir, "Hotfix.pdb.bytes"), true);

            string dllPath = Path.Combine(CodeDir, "Hotfix.dll.bytes");
            File.Delete(dllPath);
            var dllSource = FileUtility.SafeReadAllBytes(Path.Combine(ScriptAssembliesDir, HotfixDll));
            var dllEncryp = EncryptHelper.EncryptBytes(dllSource);
            FileUtility.SafeWriteAllBytes(dllPath, dllEncryp);

            string pdbPath = Path.Combine(CodeDir, "Hotfix.pdb.bytes");
            File.Delete(pdbPath);
            var pdbSource = FileUtility.SafeReadAllBytes(Path.Combine(ScriptAssembliesDir, HotfixPdb));
            var pdbEncryp = EncryptHelper.EncryptBytes(pdbSource);
            FileUtility.SafeWriteAllBytes(pdbPath, pdbEncryp);

            Log.Debug($"Copy Hotfix.dll From {ScriptAssembliesDir}->{CodeDir};{EncryptString}");
            AssetDatabase.Refresh();
        }
        
        // [MenuItem("ILRuntime/Copy Release.DLL To Bundles",false,400)]
        public static void CopyHotfixToBundles()
        {
            if (!File.Exists(CodeDir))
            {
                Directory.CreateDirectory(CodeDir);
            }

            string dllPath = Path.Combine(CodeDir, "Hotfix.dll.bytes");
            File.Delete(dllPath);
            var dllSource = FileUtility.SafeReadAllBytes(Path.Combine(ReleaseAssembliesDir, HotfixDll));
            var dllEncryp = EncryptHelper.EncryptBytes(dllSource);
            FileUtility.SafeWriteAllBytes(dllPath, dllEncryp);

            
            string pdbPath = Path.Combine(CodeDir, "Hotfix.pdb.bytes");
            File.Delete(pdbPath);
            var pdbSource = FileUtility.SafeReadAllBytes(Path.Combine(ReleaseAssembliesDir, HotfixPdb));
            var pdbEncryp = EncryptHelper.EncryptBytes(pdbSource);
            FileUtility.SafeWriteAllBytes(pdbPath, pdbEncryp);
            
            Log.Debug($"Copy Hotfix.dll From {ReleaseAssembliesDir}->{CodeDir};{EncryptString}");

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("提示", "release code generator success...", "OK");
        }

        static string EncryptString
        {
            get
            {
                return Define.IsEncrypt? "加密" : "";
            }
        }
    }
}