using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using XAsset;

namespace ETModel
{
    public class ErrorData
    {
        public string Uid;
        public string device;
        public string idfa;
        public string touristId;
        public string platform;
        public string channel;
        public string appVersion;
        public string resVersion;
        public string log;
        public string logType;

        public string resVerstion;
    }

    public class LoggerHelper :MonoSingleton<LoggerHelper>
    {
        private WebClient m_webClient = new WebClient();

        private List<ErrorData> m_errorList = new List<ErrorData>();
        private bool m_isInit = false;
        private int counter = 0;
        private bool m_canTakeError = true;

        public static string loginUid = string.Empty;
        public static string platName = string.Empty;
        public static string channelName = string.Empty;
        public static string appVersion = string.Empty;
        public static string resVersion = string.Empty;
        public static string deviceId = string.Empty;
        public static string idfa = string.Empty;
        public static string touristId = string.Empty;

        protected override void Init()
        {
            if (Application.isEditor)
            {
                return;
            }

            Application.logMessageReceived += LogHandler;

            platName = Utility.GetPlatform();
            deviceId = Utility.DeviceModel();
            // idfa = NativeBridgeManager.Instance.GetDeviceID();

            //关闭error上报定时器
            InvokeRepeating("CheckReportError", 1f, 1f);
        }

        private void LogHandler(string condition, string stackTrace, LogType type)
        {
            if (Application.isEditor)
            {
                return;
            }

            AddLog(condition + " \n" + stackTrace, type);
        }

        public void AddLog(string message,LogType type)
        {
            if (!string.IsNullOrEmpty(message))
            {
                message = message.Replace("\n", "\\n").Replace("\r", "").Replace("\"", "\\\"");

                if (ExitError(message))
                    return;

                ErrorData error = new ErrorData();
                error.Uid = loginUid;
                error.idfa = idfa;
                error.touristId = touristId;
                error.device = deviceId;
                error.appVersion=appVersion;
                error.resVersion=resVersion;
                error.platform = platName;
                error.channel = channelName;
                error.log = message;
                error.logType = type.ToString();

                m_errorList.Add(error);
            }
        }

        bool ExitError(string errorLog)
        {
            var result= m_errorList.Find((data)=> {
                return data.log == errorLog;
            });
            return result != null;
        }

        private void SendToHttpSvr(string postData)
        {
            if (!string.IsNullOrEmpty(postData))
            {
                //Log.Debug($"<color=red>{postData}</color>");
                if (!m_isInit)
                {
                    m_webClient.UploadStringCompleted += new UploadStringCompletedEventHandler(OnUploadStringCompleted);
                    m_isInit = true;
                }

                m_webClient.UploadStringAsync(new Uri(URLSetting.REPORT_ERROR_URL), "POST", postData);
            }
        }

        public void CheckReportError()
        {
            counter++;

            if (counter % 5 == 0 && m_canTakeError)
            {
                DealWithReportError();
                counter = (counter > 1000) ? 0 : counter;
            }
        }

        private void DealWithReportError()
        {
            int errorCount = m_errorList.Count;
            if (errorCount > 0)
            {
                m_canTakeError = false;
                counter = 0;

                SendToHttpSvr(JsonHelper.ToJson(m_errorList));
                m_errorList.Clear();
            }
        }
        void OnUploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            m_canTakeError = true;
        }

    }
}