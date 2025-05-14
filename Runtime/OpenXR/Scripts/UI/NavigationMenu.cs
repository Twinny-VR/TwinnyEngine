using System;
using System.Linq;
using Concept.Helpers;
using Twinny.Localization;
using Twinny.System;
using Twinny.XR;
using UnityEngine;


namespace Twinny.UI
{


    public class NavigationMenu : TSingleton<NavigationMenu>
    {

        #region Fields
        [SerializeField] private GameObject _navigationMenu;
        private LandMarkNode _activeNode;

        [Header("Arrows")]
        [SerializeField] private GameObject _northArrow;
        [SerializeField] private GameObject _southArrow;
        [SerializeField] private GameObject _eastArrow;
        [SerializeField] private GameObject _westArrow;

        #endregion

        #region MonoBehaviour Methods

        protected override void Awake()
        {
        }

        protected override void Start()
        {
            base.Start();
            Init();
        }


        #endregion

        #region Public Methods

        public void SetArrows(LandMarkNode node)
        {
            var feature = SceneFeature.Instance as SceneFeatureXR;

            if (!feature) { Debug.LogWarning($"[NavigationMenu] Must be in a navegable SceneFeature."); return; }

            _navigationMenu?.SetActive(node && NetworkedLevelManager.IsManager);
            //            _navigationMenu.SetActive(true);    

            if (!node) return;

            _activeNode = node;
            _northArrow.SetActive(node.north);
            _southArrow.SetActive(node.south);
            _eastArrow.SetActive(node.east);
            _westArrow.SetActive(node.west);

            transform.rotation = feature.worldTransform.rotation;
        }




        #endregion

        #region UI CallBack Methods

        [ContextMenu("GO NORTH")]
        public void GoNorth()
        {
            OnArrowRelease("NORTH");
        }

        [ContextMenu("GO SOUTH")]
        public void GoSouth()
        {
            OnArrowRelease("SOUTH");
        }
        [ContextMenu("GO EAST")]
        public void GoEast()
        {
            OnArrowRelease("EAST");
        }
        [ContextMenu("GO WEST")]
        public void GoWest()
        {
            OnArrowRelease("WEST");
        }


        public void OnArrowRelease(string direction)
        {
            if (!SceneFeatureXR.Instance) { Debug.LogWarning($"[NavigationMenu] Must be in a navegable SceneFeature."); return; }
            if (!_activeNode) { Debug.LogWarning($"[NavigationMenu] Navigation nodes are not configured."); return; }

            if (!LevelManagerXR.Config.allowClickSafeAreaOutside && (AnchorManager.Instance && !AnchorManager.Instance.isInSafeArea))//TODO Globalizar isso sem anchor
            {
                AlertViewHUD.PostMessage(LocalizationProvider.GetTranslated("%BACK_TO_SAFE_AREA"), AlertViewHUD.MessageType.Warning, 5f);
                return;
            }



            LandMarkNode targetNode = _activeNode;
            switch (direction)
            {
                case "NORTH":
                    targetNode = _activeNode.north;
                    break;
                case "SOUTH":
                    targetNode = _activeNode.south;
                    break;
                case "EAST":
                    targetNode = _activeNode.east;
                    break;
                case "WEST":
                    targetNode = _activeNode.west;
                    break;
            }
            var feature = SceneFeature.Instance as SceneFeatureXR;

            LandMark landMark = feature.landMarks.FirstOrDefault(lm => lm.node == targetNode);
            int landMarkIndex = Array.IndexOf(feature.landMarks, landMark);
            NetworkedLevelManager.Instance.RPC_NavigateTo(landMarkIndex);
            //            LevelManagerXR.instance.NavigateTo(landMarkIndex);
        }

        #endregion
    }

}

