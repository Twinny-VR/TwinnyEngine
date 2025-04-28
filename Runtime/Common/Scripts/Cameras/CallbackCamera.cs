using System;
using System.Collections;
using System.Collections.Generic;
using Twinny.System.Cameras;
using Twinny.UI;
using UnityEngine;

namespace Twinny.System
{
    public static class CallBackCamera
    {
        private static List<ICameraCallBacks> callBacks = new List<ICameraCallBacks>();

        public static void RegisterCallback(ICameraCallBacks callback)
        {
            if (!callBacks.Contains(callback))
            {
                callBacks.Add(callback);
            }
        }


        public static void UnregisterCallback(ICameraCallBacks callback)
        {
            if (callBacks.Contains(callback))
            {
                callBacks.Remove(callback);
            }
        }

        public static void CallAction(Action<ICameraCallBacks> action)
        {
            foreach (var callback in callBacks)
            {
                action(callback);
            }
        }
    }
}
