using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;

namespace Delivery1
{
    public class AsyncTaskManager : MonoBehaviour
    {
        public string url = "https://speed.hetzner.de/1GB.bin";
        public string filePath = @"Assets/Downloads/";
        private Coroutine _taskCoroutine = null;
        private Thread _taskThread = null;
        public static Action<bool> OnCoroutineUpdate;
        public static Action<bool> OnThreadUpdate;
        private void OnEnable()
        {
            TaskButtonManager.OnCoroutineActionActivate += OnCoroutineStartRequest;
            TaskButtonManager.OnThreadActionActivate += OnThreadStartRequest;
        }

        private void OnDisable()
        {
            TaskButtonManager.OnCoroutineActionActivate -= OnCoroutineStartRequest;
            TaskButtonManager.OnThreadActionActivate -= OnThreadStartRequest;
        }

        private static void DownloadFileFromUrl(string url, string localFilePath)
        {
            var webClient = new WebClient();
            webClient.DownloadFile(url, localFilePath);
        }
        private void OnDestroy()
        {
            if(_taskThread is { IsAlive: true }) _taskThread.Abort();
        }
        void OnCoroutineStartRequest()
        {
            _taskCoroutine ??= StartCoroutine(DownloadFileCoroutine());
            OnCoroutineUpdate?.Invoke(true);
        }

        void OnThreadStartRequest()
        {
            if (_taskThread != null)
                return;
            _taskThread = new Thread(DownloadFileThread);
            _taskThread.Start();
            OnThreadUpdate?.Invoke(true);
        }

        void DownloadFileThread()
        {
            if(File.Exists($"{filePath}ThreadFile.bin"))
                File.Delete($"{filePath}ThreadFile.bin");
            DownloadFileFromUrl(url, $@"{filePath}ThreadFile.bin");
            OnThreadUpdate?.Invoke(false);
            _taskThread.Join();
            _taskThread = null;
        }
        IEnumerator DownloadFileCoroutine()
        {
            if(File.Exists($"{filePath}CoroutineFile.bin"))
                File.Delete($"{filePath}CoroutineFile.bin");
        
            OnCoroutineUpdate?.Invoke(true);
            DownloadFileFromUrl(url, $@"{filePath}CoroutineFile.bin");
            yield return new WaitForEndOfFrame(); 
            _taskCoroutine = null;
            OnCoroutineUpdate?.Invoke(false);
        }
    }
}
