using System;
using System.Collections;
using System.Collections.Generic;
using Twinny.UI;
using UnityEngine;

namespace Twinny.System
{
    public static class CallBackEngine
    {
        private static List<IEngineCallBacks> callBacks = new List<IEngineCallBacks>();

        public static void RegisterCallback(IEngineCallBacks callback)
        {
            if (!callBacks.Contains(callback))
            {
                callBacks.Add(callback);
            }
        }


        public static void UnregisterCallback(IEngineCallBacks callback)
        {
            if (callBacks.Contains(callback))
            {
                callBacks.Remove(callback);
            }
        }

        public static void CallAction(Action<IEngineCallBacks> action)
        {
            foreach (var callback in callBacks)
            {
                action(callback);
            }
        }
    }
}
