using Concept.Helpers;
using Twinny.Helpers;
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
        [SerializeField] private OnInternetConnectedEvent OnInternetConnectedEvent;
        [SerializeField] private OnInternetDisconnectedEvent OnInternetDisconnectedEvent;

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
        [SerializeField] private GameObject _soundOnIcon;
        [SerializeField] private GameObject _soundOffIcon;
        [SerializeField] private GameObject _voiceOnIcon;
        [SerializeField] private GameObject _voiceOffIcon;

   //     private Action<bool> OnClose;
        #endregion



        #region MonoBehaviour Methods
        // Start is called before the first frame update
        void Start()
        {
            //Set CallBacks
            NetworkUtils.OnInternetConnectedEvent.AddListener(OnInternetConnected);
            NetworkUtils.OnInternetDisconnectedEvent.AddListener(OnInternetDisconnected);
            AudioManager.OnVolumeChanged += OnVolumeChange;
            AudioManager.OnVoipChanged += OnVoipChange;
            AnchorManager.OnAnchorStateChanged += OnAnchorStateChanged;

            //Sets Audio Feed UI Icon
            OnVolumeChange(AudioManager.GetAudioVolume());

            //Sets Voip Feed UI Icon
            // OnVoipChange(LevelManager.GetVoipStatus());

            //Sets WiFi Feed UI Icon
            bool hasInternet = NetworkUtils.IsWiFiConnected();

            if (hasInternet)
                OnInternetConnected();
            else
                OnInternetDisconnected();

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
            NetworkUtils.OnInternetConnectedEvent.RemoveListener(OnInternetConnected);
            NetworkUtils.OnInternetDisconnectedEvent.RemoveListener(OnInternetDisconnected);
            AudioManager.OnVolumeChanged -= OnVolumeChange;
            AnchorManager.OnAnchorStateChanged -= OnAnchorStateChanged;
            AudioManager.OnVoipChanged -= OnVoipChange;
        }
        #endregion


        #region Private Methods


        /// <summary>
        /// This CallBack is called when find Wifi connection.
        /// </summary>
        private void OnInternetConnected()
        {
            OnInternetConnectedEvent?.Invoke();
        }

        /// <summary>
        /// This CallBack is called when Wifi connection lost.
        /// </summary>
        private void OnInternetDisconnected()
        {
            OnInternetDisconnectedEvent?.Invoke();
        }

        /// <summary>
        /// This CallBakc is called when MasterVolume has changed.
        /// </summary>
        /// <param name="volume">Value between -80f(mute) and 0f(normal).</param>
        private void OnVolumeChange(float volume)
        {
            if(_soundOffIcon && _soundOnIcon)
            {

            _soundOffIcon.SetActive(volume <= -80f);
            _soundOnIcon.SetActive(volume > -80f);
            }
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
                    case StateAnchorManager.ANCHORED:
                    if (_isActive)
                    {
                        _anchorButton.SetActive(false);
                        _closeButton.SetActive(false);
                        _mainButton.SetActive(true);
                    }
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

            Debug.LogWarning($"[RadialMenu] {option} selected.");

            _isActive = false;
            _closeTimer = 0;
            _radialMenu.SetActive(false);
            _foldedButton.SetActive(false);
            _mainButton.SetActive(true);

            switch (option)
            {
                case "AUDIO"://It's called by BT_SOUND
                    AudioManager.SetAudio();
                break;
                case "VOICE"://It's called by BT_SOUND
                    //LevelManager.SetVoip(); TODO Implement this feature
                    break;
                case "ANCHORING"://It's called by BT_ANCHORING
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
            ShowRadialMenu(false);
        }

        #endregion


        [ContextMenu("Start Anchoring")]
        public void Anchoring() {
            SelectOption("ANCHORING");
        }

        [ContextMenu("Create Anchor")]
        public void Anchored()
        {
            SelectOption("ANCHORED");
        }



    }

}
