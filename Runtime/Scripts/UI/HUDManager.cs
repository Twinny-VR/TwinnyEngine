using Meta.WitAi.Json;
using Oculus.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Twinny.Helpers;
using Twinny.System;
using UnityEngine;
using static OVRHaptics;

namespace Twinny.UI
{
    [Serializable]
    public enum HUDType
    {
        STATIC,
        FOLLOW_USER
    }

    /// <summary>
    /// This is a singleton class to control H.U.D Anchor elements
    /// </summary>
    public class HUDManager : TSingleton<HUDManager>
    {

        #region Fields

        [Header("COMPONENTS")]

        [SerializeField] private Canvas _overlayScreen;
        [SerializeField] private CanvasGroup _fadeScreen;
        [Space]
        [Tooltip("H.U.D Dinâmica segue o F.O.V")]
        [SerializeField] private GameObject _dynamicHud;
        [SerializeField] private Transform _canvasRoot;

        [Tooltip("Ângulo limite de visão até iniciar rotação.")]
        [SerializeField] private float _dynamicTreesholdAngle = 30f;
        [Tooltip("Velocidade de rotação da HUD.")]
        [Range(0.1f, 1f)]
        [SerializeField] private float _dynamicRotationSpeed = .5f;
        
        [Space]
        [SerializeField] private GameObject _navigationHud;


        private bool _isFollowing;
        [SerializeField]
        private List<HudElement> _hudElements = new List<HudElement>();

        public bool allowClickSafeAreaOutside = false;
        [Space]
        [Tooltip("H.U.D Estática")]
        [SerializeField] private GameObject _staticHud;
        private Coroutine _fadeCoroutine;
        private Coroutine _fadeDynamicCoroutine;
        private Coroutine _fadeNavCoroutine;
        private GameObject _extensionMenu;

        private Vector3 _previousCameraPos;
        #endregion
        #region Delegates


        private Transform _mainCameraTransform;
        #endregion

        #region MonoBehaviour Methods

        //Awake is called before the script is started
        private void Awake()
        {
            Init();
        }

        // Start is called before the first frame update
        void Start()
        {

            _overlayScreen.worldCamera = Camera.main;
            FadeScreen(true);
            _mainCameraTransform = Camera.main.transform;
            _previousCameraPos = _mainCameraTransform.position;
            }
       
        // Update is called once per frame
        void Update()
        {
            if (_dynamicHud) FollowUser(_dynamicHud.transform, _dynamicRotationSpeed, _dynamicTreesholdAngle);
            if (_navigationHud) FollowUser(_navigationHud.transform, 0);

            float distance = Vector3.Distance(_previousCameraPos, _mainCameraTransform.position);
            distance = Mathf.Round(distance * 100f) / 100f;
            if (distance > .1f)
            {
                _previousCameraPos = _mainCameraTransform.position;
                FadeHud(true);
            }
            else
                FadeHud(false);
            //if (_configMenu != null) FollowUser2(_configMenu.transform, 0.5f);
        }

        private void OnDisable()
        {

        }
        #endregion

        #region Public Methods



        /// <summary>
        /// Fade screen between change scenes
        /// </summary>
        /// <param name="fade">true hides, false shows</param>
        /// <param name="callback">bool return: Callback function (true for hided, false for showing)</param>
        public void FadeScreen(bool fade, Action<bool> callback = null)
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

            _fadeCoroutine = StartCoroutine(FadeCanvas(_fadeScreen, fade ? 0 : 1, 0, .5f, callback));
        }


        public void FadeHud(bool status, float limit = 0f)
        {
            //TODO Criar sistema de esmaecer controles
           // if (status) Debug.Log("MOVIMENTANDO");
        }


        public void HideHud(bool status, float limit = 0f)
        {
            _canvasRoot.gameObject.SetActive(!status);
            //TODO Criar sistema de esmaecer controles
            // if (status) Debug.Log("MOVIMENTANDO");
        }


