using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Twinny.System;
using UnityEngine;
using UnityEngine.UI;

namespace Twinny.UI
{

    /// <summary>
    /// This Class manages the Radial Menu on UI.
    /// </summary>
    public class RadialMenu : MonoBehaviour
    {

        #region Fields
        private bool _isActive = false;
        [Tooltip("Tempo de vida do menu sem atividade.")]
        [SerializeField] private float _closeInactiveMenuTime = 3f;
        private float _closeTimer = 0f;
        [Header("UI Elements")]
        [SerializeField] private GameObject _mainButton;
        [SerializeField] private GameObject _foldedButton;
        [SerializeField] private GameObject _radialMenu;
        [SerializeField] private GameObject _closeButton;
        [SerializeField] private GameObject _anchorButton;
        [SerializeField] private GameObject _wifiOnIcon;
        [SerializeField] private GameObject _wifiOffIcon;
        [SerializeField] private GameObject _soundOnIcon;
        [SerializeField] private GameObject _soundOffIcon;
        [SerializeField] private GameObject _voiceOnIcon;
        [SerializeField] private GameObject _voiceOffIcon;

        private Action OnClose;
        #endregion



        #region MonoBehaviour Methods
        // Start is called before the first frame update
        void Start()
        {
            //Set CallBacks
            LevelManager.OnInternetConnectionChanged += OnInternetConnectionChanged;
            LevelManager.OnVolumeChanged += OnVolumeChange;
            LevelManager.OnVoipChanged += OnVoipChange;
            AnchorManager.OnAnchorStateChanged += OnAnchorStateChanged;

            //Sets Audio Feed UI Icon
            OnVolumeChange(LevelManager.GetAudioVolume());

            //Sets Voip Feed UI Icon
            OnVoipChange(LevelManager.GetVoipStatus());

            //Sets WiFi Feed UI Icon
            OnInternetConnectionChanged(LevelManager.IsWiFiConnected());
            
            ResizeRadialMenu();
        }

        // Update is called once per frame
        void Update()
        {
            if (_isActive) //Checks if menu is active
            if (_closeTimer > 0f){ _closeTimer -= Time.deltaTime;} //Start countdown
            else
            {
                _closeTimer = 0;
                    ShowRadialMenu(false);
            }

        }

        // OnDestroy is called when component/object was removed
        private void OnDestroy()
        {
            //Unset all callbacks
            LevelManager.OnInternetConnectionChanged -= OnInternetConnectionChanged;
            LevelManager.OnVolumeChanged -= OnVolumeChange;
            AnchorManager.OnAnchorStateChanged -= OnAnchorStateChanged;
            LevelManager.OnVoipChanged -= OnVoipChange;
        }
        #endregion


        #region Private Methods


        /// <summary>
        /// This CallBack is called when Wifi connection have changes.
        /// </summary>
        /// <param name="status">Has connection.</param>
        private void OnInternetConnectionChanged(bool status)
        {
            _wifiOnIcon.SetActive(status);
            _wifiOffIcon.SetActive(!status);
        }

        /// <summary>
        /// This CallBakc is called when MasterVolume has changed.
        /// </summary>
        /// <param name="volume">Value between -80f(mute) and 0f(normal).</param>
        private void OnVolumeChange(float volume)
        {
            _soundOffIcon.SetActive(volume <= -80f);
            _soundOnIcon.SetActive(volume > -80f);
        }

        /// <summary>
        /// This callback is called when Primary Recorder transmission state is changed
        /// </summary>
        /// <param name="status">Is transmiting</param>
        private void OnVoipChange(bool status)
        {
           // Debug.Log("VOIP CHANGED to " + status);
            _voiceOffIcon.SetActive(!status);
            _voiceOnIcon.SetActive(status);
        }

        /// <summary>
        /// This CallBakc is called when AnchorManager State has changed.
        /// </summary>
        /// <param name="state">Current state of anchoring</param>
        private void OnAnchorStateChanged(StateAnchorManager state)
        {
            Debug.Log(state.ToString());
                switch (state)
                {
                    case StateAnchorManager.DISABLED:
                        _anchorButton.SetActive(false);
                        _closeButton.SetActive(false);
                        _mainButton.SetActive(true);
                        break;
                    case StateAnchorManager.ANCHORED:
                        _anchorButton.SetActive(false);
                        _closeButton.SetActive(true);
                        _mainButton.SetActive(false);
                        break;
                    case StateAnchorManager.ANCHORING:
                        _anchorButton.SetActive(true);
                        _closeButton.SetActive(false);
                        _mainButton.SetActive(false);
                        break;
                    default:
                        break;
                }
        }
        
        /// <summary>
        /// This method resizes radial menu by elements count
        /// </summary>
        private void ResizeRadialMenu() //TODO Creates a dynamic and responsive system
        {
            if (_radialMenu != null) {
                Image sprite = _radialMenu.GetComponent<Image>();
                sprite.fillAmount = _radialMenu.transform.childCount * .10f;
            }
        }

        #endregion

        #region Buttons Actions

        /// <summary>
        /// This Method is called by BT_CONFIG UI element. Shows the radial menu
        /// </summary>
        /// <param name="status">Is Active</param>
        public void ShowRadialMenu(bool status)
        {
            _isActive = status;
            _closeTimer = status ? _closeInactiveMenuTime : 0;
            _radialMenu.SetActive(status);
            _foldedButton.SetActive(status);
            _mainButton.SetActive(!status);
        }


        /// <summary>
        /// This method is called by UI elements to keep Radial Menu active.
        /// </summary>
        public void Interacting()
        {
            if (_isActive) 
            _closeTimer = _closeInactiveMenuTime;
        }

        /// <summary>
        /// This method is called by UI elements
        /// </summary>
        /// <param name="option">Action option.</param>
        public void SelectOption(string option)
        {
            _isActive = false;
            _closeTimer = 0;
            _radialMenu.SetActive(false);
            _foldedButton.SetActive(false);
            _mainButton.SetActive(true);

            switch (option)
            {
                case "AUDIO"://It's called by BT_SOUND
                    LevelManager.SetAudio();
                break;
                case "VOICE"://It's called by BT_SOUND
                    LevelManager.SetVoip();
                    break;
                case "ANCHORING"://It's called by BT_ANCHORING
                    OnClose = AnchorManager.HandleAnchorPlacement;
                    AnchorManager.HandleAnchorPlacement();
                    break;
                case "ANCHORED"://It's called by BT_ANCHORED
                    AnchorManager.CreateAnchor();
                    break;
                default:
                    break;
            }


        }

        /// <summary>
        /// This method is called by BT_CLOSE UI Element.
        /// </summary>
        public void OnCloseRelease()
        {
            OnClose?.Invoke();
            OnClose = null;
        }

        #endregion

    }

}