using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using XAsset;
using System.IO;
using ETModel;
using Debug = UnityEngine.Debug;

namespace ETEditor
{
    public class AwscliGeneratorEditor: Editor
    {

        static string platform;
        static string channelName;

        static string remote_url;
        static string remote_flush;

        public static void GenerateAwsCliFile()
        {
            var start = System.DateTime.Now;

            platform = BuildScript.GetPlatformName();
            channelName = BuildScript.GetChannelName();

            remote_url = URLSetting.S3_URL + "/AssetBundles/" + channelName + "/" + platform + "/";
            remote_flush = URLSetting.S3_URL + "/flush";

#if UNITY_EDITOR_WIN
            CreateWinUploadFile();
#else
            CreateMacUploadFile();
#endif
        }

        static string GetOldVersion()
        {
            string version = BuildScript.GetManifest().resVersion;
            var arr = version.Split('.');
            int v3 = int.Parse(arr[2]);
            string oldVersion = string.Format("{0}.{1}.{2:D3}", arr[0], arr[1], (v3 - 1));

            return oldVersion;
        }

        static void CreateWinUploadFile()
        {
            string outputPath = Path.Combine(Application.dataPath.Replace("Assets", Utility.AssetBundles)) + "/";
            string aws_path = outputPath + "win_update_aws.bat";

            string flush_path = outputPath + "flush";
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            string newVersion = BuildScript.GetManifest().resVersion;

            sb.AppendLine(string.Format("aws s3 sync {0} {1} --delete --exclude *.manifest", outputPath + channelName + "/" + platform,
                remote_url + newVersion));
            sb.AppendLine(string.Format("aws s3 sync {0} {1} --exclude *.manifest", flush_path, remote_flush));
            sb.AppendLine("pause");

            Debug.Log("save to aws_path：" + aws_path);
            FileUtility.SafeWriteAllText(aws_path.ToLower(), sb.ToString());
        }

        /// <summary>
        /// 根据web信息编辑当前的win_update_aws.bat信息
        /// </summary>
        /// <param name="data">web相关信息</param>
        public static void CreateWinUploadFileFromWeb(StartUpVersionHelper.UploadData data)
        {
            platform = BuildScript.GetPlatformName();
            channelName = BuildScript.GetChannelName();

            remote_url = URLSetting.S3_URL + "/AssetBundles/" + channelName + "/" + platform + "/";
            remote_flush = URLSetting.S3_URL + "/flush";

            string[] info = data.resUrl.Split('/');
            string uploadFile = info[info.Length - 1];

            string outputPath = Path.Combine(Application.dataPath.Replace("Assets", Utility.AssetBundles)) + "/";
            string aws_path = outputPath + "win_update_aws.bat";

            string flush_path = outputPath + "flush";
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine(string.Format("aws s3 sync {0} {1} --delete --exclude *.manifest", outputPath + channelName + "/" + platform,
                remote_url + uploadFile));
            sb.AppendLine(string.Format("aws s3 sync {0} {1} --exclude *.manifest", flush_path, remote_flush));
            sb.AppendLine("pause");

            FileUtility.SafeWriteAllText(aws_path.ToLower(), sb.ToString());
        }

        static void CreateMacUploadFile()
        {
            string outputPath = Path.Combine(Application.dataPath.Replace("Assets", Utility.AssetBundles)) + "/";
            string aws_path = outputPath + "mac_update_aws.sh";

            string flush_path = outputPath + "flush";
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            string newVersion = BuildScript.GetManifest().resVersion;

            sb.AppendLine("source ~/.bash_profile");
            sb.AppendLine(string.Format("aws s3 sync {0} {1} --delete --exclude *.manifest,*.DS_Store", outputPath + channelName + "/" + platform,
                remote_url + newVersion));
            sb.AppendLine(string.Format("aws s3 sync {0} {1} --exclude *.manifest,*.DS_Store", flush_path, remote_flush));

            Debug.Log("save to aws_path：" + aws_path);
            FileUtility.SafeWriteAllText(aws_path.ToLower(), sb.ToString());
        }

        
        public static void ExcuteUpload()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                string command = PackageTool.TerminalPath; // "/System/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal";

                string rootPath = Directory.GetCurrentDirectory();
                string shell = rootPath + "/AssetBundles/mac_update_aws.sh";

                Process.Start(command, shell);

                Debug.LogWarning("mac update awscli 命令自动执行");
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                string rootPath = Directory.GetCurrentDirectory();
                string bat = rootPath + "/AssetBundles/win_update_aws.bat";

                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = bat;
                info.UseShellExecute = true;
                info.ErrorDialog = true;
                info.WorkingDirectory = rootPath;

                Debug.Log(info.FileName);
                Process.Start(info);
            }
        }
    }
}