        /// <summary>
        /// Set all handeables UI elements active.
        /// </summary>
        /// <param name="elements">Array of elements that only will be activated.</param>
        public void SetElementActive(string[] elements = null)
        {

            foreach (HudElement item in _hudElements)
            {
                item.element.SetActive(false);
                if (elements != null)
                    foreach (var element in elements)
                    {
                        if (element == item.key)
                        {

                            item.element.SetActive(true);
                        }
                    }
            }
        }



        /// <summary>
        /// This method insert a new loaded scene menus
        /// </summary>
        /// <param name="menu">Menu UI object</param>
        /// <param name="isStatic">Default:false</param>
        public void LoadExtensionMenu(GameObject menu, bool isStatic = false)
        {
            if (_extensionMenu != null)
            {
                _hudElements.RemoveAll(item => item.element == _extensionMenu);
                Destroy(_extensionMenu.gameObject);
            }

            if (menu != null)
            {
                _extensionMenu = Instantiate(menu, isStatic ? _staticHud.transform : _canvasRoot);
                _extensionMenu.SetActive(false);
                HudElement he = new HudElement();
                he.key = menu.name;
                he.element = _extensionMenu;
                _hudElements.Add(he);
            }
        }


        #endregion


        #region Private Methods

        /// <summary>
        /// This method makes HUD follow the user orbit.
        /// </summary>
        /// <param name="tracer">Tracer object.</param>
        /// <param name="speed">Trajectory velocity.</param>
        /// <param name="treesholdAngle">F.O.V Angle Limit until following.</param>
        private void FollowUser(Transform tracer, float speed, float treesholdAngle = 0f)
        {
            if (speed > 0)
            {

            Quaternion targetRotation = Quaternion.Euler(0f, _mainCameraTransform.eulerAngles.y, 0f);

            float angleDifference = Quaternion.Angle(tracer.rotation, targetRotation);

            if (angleDifference > treesholdAngle || _isFollowing)
            {

                tracer.rotation = Quaternion.Slerp(tracer.rotation, targetRotation, (10f * speed) * Time.deltaTime);
                //Debug.Log(angleDifference);
                _isFollowing = (angleDifference > 1f);
            }
            } 
            Vector3 desiredPosition = _mainCameraTransform.position;
            desiredPosition.y = 0f;

            tracer.position = desiredPosition;
        }


        /// <summary>
        /// Search in all handeables UI elements in scene.
        /// </summary>
        /// <param name="key">key to search</param>
        /// <returns>Especific UI element</returns>
        private HudElement GetHudElement(string key)
        {
            return _hudElements.FirstOrDefault(item => item.key == key);
        }



        #endregion

        #region Coroutines

        /// <summary>
        /// This coroutine fades the canvas element smoothly
        /// </summary>
        /// <param name="canvas">Canvas Object</param>
        /// <param name="targetAlpha">Desired alpha value</param>
        /// <param name="delay">Delay before start motion</param>
        /// <param name="duration">Motion time</param>
        /// <param name="callback">bool return: Callback function (true for hided, false for showing)</param>
        /// <returns></returns>
        private IEnumerator FadeCanvas(CanvasGroup canvas, float targetAlpha, float delay, float duration, Action<bool> callback = null)
        {
            float startAlpha = canvas.alpha;
            float elapsedTime = 0f;

            yield return new WaitForSeconds(delay);

            // Smooth fade progress
            while (elapsedTime < duration)
            {
                canvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);

                elapsedTime += Time.deltaTime;

                yield return null;  // Espera o próximo frame
            }

            //Force final alpha result
            canvas.alpha = targetAlpha;

            //Send callback to caller function
            if (callback != null)
            {
                callback((canvas.alpha == 0));
            }
        }

        #endregion

        #region UI Buttons Actions

        #endregion

        #region CallBack Methods

        private void OnSafeAreaEntered2(bool status)
        {

        }

        #endregion
    }


    [Serializable]
    public class HudElement
    {
        public string key;
        public GameObject element;

    }
}