//
// AssetDownload.cs
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
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace XAsset
{
    public class Download
    {
        public enum State
        {
            HeadRequest,
            BodyRequest,
            FinishRequest,
            Completed,
        }

        public float progress { get; private set; }

        public bool isDone { get; private set; }

        public string error = null;

        public long maxlen { get; private set; }

        public long len { get; private set; }

        public int index { get; private set; }

        public string url { get; set; }

        public string path { get; set; }

        public string savePath { get; set; }

        public string version { get; set; }

        public State state { get; private set; }

        private UnityWebRequest request { get; set; }

        void WriteBuffer()
        {
            var buff = request.downloadHandler.data;
            if (buff != null)
            {
                var length = buff.Length - index;
                index += length;
                len += length;
                progress = len / (float)maxlen;
            }
        }

        public void Update()
        {
            if (isDone)
            {
                return;
            }

            switch (state)
            {
                case State.HeadRequest:
                    if (request.error != null)
                    {
                        error = string.Format("download->url:\"{0}\";\n Error:{1}", url, request.error);
                        isDone = true;
                        return;
                    }

                    if (request.isDone)
                    {
                        maxlen = long.Parse(request.GetResponseHeader("Content-Length"));
                        request.Dispose();
                        request = null;
                        var dir = Path.GetDirectoryName(savePath);
                        if (!Directory.Exists(dir))
                        {
                            // ReSharper disable once AssignNullToNotNullAttribute
                            Directory.CreateDirectory(dir);
                        }
                        //fs = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.Write);
                        //len = fs.Length;
                        //var emptyVersion = string.IsNullOrEmpty(version);
                        //var oldVersion = Versions.Get(savePath);
                        //var emptyOldVersion = string.IsNullOrEmpty(oldVersion);
                        //if (emptyVersion || emptyOldVersion || !oldVersion.Equals(version))
                        //{
                        //    Versions.Set(savePath, version);
                        //    len = 0;
                        //}

                        //if (len < maxlen)
                        //{
                        len = 0;
                        //fs.Seek(len, SeekOrigin.Begin);
                        request = UnityWebRequest.Get(url);
                        request.SetRequestHeader("Range", "bytes=" + len + "-");
#if UNITY_2017_1_OR_NEWER
                        request.SendWebRequest();
#else
                        request.Send();
#endif
                        index = 0;
                        state = State.BodyRequest;
                        //}
                        //else
                        //{
                        //    state = State.FinishRequest;
                        //}
                    }
                    break;
                case State.BodyRequest:
                    if (request.error != null)
                    {
                        isDone = true;
                        error = string.Format("download->url:\"{0}\";\n Error:{1}", url, request.error);
                        return;
                    }

                    WriteBuffer();

                    if (request.isDone)
                    {
                        if (len != maxlen)
                        {
                            error = string.Format("download->url:\"{0}\";\n Error: missing bytes", url);
                            isDone = true;
                            return;
                        }
                        else
                        {
                            
                            state = State.FinishRequest;
                            Save();

                            request.Dispose();
                        }
                    }
                    break;
                case State.FinishRequest:
                    isDone = true;
                    state = State.Completed;
                    break;
            }
        }

        public void Start()
        {
            request = UnityWebRequest.Head(url);
#if UNITY_2017_1_OR_NEWER
            request.SendWebRequest();
#else
            request.Send();
#endif
            progress = 0;
            isDone = false;

            Log(url);
        }

        [Conditional("LOGGER_ON")]
        private static void Log(string s)
        {
            ETModel.Log.Debug("[download]{0}", s);
        }

        public void Stop()
        {
            isDone = true;
        }

        public void Reset()
        {
            isDone = false;
            progress = 0;
            error = null;
            len = 0;
            index = 0;
            state = State.HeadRequest;

            request?.Dispose();
            request = null;
        }

        public void Save()
        {
            FileUtility.SafeWriteAllBytes(savePath,request.downloadHandler.data);
        }
    }
}