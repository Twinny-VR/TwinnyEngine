using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twinny.UI;
using UnityEngine;

namespace Twinny.System
{
    public static class CallBackManager
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
            foreach (var callback in callBacks.ToArray()) // 👈 agora é seguro
            {
                if (callback == null || (callback is UnityEngine.Object obj && obj == null))
                {
                    callBacks.Remove(callback);
                    continue;
                }

                if (callback is T typed)
                {
                    action(typed);
                }
            }
        }

        public static async Task CallTask<T>(Func<T, Task> task) where T : class
        {
            foreach (var callback in callBacks.ToArray())
            {
                if (callback == null || (callback is UnityEngine.Object obj && obj == null))
                {
                    callBacks.Remove(callback);
                    continue;
                }

                if (callback is T typed)
                {
                    await task(typed);
                }
            }
        }

    }
}
