using System.Runtime.InteropServices;
using UnityEngine;

namespace CoalPulverizer
{
    public static class JSBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        public static extern void NotifyPartSelected(string partId);
#else
        public static void NotifyPartSelected(string partId)
        {
            Debug.Log($"[JSBridge] NotifyPartSelected({partId})");
        }
#endif
    }
}
