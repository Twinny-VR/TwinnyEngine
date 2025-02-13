using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twinny.UI;
using UnityEngine;

namespace Twinny.System
{
    public static class CallBackUI
    {
        private static List<IUICallBacks> callBacks = new List<IUICallBacks>();

        public static void RegisterCallback(IUICallBacks callback)
        {
            if (!callBacks.Contains(callback))
            {
                callBacks.Add(callback);
            }
        }


        public static void UnregisterCallback(IUICallBacks callback)
        {
            if (callBacks.Contains(callback))
            {
                callBacks.Remove(callback);
            }
        }


        public static void CallAction(Action<IUICallBacks> action)
        {
            foreach (var callback in callBacks)
            {
                action(callback);
            }
        }

        public static async Task CallTask(Func<IUICallBacks, Task> task)
        {
            foreach (var callback in callBacks)
            {
                await task(callback);
            }
        }

    }
}
