//
// ResourcesComponentUpdate.cs
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using XAsset;

namespace ETModel
{
    [ObjectSystem]
    public class AssetsUpdateComponentAwakeSystem : AwakeSystem<AssetsUpdateComponent>
    {
        public override void Awake(AssetsUpdateComponent self)
        {
            self.Awake();
        }
    }

    [ObjectSystem]
    public class AssetsUpdateComponentUpdateSystem : UpdateSystem<AssetsUpdateComponent>
    {
        public override void Update(AssetsUpdateComponent self)
        {
            self.Update();
        }
    }

    public class AssetsUpdateComponent : Entity
    {
        public enum State
        {
            Wait,
            Checking,
            WaitDownload,
            Downloading,
            Completed,
            Error,
        }

        static string APK_FILE_PATH = "/word_letter_{0}.apk";

        public State state;

        public Action completed;

        public Action<string, int, int, float> progress;

        public Action<string> onError;

        private List<Download> needDownloadList = new List<Download>();

        private int downloadIndex;
        private int finishedDownloadCount;

        private ETTaskCompletionSource<bool> tcsb;
        private ETTaskCompletionSource updateTcs;

        private const int RETRY_COUNT= 3;
        private int downloadError = 0;

        private void OnError(string e)
        {
            onError?.Invoke(e);

            Log.Error(e);
            state = State.Error;
        }

        string message = "click Check to start.";
        string Message
        {
            get
            {
                return message;
            }
            set
            {
                if (message != value)
                {
                    message = value;
                    Log.Warning(message);
                }
            }
        }

        void OnProgress(string url, int finishCount, int totalCount, float progress)
        {
            //percent = downloadIndex / (float)needDownloadList.Count;
            //Message = string.Format("{0:F0}%:{1}({2}/{3})", progress * 100, url, finishCount, totalCount);

            //计算真实显示进度
            float progressSlice = 1.0f / totalCount;
            float progressValue = finishCount * progressSlice;
            progressValue += (progressSlice * progress);

            Message = string.Format("{0:F0}%:({1}/{2})", progressValue * 100, finishCount, totalCount);
            EventCenter.Dispatch(EventID.UI_LAUNCH_PROGRESS, Message, progressValue);
        }

        public void Awake()
        {
            //热更模式
            if (Utility.assetBundleMode)
                state = State.Wait;
            else
                state = State.Completed;
        }

        public override void Dispose()
        {
            base.Dispose();
            downloadIndex = 0;
            needDownloadList.Clear();
        }

        void Clear()
        {
            var dir = Path.GetDirectoryName(Utility.updatePath);
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }

            downloadIndex = 0;
            needDownloadList.Clear();

            Message = "Clear AssetsUpdate";
            state = State.Wait;

            Versions.Clear();
        }

