using System;
using UnityEngine;
using UnityEngine.Events;


namespace Twinny.UI
{

    public class ResponsiveLayoutElement : MonoBehaviour
    {
        private MobileHUDManager _hud;

        private RectTransform _rectTransform;


        public LayoutElementProperties landscapeLayout = new LayoutElementProperties();
        public LayoutElementProperties portraitLayout = new LayoutElementProperties();



        private void OnDisable()
        {
            _hud.onLandscapeOrientation?.RemoveListener(OnLandscapeOrientation);
            _hud.onPortraitOrientation?.RemoveListener(OnPortraitOrientation);
        }

        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
            _hud = MobileHUDManager.Instance;
            _hud.onLandscapeOrientation?.AddListener(OnLandscapeOrientation);
            _hud.onPortraitOrientation?.AddListener(OnPortraitOrientation);
        }

        private void OnLandscapeOrientation() {
            _rectTransform.position = landscapeLayout.position;
            _rectTransform.sizeDelta = landscapeLayout.size;
            _rectTransform.eulerAngles = landscapeLayout.rotation;
            _rectTransform.localScale = landscapeLayout.scale;
        }
        private void OnPortraitOrientation() {
            _rectTransform.position = portraitLayout.position;
            _rectTransform.sizeDelta = portraitLayout.size;
            _rectTransform.eulerAngles = portraitLayout.rotation;
            _rectTransform.localScale = portraitLayout.scale;
        }
    }

    [Serializable]
    public class LayoutElementProperties
    {
        [VectorLabels("Pos X", "Pos Y")]
        public Vector2 position;
        [VectorLabels("Width", "Height")]
        public Vector2 size = new Vector2(100,100);

        public Vector3 rotation;
        public Vector3 scale = Vector3.one;

    }

}