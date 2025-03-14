using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twinny.Helpers;
using Twinny.System;
using UnityEngine;

namespace Twinny.UI
{
    public class CanvasTransition : TSingleton<CanvasTransition>
    {

        [SerializeField] private Canvas _overlayScreen;
        [SerializeField] private CanvasGroup _fadeScreen;


        private void Awake()
        {
            Init();
        }

        // Start is called before the first frame update
        void Start()
        {
            if( _overlayScreen == null ) _overlayScreen = GetComponent<Canvas>();
                _overlayScreen.worldCamera = Camera.main;

        }


        /// <summary>
        /// Fade screen between change scenes
        /// </summary>
        /// <param name="fadeIn">true to show, false to hide.</param>
        /// <param name="callback">bool return: Callback function (true for hided, false for showing)</param>
        public static async Task<bool> FadeScreen(bool fadeIn)
        {
            if(!Instance)
            {
                Debug.LogWarning("[CanvasTransition] Instance not found.");
                return false;
            }
         
            float startAlpha = Instance. _fadeScreen.alpha;
            float targetAlpha = fadeIn ? 1f : 0f;
            float elapsedTime = 0f;


            // Smooth fade progress
            while (elapsedTime < LevelManager.Config.fadeTime)
            {
                Instance._fadeScreen.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / LevelManager.Config.fadeTime);

                elapsedTime += Time.deltaTime;

                await Task.Yield();  
            }

            //Force final alpha result
            Instance._fadeScreen.alpha = targetAlpha;

            return true;

        }


    }
}
