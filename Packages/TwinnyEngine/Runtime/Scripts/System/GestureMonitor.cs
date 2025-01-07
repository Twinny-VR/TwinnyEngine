using System.Collections;
using System.Collections.Generic;
using Twinny.Helpers;
using Twinny.UI;
using UnityEngine;


namespace Twinny.System
{


    /// <summary>
    /// This class is used to monitoring the hand gestures with delegates callbacks
    /// </summary>
    public class GestureMonitor : TSingleton<GestureMonitor>
    {

        #region Fields
        private Camera _camera;
        private OVRHand _leftHand, _rightHand;
        private bool _wasPinchingLeft, _wasPinchingRight = false;  // Left and Right hand status Flag
        [Header("Trace Interactables")]
        [SerializeField] private bool _traceInteractables;
        [SerializeField] private float _closeUpTime = 1f;

        [SerializeField] private float _rayDistance = 10f;  // Distância do raio
        [SerializeField] private LayerMask _interactableLayer;
        private Interactable _target;
        private float _observeTime = 0f;
        #endregion

        #region Delegates
        public delegate void onPinchLeft();
        public onPinchLeft OnPinchLeft;
        public delegate void onPinchRight();
        public onPinchRight OnPinchRight;

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
            _camera = Camera.main;  
            FindHands();
            if (!_leftHand)
                Debug.LogWarning("[GestureMonitor] Left Hand NOT FOUND");
            if (!_rightHand)
                Debug.LogWarning("[GestureMonitor] Right Hand NOT FOUND");

        }

        // Update is called once per frame
        void Update()
        {

            if (_traceInteractables) TraceInteractables();

            if(!_leftHand ||  !_rightHand) return;

            if (IsPinching(_leftHand, ref _wasPinchingLeft)) OnPinchLeft();
            if (IsPinching(_rightHand, ref _wasPinchingRight)) OnPinchRight();

        }
        #endregion


        #region Privates Methods

        // Função para encontrar a mão associada ao controlador
        private void FindHands()
        {
            // Encontrar todas as mãos (OVRHands) na cena
            OVRHand[] hands = FindObjectsOfType<OVRHand>();

            // Iterar sobre as mãos para associar a mão direita ou esquerda com base no controlador
            foreach (OVRHand hand in hands)
            {
                if (hand.GetHand() == OVRPlugin.Hand.HandLeft)
                    _leftHand = hand;
                if (hand.GetHand() == OVRPlugin.Hand.HandRight)
                    _rightHand = hand;
            }
        }


        private bool IsPinching(OVRHand hand, ref bool wasPinching)
        {
            bool isPinching = false;

            // Verificar se a mão está rastreada e detectar pinça
            if (hand != null && hand.IsTracked)
            {
                isPinching = hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
            }

            // Verifique se houve uma transição do estado de "não pinçando" para "pinçando"
            if (isPinching && !wasPinching)
            {
                wasPinching = true;  // Marque que agora a mão está pinçando
                return true; // Retorne true apenas no momento que começou o pinçamento
            }
            else if (!isPinching)
            {
                wasPinching = false; // Se a mão não está pinçando, resetar o estado
            }

            return false; // Se não for o momento de iniciar o pinçamento, retorne false
        }

        private void TraceInteractables()
        {
            RaycastHit hit;
            Ray ray = new Ray(_camera.transform.position,_camera.transform.forward);
            if (Physics.Raycast(ray,out hit, _rayDistance,_interactableLayer))
            {
                if (hit.collider.isTrigger)
                {
                    Debug.Log("Olhando para: " + hit.collider.name);
                }
                Interactable interactableTarget = hit.collider.GetComponent<Interactable>();

                if (interactableTarget != null)
                {
                    if (_target == null || _target != interactableTarget) { 
                        if(_target != null) _target.SetHighLight(false);
                        _target = interactableTarget;
                        _target.SetHighLight();
                        _observeTime = 0f;
                    }

                    _observeTime += Time.deltaTime;
                    if (_observeTime > _closeUpTime) { 
                        Debug.Log("Olhando para: " + interactableTarget.name);
                        _observeTime = 0f;
                    }

                }else
                {
                    if (_target != null) _target.SetHighLight(false);
                    _target = null;
                    _observeTime = 0f;

                }
            }
            else
            {
                if (_target != null) _target.SetHighLight(false);
                _target = null;
                _observeTime = 0f;

            }
        }
        #endregion
    }

}