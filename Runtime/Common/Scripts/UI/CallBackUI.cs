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
        private static List<object> callBacks = new List<object>();

        public static void RegisterCallback<T>(T callback) where T : class
        {
            if (!callBacks.Contains(callback))
            {
                callBacks.Add(callback);
            }
        }


        public static void UnregisterCallback<T>(T callback) where T : class
        {
            if (callBacks.Contains(callback))
            {
                callBacks.Remove(callback);
            }
        }


        public static void CallAction<T>(Action<T> action) where T : class
        {
            foreach (var callback in callBacks)
            {
                if (callback is T) action(callback as T);
            }
        }

        public static async Task CallTask<T>(Func<T, Task> task) where T : class
        {
            foreach (var callback in callBacks)
            {
                await task(callback as T);
            }
        }

    }
}
