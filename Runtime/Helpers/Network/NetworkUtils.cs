using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Concept.Helpers
{


    /// <summary>
    ///  This class is to manage network features.
    /// </summary>
    public static class NetworkUtils
    {
        public static UnityEvent OnInternetConnectedEvent = new UnityEvent();
        public static UnityEvent OnInternetDisconnectedEvent = new UnityEvent();
//        public static OnInternetConnectedEvent OnInternetConnectedEvent = new OnInternetConnectedEvent();
  //      public static OnInternetDisconnectedEvent OnInternetDisconnectedEvent = new OnInternetDisconnectedEvent();



        static NetworkUtils() { CheckInternetConnection(); } //Start to check by initialization

        /// <summary>
        /// Checks internet connection
        /// </summary>
        /// <returns>Has Internet: (true or false)</returns>
        public static bool IsWiFiConnected()
        {

            if (UnityEngine.Application.platform == RuntimePlatform.Android)
            {

                try
                {
                    // Obter o contexto da UnityActivity
                    using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    {
                        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                        // Acessar o ConnectivityManager do Android
                        using (AndroidJavaClass connectivityManagerClass = new AndroidJavaClass("android.net.ConnectivityManager"))
                        {
                            AndroidJavaObject connectivityManager = currentActivity.Call<AndroidJavaObject>("getSystemService", "connectivity");
                            AndroidJavaObject activeNetwork = connectivityManager.Call<AndroidJavaObject>("getActiveNetworkInfo");

                            if (activeNetwork != null)
                            {
                                bool isConnected = activeNetwork.Call<bool>("isConnected");
                                bool isWiFi = activeNetwork.Call<int>("getType") == 1; // 1 significa Wi-Fi
                                return isConnected && isWiFi;
                            }
                            else
                            {
                                Debug.LogWarning("No active network information available.");
                                return false;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Erro ao acessar a conectividade: " + e.Message);
                    return false;
                }
            }
            else
                return UnityEngine.Application.internetReachability != NetworkReachability.NotReachable;
        }


        /// <summary>
        /// This coroutine check every a second if has internet connection
        /// </summary>
        public static async void CheckInternetConnection()
        {
            bool conected = NetworkUtils.IsWiFiConnected();
            while (true)
            {
                //Debug.Log("Teste: "+ conected);
                if (!conected && NetworkUtils.IsWiFiConnected())
                {
                    conected = true;
                    OnInternetConnectedEvent?.Invoke();
                }
                else if (conected && !NetworkUtils.IsWiFiConnected())
                {
                    conected = false;
                    OnInternetDisconnectedEvent?.Invoke();
                }
                await Task.Delay(1000);
            }
        }

        private static async Task<Sprite> LoadImageAsync(string url)
        {
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
            {
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield(); // Liberar o thread principal

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Load Image error: " + request.error);
                    return null;
                }

                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                return Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );
            }
        }

    }
}
