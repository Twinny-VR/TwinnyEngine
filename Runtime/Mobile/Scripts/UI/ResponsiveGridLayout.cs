using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using static UnityEngine.UI.GridLayoutGroup;


namespace Twinny.UI
{

    [AddComponentMenu("Layout/Responsive Grid Layout Group")]
    /// <summary>
    /// Layout class to arrange child elements in a grid format.
    /// </summary>
    /// <remarks>
    /// The GridLayoutGroup component is used to layout child layout elements in a uniform grid where all cells have the same size. The size and the spacing between cells is controlled by the GridLayoutGroup itself. The children have no influence on their sizes.
    /// </remarks>
    public class ResponsiveGridLayoutGroup : GridLayoutGroup
    {
        [SerializeField]
        private ScreenOrientation _screenOrientation = ScreenOrientation.LandscapeLeft;
        public ScreenOrientation screenOrientation { get => _screenOrientation; set { _screenOrientation = value; RefreshLayout(value); } }
        public GridLayoutProperties landscapeLayout = new GridLayoutProperties() {cellSize = new Vector2(680f,380f), spacing = new Vector2(10f,10f), constraint = Constraint.FixedRowCount, constraintCount = 2 };
        public GridLayoutProperties portraitLayout = new GridLayoutProperties() {cellSize = new Vector2(1360f,760f), spacing = new Vector2(10f,10f), constraint = Constraint.FixedRowCount, constraintCount = 1 };

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            screenOrientation = _screenOrientation;
        }
#endif

        protected override void OnEnable()
        {
            base.OnEnable();
            MobileHUDManager.OnOrientationChanged += OnOrientationChanged;

        }

        protected override void OnDisable()
        {
            base.OnDisable();
            MobileHUDManager.OnOrientationChanged -= OnOrientationChanged;

        }

        private void OnOrientationChanged(ScreenOrientation orientation)
        {
            screenOrientation = orientation;
        }

        private void RefreshLayout(ScreenOrientation orientation)
        {
            GridLayoutProperties layout = (orientation == ScreenOrientation.Portrait || orientation == ScreenOrientation.PortraitUpsideDown) ? portraitLayout : landscapeLayout;
            padding = layout.padding;
            cellSize = layout.cellSize;
            spacing = layout.spacing;
            startCorner = layout.startCorner;
            startAxis = layout.startAxis;
            constraint = layout.constraint;
            constraintCount = layout.constraintCount;
        }

    }

    [Serializable]
    public class GridLayoutProperties
    {
        public RectOffset padding;
        public Vector2 cellSize;
        public Vector2 spacing;
        public Corner startCorner;
        public Axis startAxis;
        public TextAnchor childAlignment;
        public Constraint constraint;
        public int constraintCount;
    }

}