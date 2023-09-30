using System;
using UnityEngine;

namespace Delivery1
{
    public class TaskButtonManager : MonoBehaviour
    {
        public static Action OnThreadActionActivate;
        public static Action OnCoroutineActionActivate;
    
        public void Set60FPS() => Application.targetFrameRate = 60;
        public void UnlockFPS() => Application.targetFrameRate = -1;
        public void DoTaskThreads() => OnThreadActionActivate?.Invoke();
        public void DoTaskCoroutine() => OnCoroutineActionActivate?.Invoke();
    }
}
