using Oculus.Interaction;
using UnityEngine;

namespace Twinny.UI
{
    /// <summary>
    /// Bespoke behavior created to control a particular palm menu. This menu primarily controls the swiping behavior,
    /// showing and hiding various options and controlling the pagination dots depending on the state of the menu. Note that, for
    /// buttons with several possible icons, the states of those buttons are controlled by the PalmMenuExampleButtonHandlers script,
    /// which manages the state of the various handlers.
    /// </summary>
    public class PalmMenu : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        private PokeInteractable _menuInteractable;

        [SerializeField]
        private GameObject _menuParent;

        [SerializeField]
        private RectTransform _menuPanel;

        [SerializeField]
        private RectTransform[] _buttons;

        [SerializeField]
        private RectTransform[] _paginationDots;

        [SerializeField]
        private RectTransform _selectionIndicatorDot;

        [SerializeField]
        private AnimationCurve _paginationButtonScaleCurve;

        [SerializeField]
        private float _defaultButtonDistance = 100f;

        [SerializeField]
        private AudioSource _paginationSwipeAudio;

        [SerializeField]
        private AudioSource _showMenuAudio;

        [SerializeField]
        private AudioSource _hideMenuAudio;

        private int _currentSelectedButtonIdx;

        #endregion

        #region MonoBehaviour Methods
        private void Start()
        {
            _currentSelectedButtonIdx = CalculateNearestButtonIdx();
            _selectionIndicatorDot.position = _paginationDots[_currentSelectedButtonIdx].position;

            _menuParent.SetActive(false);
        }

        private void Update()
        {
            var nearestButtonIdx = CalculateNearestButtonIdx();
            if (nearestButtonIdx != _currentSelectedButtonIdx)
            {
                _currentSelectedButtonIdx = nearestButtonIdx;
                _paginationSwipeAudio.Play();
                _selectionIndicatorDot.position = _paginationDots[_currentSelectedButtonIdx].position;
            }

            if (_menuInteractable.State != InteractableState.Select)
            {
                LerpToButton();
            }
        }

        #endregion

        #region Private Methods
        private int CalculateNearestButtonIdx()
        {
            var nearestButtonIdx = 0;
            var nearestDistance = float.PositiveInfinity;
            for (int idx = 0; idx < _buttons.Length; ++idx)
            {
                var deltaX = _buttons[idx].localPosition.x + _menuPanel.anchoredPosition.x;
                var adjacentIdx = deltaX < 0f ? idx + 1 : idx - 1;
                var distanceX = Mathf.Abs(deltaX);

                if (distanceX < nearestDistance)
                {
                    nearestButtonIdx = idx;
                    nearestDistance = distanceX;
                }

                var normalizingDistance = _defaultButtonDistance;
                if (adjacentIdx >= 0 && adjacentIdx < _buttons.Length)
                {
                    normalizingDistance = Mathf.Abs(_buttons[adjacentIdx].localPosition.x - _buttons[idx].localPosition.x);
                }
                var scale = _paginationButtonScaleCurve.Evaluate(distanceX / normalizingDistance);
                _buttons[idx].localScale = scale * Vector3.one;
            }
            return nearestButtonIdx;
        }

        private void LerpToButton()
        {
            var nearestX = _buttons[0].localPosition.x;
            var nearestDistance = Mathf.Abs(nearestX + _menuPanel.anchoredPosition.x);

            for (int idx = 1; idx < _buttons.Length; ++idx)
            {
                var x = _buttons[idx].localPosition.x;
                var distance = Mathf.Abs(x + _menuPanel.anchoredPosition.x);
                if (distance < nearestDistance)
                {
                    nearestX = x;
                    nearestDistance = distance;
                }
            }

            const float t = 0.2f;
            _menuPanel.anchoredPosition = Vector2.Lerp(_menuPanel.anchoredPosition, new Vector2(-nearestX, 0f), t);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Show/hide menu.
        /// </summary>
        public void ToggleMenu()
        {
            if (_menuParent.activeSelf)
            {
                _hideMenuAudio.Play();
                _menuParent.SetActive(false);
            }
            else
            {
                _showMenuAudio.Play();
                _menuParent.SetActive(true);
            }
        }
        #endregion

        #region UI Buttons Actions
        public void PalmClick()
        {
            Debug.LogWarning("PALM CLICK");
        }
        #endregion

    }
}
