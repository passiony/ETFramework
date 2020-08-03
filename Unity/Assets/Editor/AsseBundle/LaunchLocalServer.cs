using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ETEditor
{
    internal class LaunchLocalServer : ScriptableSingleton<LaunchLocalServer>
    {
        [SerializeField] int m_ServerPID = 0;

        public static bool IsRunning()
        {
            if (instance.m_ServerPID == 0)
                return false;

            try
            {
                var process = Process.GetProcessById(instance.m_ServerPID);
                return !process.HasExited;
            }
            catch
            {
                return false;
            }
        }

        public static void KillRunningAssetBundleServer()
        {
            // Kill the last time we ran
            Debug.Log("Kill Assets Server");
            try
            {
                if (instance.m_ServerPID == 0)
                    return;

                var lastProcess = Process.GetProcessById(instance.m_ServerPID);
                lastProcess.Kill();
                instance.m_ServerPID = 0;
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
        }

        public static void Run()
        {
            string pathToAssetServer = Path.GetFullPath("Assets/Editor/XAsset/AssetBundleServer.exe");
            string assetBundlesDirectory = Path.Combine(Environment.CurrentDirectory, "AssetBundles");

            KillRunningAssetBundleServer();

            Debug.Log("Run Assets Server:");
            BuildScript.CreateAssetBundleDirectory();

            string args = assetBundlesDirectory;
            args = string.Format("\"{0}\" {1}", args, Process.GetCurrentProcess().Id);
            ProcessStartInfo startInfo = ExecuteInternalMono.GetProfileStartInfoForMono(
                MonoInstallationFinder.GetMonoInstallation("MonoBleedingEdge"), GetMonoProfileVersion(),
                pathToAssetServer, args, true);
            
            startInfo.WorkingDirectory = assetBundlesDirectory;
            startInfo.UseShellExecute = false;
            startInfo.WindowStyle = ProcessWindowStyle.Normal;

            Process launchProcess = Process.Start(startInfo);
            launchProcess.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                Debug.LogWarning(e.Data);
            };
            launchProcess.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                Debug.LogError(e.Data);
            };

            if (launchProcess == null || launchProcess.HasExited == true || launchProcess.Id == 0)
            {
                //Unable to start process
                Debug.LogError("Unable Start AssetBundleServer process");
            }
            else
            {
                //We seem to have launched, let's save the PID
                instance.m_ServerPID = launchProcess.Id;
            }
        }

        static string GetMonoProfileVersion()
        {
            return "3.5";
        }
    }
}