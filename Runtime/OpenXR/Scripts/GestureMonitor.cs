using Concept.Core;
using Concept.Helpers;
using Oculus.Interaction.HandGrab;
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
        [Header("Interactions")]
        [SerializeField] private HandGrabInteractor _handGrabInteractorLeft;
        [SerializeField] private HandGrabInteractor _handGrabInteractorRight;


        [Header("Trace Interactables")]
        [SerializeField] private bool _traceInteractables;
        [SerializeField] private float _closeUpTime = 1f;

        [SerializeField] private float _rayDistance = 10f;  // Dist�ncia do raio
        [SerializeField] private LayerMask _interactableLayer;
        private Interactable _target;
        private float _observeTime = 0f;
        #endregion

        #region Delegates
        public delegate void onPinchLeft();
        public onPinchLeft OnPinchLeft;
        public delegate void onPinchRight();
        public onPinchRight OnPinchRight;
        public delegate void onGrabbing(bool status);
        public onGrabbing OnGrabbing;
        #endregion

        #region MonoBehaviour Methods


        protected override void Start()
        {
            base.Start();
            OnGrabbing += OnGrabbingCallBack;

            _camera = Camera.main;
            FindHands();
            if (!_leftHand)
                Debug.LogWarning("[GestureMonitor] Left Hand NOT FOUND");
            if (!_rightHand)
                Debug.LogWarning("[GestureMonitor] Right Hand NOT FOUND");

        }


        protected override void Update()
        {
            base.Update();

            if (_traceInteractables) TraceInteractables();

            if (!_leftHand || !_rightHand) return;

            if (IsPinching(_leftHand, ref _wasPinchingLeft)) OnPinchLeft?.Invoke();
            if (IsPinching(_rightHand, ref _wasPinchingRight)) OnPinchRight?.Invoke();
            
            OnGrabbing(_handGrabInteractorLeft.IsGrabbing || _handGrabInteractorRight.IsGrabbing);
        }
        #endregion


        #region Privates Methods

        // Fun��o para encontrar a m�o associada ao controlador
        private void FindHands()
        {
            // Encontrar todas as m�os (OVRHands) na cena
            OVRHand[] hands = FindObjectsByType<OVRHand>(FindObjectsSortMode.None);

            // Iterar sobre as m�os para associar a m�o direita ou esquerda com base no controlador
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

            // Verificar se a m�o est� rastreada e detectar pin�a
            if (hand != null && hand.IsTracked)
            {
                isPinching = hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
            }

            // Verifique se houve uma transi��o do estado de "n�o pin�ando" para "pin�ando"
            if (isPinching && !wasPinching)
            {
                wasPinching = true;  // Marque que agora a m�o est� pin�ando
                return true; // Retorne true apenas no momento que come�ou o pin�amento
            }
            else if (!isPinching)
            {
                wasPinching = false; // Se a m�o n�o est� pin�ando, resetar o estado
            }

            return false; // Se n�o for o momento de iniciar o pin�amento, retorne false
        }


        private bool IsPalmUp(OVRHand hand)
        {
            if (hand != null && hand.IsTracked)
            {
                // Obter a rota��o da m�o
                Quaternion palmRotation = hand.transform.rotation;

                // Obter o vetor da dire��o para cima a partir da rota��o da m�o
                Vector3 palmUp = palmRotation * Vector3.up;

                // Se a dire��o da palma (palmUp) tiver um componente Y positivo, a palma est� para cima
                return palmUp.y > 0;
            }

            return false;
        }

        private bool IsPalmDown(OVRHand hand)
        {
            if (hand != null && hand.IsTracked)
            {
                // Obter a rota��o da m�o
                Quaternion palmRotation = hand.transform.rotation;

                // Obter o vetor da dire��o para cima a partir da rota��o da m�o
                Vector3 palmUp = palmRotation * Vector3.up;

                // Se a dire��o da palma (palmUp) tiver um componente Y negativo, a palma est� para baixo
                return palmUp.y < 0;
            }

            return false;
        }



        private bool IsPointing(OVRHand hand)
        {
            if (hand != null && hand.IsTracked)
            {
                bool isPointing = hand.GetFingerIsPinching(OVRHand.HandFinger.Index) == false &&
                                  hand.GetFingerIsPinching(OVRHand.HandFinger.Middle) == false &&
                                  hand.GetFingerIsPinching(OVRHand.HandFinger.Ring) == false &&
                                  hand.GetFingerIsPinching(OVRHand.HandFinger.Pinky) == false;

                // Certifique-se de que o dedo indicador est� esticado e a palma da m�o est� voltada para cima
                return isPointing && IsPalmUp(hand);
            }

            return false;
        }



        private bool IsFist(OVRHand hand)
        {
            if (hand != null && hand.IsTracked)
            {
                // Verifique se todos os dedos est�o fechados (n�o est�o pin�ando)
                bool isFist = hand.GetFingerIsPinching(OVRHand.HandFinger.Index) &&
                              hand.GetFingerIsPinching(OVRHand.HandFinger.Middle) &&
                              hand.GetFingerIsPinching(OVRHand.HandFinger.Ring) &&
                              hand.GetFingerIsPinching(OVRHand.HandFinger.Pinky);

                return isFist;
            }

            return false;
        }

        private bool IsOpenHand(OVRHand hand)
        {
            if (hand != null && hand.IsTracked)
            {
                // Verifique se nenhum dedo est� fechando (n�o est�o pin�ando)
                bool isOpen = !hand.GetFingerIsPinching(OVRHand.HandFinger.Index) &&
                              !hand.GetFingerIsPinching(OVRHand.HandFinger.Middle) &&
                              !hand.GetFingerIsPinching(OVRHand.HandFinger.Ring) &&
                              !hand.GetFingerIsPinching(OVRHand.HandFinger.Pinky);

                return isOpen;
            }

            return false;
        }

        private bool IsTeleportGesture(OVRHand hand)
        {
            if (hand != null && hand.IsTracked)
            {
                return IsPointing(hand) && IsPalmUp(hand);
            }

            return false;
        }



        private void TraceInteractables()
        {
            RaycastHit hit;
            Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);
            if (Physics.Raycast(ray, out hit, _rayDistance, _interactableLayer))
            {
                if (hit.collider.isTrigger)
                {
                    Debug.Log("Olhando para: " + hit.collider.name);
                }
                Interactable interactableTarget = hit.collider.GetComponent<Interactable>();

                if (interactableTarget != null)
                {
                    if (_target == null || _target != interactableTarget)
                    {
                        if (_target != null) _target.SetHighLight(false);
                        _target = interactableTarget;
                        _target.SetHighLight();
                        _observeTime = 0f;
                    }

                    _observeTime += Time.deltaTime;
                    if (_observeTime > _closeUpTime)
                    {
                        Debug.Log("Olhando para: " + interactableTarget.name);
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
            else
            {
                if (_target != null) _target.SetHighLight(false);
                _target = null;
                _observeTime = 0f;

            }
        }
        #endregion

        #region CallBacks


        public void OnGrabbingCallBack(bool status)
        {
            CallbackHub .CallAction<IUICallBacks>(callback => callback.OnHudStatusChanged(status));

        }

        #endregion
    }

}