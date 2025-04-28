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


        protected override void Start()
        {
            base.Start();
            if( _overlayScreen == null ) _overlayScreen = GetComponent<Canvas>();
                _overlayScreen.worldCamera = Camera.main;

            _fadeScreen.alpha = 1.0f;

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
            float fadeTime = 0;
            fadeTime = TwinnyManager.config.fadeTime;
            // Smooth fade progress
            while (elapsedTime < fadeTime)
            {
                Instance._fadeScreen.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeTime);

                elapsedTime += Time.deltaTime;
                await Task.Yield();  
            }

            //Force final alpha result
            Instance._fadeScreen.alpha = targetAlpha;

            return true;

        }


    }
}
