#if FUSION2
using Fusion;
using Twinny.System.Network;
#endif
using System;
using System.Collections;
using Twinny.Helpers;
using Twinny.Localization;
using Twinny.System;
using UnityEngine;

namespace Twinny.UI
{


    /// <summary>
    /// This is a singleton class to control H.U.D Anchor elements
    /// </summary>
    public class HUDManager : MonoBehaviour, IUICallBacks
    {

        #region Fields

        [Header("COMPONENTS")]

        [Space]
        [Tooltip("H.U.D Estática")]
        [SerializeField] private GameObject _staticHud;

        [Space]
        [Tooltip("H.U.D Dinâmica segue o F.O.V")]
        [SerializeField] private GameObject _dynamicHud;
        [SerializeField] private Transform _canvasRoot;
        [SerializeField] private GameObject _configMenu;
        [SerializeField] private GameObject _mainMenu;
        [SerializeField] private Transform _mainMenu2;
        private GameObject _extensionMenu;
        [Space]
        [Tooltip("Ângulo limite de visão até iniciar rotação.")]
        [SerializeField] private float _dynamicTreesholdAngle = 30f;
        [Tooltip("Velocidade de rotação da HUD.")]
        [Range(0.1f, 1f)]
        [SerializeField] private float _dynamicRotationSpeed = .5f;

        [Space]
        [Tooltip("H.U.D Dinâmica de Navegação")]
        [SerializeField] private GameObject _navigationHud;


        private bool _isFollowing;


        private Coroutine _fadeCoroutine;
        private Coroutine _fadeDynamicCoroutine;
        private Coroutine _fadeNavCoroutine;
        private Vector3 _previousCameraPos;

        #endregion
        #region Delegates


        private Transform _mainCameraTransform;
        #endregion

        #region MonoBehaviour Methods

        //Awake is called before the script is started
        private void Awake()
        {

        }

        // Start is called before the first frame update
        void Start()
        {
#if OCULUS
            AnchorManager.OnAnchorStateChanged += OnAnchorStateChanged;
#endif
            CallBackUI.RegisterCallback(this);

            _mainCameraTransform = Camera.main.transform;
            _previousCameraPos = _mainCameraTransform.position;
            _mainMenu.SetActive(false);

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
#if OCULUS
            AnchorManager.OnAnchorStateChanged -= OnAnchorStateChanged;
#endif
            CallBackUI.UnregisterCallback(this);
        }
        #endregion

        #region Public Methods



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

        /*
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
                        Debug.LogWarning(element+"|"+item.key);
                        if (element == item.key)
                        {

                            item.element.SetActive(true);
                        }
                    }
            }
        }
        */


        /// <summary>
        /// This method insert a new loaded scene menus
        /// </summary>
        /// <param name="menu">Menu UI object</param>
        /// <param name="isStatic">Default:false</param>
        private void LoadExtensionMenu(UnityEngine.GameObject menu, bool isStatic = false)
        {


            if (_extensionMenu != null)
            {
                Destroy(_extensionMenu.gameObject);
            }

            if (menu != null)
            {
                _extensionMenu = Instantiate(menu, isStatic ? _staticHud.transform : _canvasRoot);

#if FUSION2                
                _extensionMenu.SetActive(NetworkedLevelManager.IsManager);
#endif 
                HudElement he = new HudElement();
                he.key = menu.name;
                he.element = _extensionMenu;
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

#if FUSION2
        public void OnSwitchManager(PlayerRef source)
        {
            Debug.LogWarning("Switch Manager chamado no HUDManager!");

            if (source == NetworkRunnerHandler.runner.LocalPlayer)
                ActionManager.CallDelayedAction(() =>
                {

                    _extensionMenu.SetActive(true);
                    if (SceneFeature.Instance.enableNavigationMenu)
                        NavigationMenu.Instance?.SetArrows(SceneFeature.Instance?.landMarks[NetworkedLevelManager.instance.currentLandMark].node);

                }, 500);


        }

        public void OnExperienceStarting(PlayerRef source)
        {

            _mainMenu.SetActive(false);

        }

        public void OnExperienceStarted(PlayerRef source)
        {
            Debug.LogWarning("OnExperienceStarted:" + source);
            Debug.LogWarning("OnExperienceStarted:" + _mainMenu);
            _mainMenu.SetActive(true);
        }
#endif

        public void OnLoadExtensionMenu(GameObject menu)
        {
            LoadExtensionMenu(menu);

        }

        public void OnStartLoadScene()
        {

        }


        public void OnHideHud(bool status)
        {
            HideHud(status);
        }

        public void OnLoadSceneFeature()
        {

            ActionManager.CallDelayedAction(() =>
            {
                //TODO Make inactive and fadeout H.U.D

                bool active = true;
#if FUSION2
                active = NetworkedLevelManager.IsManager;
#endif

                if (_extensionMenu) _extensionMenu.SetActive(active);
                else
                    _mainMenu?.SetActive(true);

                _navigationHud.SetActive(SceneFeature.Instance.enableNavigationMenu);
                _dynamicHud.SetActive(true);

            }, 500);
        }

        public void OnUnloadSceneFeature()
        {

        }

        public void OnLoadScene()
        {
        }

#if OCULUS
        public void OnAnchorStateChanged(StateAnchorManager state)
        {
            Debug.LogWarning("OnAnchorStateChanged");

            bool isActive = state == StateAnchorManager.DISABLED || state == StateAnchorManager.ANCHORED;
            //TODO Make inactive and fadeout H.U.D
            if (_extensionMenu) _extensionMenu.SetActive(isActive);
            else _mainMenu?.SetActive(isActive);

            _navigationHud?.SetActive(isActive && SceneFeature.Instance && SceneFeature.Instance.enableNavigationMenu);

        }
#endif
        public void OnPlatformInitialize()
        {
            AlertViewHUD.PostMessage(LocalizationProvider.GetTranslated("%CONNECTING_MESSAGE"), AlertViewHUD.MessageType.Warning, 90);
        }

        public void OnExperienceReady()
        {
            AlertViewHUD.CancelMessage();
            _mainMenu.SetActive(true);
        }

        public void OnExperienceFinished(bool isRunning)
        {
            _dynamicHud?.SetActive(isRunning);
            _mainMenu?.SetActive(isRunning);
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