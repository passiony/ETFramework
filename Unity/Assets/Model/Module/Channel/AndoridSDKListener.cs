using UnityEngine;

namespace ETModel
{
    public class AndroidSDKListener : MonoSingleton<AndroidSDKListener>
    {
        private void InitCallback(string msg)
        {
            Log.Debug("InitSDKComplete with msg : " + msg);
            ChannelManager.Instance.OnInitSDKCompleted(msg);
        }

        private void DownloadGameCallback(string msg)
        {
            Log.Debug("Download game with msg: " + msg);
            int result = -1;
            int.TryParse(msg, out result);
            ChannelManager.Instance.OnDownloadGameFinished(result == 0);
        }

        private void DownloadGameProgressValueChangeCallback(string msg)
        {
            Log.Debug("Download game progress : " + msg);
            int progress = 0;
            int.TryParse(msg, out progress);
            ChannelManager.Instance.OnDownloadGameProgressValueChange(progress);
        }

        private void InstallApkCallback(string msg)
        {
            Log.Debug("Install apk with msg: " + msg);
            int result = -1;
            int.TryParse(msg, out result);
            ChannelManager.Instance.OnInstallGameFinished(result == 0);
        }

        private void LoginCallback(string msg)
        {
            Log.Debug("Login with msg : " + msg);
            ChannelManager.Instance.OnLogin(msg);
        }

        private void LogoutCallback(string msg)
        {
            Log.Debug("Logout with msg : " + msg);
            ChannelManager.Instance.OnLoginOut(msg);
        }

        private void PayCallback(string msg)
        {
            Log.Debug("SDKPay complete with msg : " + msg);
            ChannelManager.Instance.OnSDKPay(msg);
        }
    }
}