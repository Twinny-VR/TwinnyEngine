using System.Collections;
using System.Collections.Generic;
using Twinny.Helpers;
using Twinny.UI;
using UnityEngine;

namespace Twinny.System
{
    public class SceneFeatureMulti : SceneFeature
    {

        public Material sceneSkyBox;

        public Transform[] interestPoints;



        #region MonoBehaviour Methods

#if UNITY_EDITOR
        private void OnValidate()
        {
        }
#endif

        protected override void Start()
        {
            Init();
            SetHDRI(sceneSkyBox);
        }

        #endregion

        #region Overrided Methods

        /// <summary>
        /// This method change World Transform position to a especific landMark position.
        /// </summary>
        /// <param name="landMarkIndex">Index on landMarks array.</param>
        public override void TeleportToLandMark(int landMarkIndex)
        {
            if (interestPoints.Length > 0)
            {
                if(interestPoints[landMarkIndex] is BuildingFeature)
                {
                        BuildingFeature building = interestPoints[landMarkIndex] as BuildingFeature;
                        CameraManager.OnCameraLocked(building);
                }
                
            }
        }

        #endregion
    }
}