        public async ETTask<bool> CheckUpdateOrDownloadGame()
        {
            //for (int i = 0; i < RETRY_COUNT; i++)
            //{
            //    if (await Versions.DownloadServerAppVersion())
            //    {
            //        break;
            //    }
            //    await NoticeTipHelper.WaitForNoticeTip("Tips", "Network error. Failed to download version files, please check your internet and retry.", "Confirm", null);
            //    await TimerComponent.Instance.WaitAsync(100);
            //}

            bool needDownloadGame = Utility.CheckIsNewVersion(Versions.clientAppVersion, Versions.serverAppVersion);
            bool needUpdateGame = Utility.CheckIsNewVersion(Versions.clientResVersion, Versions.serverResVersion);

            if (Versions.forceType == 1 && needDownloadGame)
            {
                Log.Debug("need download app");
                await NoticeTipHelper.WaitForNoticeTip(Language.Tips_Title, Language.Assets_NeedDownload, Language.Confirm_BtnText, null);
                await DownloadGame();

                return false;
            }

            bool sameAppVersion = Utility.CheckIsSameVersion(Versions.clientAppVersion, Versions.serverAppVersion);
            if(sameAppVersion && needUpdateGame)
            {
                Log.Debug("need update resources");
                if(await CheckUpdateAsync())
                {
                    if (Versions.hasUpdate)
                    {
                        await StartUpdate(true);
                    }
                    else
                    {
                        await UpdateFinish();
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        async ETTask StartUpdate(bool isfirst = false)
        {
            if(isfirst)
            {
                int downloadSize = GetDownloadSize();
                downloadError = 0;
                if (!Application.isMobilePlatform|| Utility.IsDebugMode() || downloadSize > 5 * 1024)
                {
                    await NoticeTipHelper.WaitForNoticeTip(Language.Tips_Title, string.Format(Language.Assets_UpdateSize, Utility.KBSizeToString(downloadSize)), Language.Confirm_BtnText, null);
                }
            }

            await DonwloadResAsync(isfirst);
                        
            if (state == State.Error)
            {
                bool cancel = false;
                downloadError++;
                Log.Warning("download failed，retry times " + downloadError);
                if(downloadError>=3)
                {
                    await NoticeTipHelper.WaitForNoticeTip(Language.Tips_Title, Language.Assets_DownloadBundleError, 
                        Language.Confirm_BtnText, Language.Cancel_BtnText, 
                        () => { downloadError = 0; }, 
                        () => { cancel = true; });
                }

                if(cancel)
                {
                    return;
                }
                await TimerComponent.Instance.WaitAsync(200);
                await StartUpdate();
            }
            else
            {
                await UpdateFinish();
            }
        }

        async ETTask<bool> CheckUpdateAsync()
        {
            Log.Debug("start check update ...");
            progress += OnProgress;
            state = State.Checking;
            bool result = false;

            await Versions.LoadClientVersionHash();
            Versions.LoadTempVersionHash();
            for (int i = 0; i < RETRY_COUNT; i++)
            {
                if (await Versions.LoadServerVersionHash())
                {
                    result = true;
                    break;
                }
                //await NoticeTipHelper.WaitForNoticeTip(Language.Tips_Title, Language.Assets_DownloadVersionError, Language.Confirm_BtnText, null);
                await TimerComponent.Instance.WaitAsync(100);
            }

            needDownloadList = Versions.CheckUpdate();

            Versions.hasUpdate = needDownloadList.Count > 0;
            return result;
        }

        //游戏下载
        async ETTask DownloadGame()
        {
            await TimerComponent.Instance.WaitAsync(1);

            if (Application.internetReachability != NetworkReachability.ReachableViaLocalAreaNetwork)
            {
                await NoticeTipHelper.WaitForNoticeTip(Language.Tips_Title, Language.Assets_NonWifiAlert, Language.Confirm_BtnText, null);
            }

            Application.OpenURL(URLSetting.APP_DOWNLOAD_URL);

            //递归提醒游戏更新
            await TimerComponent.Instance.WaitAsync(1000);
            await NoticeTipHelper.WaitForNoticeTip(Language.Tips_Title, Language.Assets_DownLoadGame, Language.Confirm_BtnText, null);
            await DownloadGame();

            //#elif UNITY_IPHONE
            //ChannelManager.Instance.StartDownloadGame(URLSetting.APP_DOWNLOAD_URL);
            //#endif
        }

#if UNITY_ANDROID
        //下载android APK
        void DownloadGameForAndroid()
        {
            EventCenter.Dispatch(EventID.UI_LAUNCH_PROGRESS, "Downloading...", 0f);

            string saveName = string.Format(APK_FILE_PATH, Versions.serverAppVersion);
            Log.Debug(string.Format("Download game : {0}", saveName));
            ChannelManager.Instance.StartDownloadGame(URLSetting.APP_DOWNLOAD_URL, DownloadGameSuccess, DownloadGameFail, (int progress) =>{ Log.Debug("progress: " + progress); }, saveName);
        }

        void DownloadGameSuccess()
        {
            NoticeTipHelper.ShowOneButtonTip("Tips", "The game is downloaded, install now?", "Confirm", () =>
            {
                ChannelManager.Instance.InstallGame(DownloadGameSuccess, DownloadGameFail);
            });
        }

        void DownloadGameFail()
        {
            NoticeTipHelper.ShowOneButtonTip("Tips", "Game Download Failed！", "Retry", () =>
            {
                DownloadGameForAndroid();
            });
        }
#endif

        async ETTask DonwloadResAsync(bool isfirst = false)
        {
            if (needDownloadList.Count > 0)
            {
                if(isfirst)
                {
                    downloadIndex = 0;
                }
                needDownloadList[downloadIndex].Start();
                state = State.Downloading;
            }

            await GameUtility.WaitUntil(() => {
                return state != State.Downloading;
            });
        }

        private void OnCompleted()
        {
            if (updateTcs != null)
                updateTcs.SetResult();
        }

        public void Update()
        {
            if (state == State.Downloading)
            {
                if (downloadIndex < needDownloadList.Count)
                {
                    var download = needDownloadList[downloadIndex];
                    download.Update();
                    progress?.Invoke(download.url, downloadIndex, needDownloadList.Count, download.progress);

                    if (download.isDone)
                    {
                        if (string.IsNullOrEmpty(download.error))
                        {
                            Versions.SaveTempFiles(needDownloadList.GetRange(0, downloadIndex));
                            downloadIndex = downloadIndex + 1;
                            if (downloadIndex == needDownloadList.Count)
                            {
                                state = State.Completed;
                            }
                            else
                            {
                                needDownloadList[downloadIndex].Start();
                            }
                        }
                        else
                        {
                            OnError(download.error);
                            needDownloadList[downloadIndex].Reset();
                        }
                    }
                }
            }
        }

        private async ETTask UpdateFinish()
        {
            updateTcs = new ETTaskCompletionSource();

            Versions.SaveResVersion();
            if (needDownloadList.Count > 0)
            {
                CopyTemp2Data();
                Versions.SaveAllFiles();
                
                Message = string.Format("{0} files has update.", needDownloadList.Count);
                EventCenter.Dispatch(EventID.UI_LAUNCH_PROGRESS, "complete...", 1f);

                ResourcesComponent.CleanUp();
                ResourcesComponent.Initialize(OnCompleted, OnError);
            }
            else
            {
                Message = "nothing to update...";
                OnCompleted();
            }

            await updateTcs.Task;
        }

        void CopyTemp2Data()
        {
            var files=FileUtility.GetAllFilesInFolder(Utility.TempPath);
            if (files.Length > 0)
            {
                foreach (string fromfile in files)
                {
                    if (!fromfile.EndsWith(Versions.versionFile))
                    {
                        string tofile = fromfile.Replace(Utility.TempFolderName, Utility.GetPlatform()); 
                        FileUtility.SafeCopyFile(fromfile,tofile);
                        FileUtility.SafeDeleteFile(fromfile);
                    }
                }
                
                FileUtility.SafeDeleteDir(Utility.TempPath);
            }
        }
        
        public int GetDownloadSize()
        {
            int downloadSize = 0;
            foreach (var downloader in needDownloadList)
            {
                int size = 0;
                if (!Versions.GetSize(downloader.path, out size))
                {
                    Log.Debug("no assetbundle size info : " + downloader.path);
                }
                downloadSize += size;
            }

            return downloadSize;
        }

       
    }
}