#if OCULUS
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Twinny
{
    public class ShelfButtonAction : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Button _button;
        [SerializeField] private EventTrigger _eventTrigger;
        [SerializeField] private Image _fillBackground;
        [Header("Holding Configuration")]
        [SerializeField] private float _holdDuration = 2f;
        private float _holdProgress = 0f;
        public float holdProgress { get => _holdProgress; set { 
                _holdProgress = value;
                _fillBackground.fillAmount = Mathf.Clamp01(value / _holdDuration);
            } }
        private bool _isHolding = false;

        [SerializeField]
        private UnityEvent<PointerEvent> _whenHold;

        public UnityEvent<PointerEvent> WhenHold => _whenHold;


        // Start is called before the first frame update
        void Start()
        {
            if (!_button) _button = GetComponent<Button>();
            if (!_eventTrigger) _eventTrigger = GetComponent<EventTrigger>();
        }

        // Update is called once per frame
        void Update()
        {
            if (_isHolding)
            {
                if (_holdProgress < _holdDuration)
                {
                    holdProgress += Time.deltaTime;
                }
                else
                {
                    _isHolding = false;
                    OnHoldComplete();
                }
            }else
                if (_holdProgress > 0 && _holdProgress < _holdDuration)
            {
                holdProgress -= Time.deltaTime * 2f;
            }
        }


        private void OnHoldComplete()
        {
            PointerEvent pointerEventData = new PointerEvent();
            WhenHold.Invoke(pointerEventData);
            Debug.LogWarning("OnHoldComplete");
        }

        #region CallBacks
        public void OnPointerDown(PointerEventData eventData)
        {
            holdProgress = 0f;
            _isHolding = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isHolding = false;
        }

        #endregion
    }
}
#endif