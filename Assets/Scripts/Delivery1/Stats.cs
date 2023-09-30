using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Delivery1
{
    public class Stats : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _cubeCountText;
        private const string CubeCountDisplay = "Moving cubes: {0}";
        [SerializeField] private TextMeshProUGUI _coroutineStatusText;
        private const string CoroutineStatusDisplay = "Corutine active: {0}";
        [SerializeField] private TextMeshProUGUI _threadStatusText;
        private const string ThreadStatusDisplay = "Thread active: {0}";

        [SerializeField] private GameObject _downloadingCoroutine;
        [SerializeField] private GameObject _downloadingThread;

        private void OnEnable()
        {
            CubeSpawner.OnCubeAdded += UpdateCubeCounter;
            AsyncTaskManager.OnCoroutineUpdate += UpdateCoroutineStatus;
            AsyncTaskManager.OnThreadUpdate += UpdateThreadStatus;
        }
            

        private void OnDisable()
        {
            CubeSpawner.OnCubeAdded -= UpdateCubeCounter;
            AsyncTaskManager.OnCoroutineUpdate -= UpdateCoroutineStatus;
            AsyncTaskManager.OnThreadUpdate -= UpdateThreadStatus;
        }

        private void Start()
        {
            UpdateCoroutineStatus(false);
            UpdateThreadStatus(false);
        }



        void UpdateCubeCounter(int count) =>
            _cubeCountText.text = string.Format(CubeCountDisplay, count);

        void UpdateCoroutineStatus(bool status)
        {
            _downloadingCoroutine.SetActive(status);
            _coroutineStatusText.text = string.Format(CoroutineStatusDisplay, status);
        }

        void UpdateThreadStatus(bool status)
        {
            _downloadingThread.SetActive(status);
            _threadStatusText.text = string.Format(ThreadStatusDisplay, status);
        }
    }
}
