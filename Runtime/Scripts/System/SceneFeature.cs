using System.Collections;
using System.Collections.Generic;
using Twinny.Helpers;
using Twinny.System.Cameras;
using Twinny.UI;
using UnityEngine;

namespace Twinny.System
{
    public class SceneFeature : TSingleton<SceneFeature>
    {

#if !OCULUS

        public InterestItem[] interestPoints;

#endif


        #region MonoBehaviour Methods

#if UNITY_EDITOR
        private void OnValidate()
        {
        }
#endif
        private void Start()
        {
            Init();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method change World Transform position to a especific landMark position.
        /// </summary>
        /// <param name="landMarkIndex">Index on landMarks array.</param>
        public void TeleportToLandMark(int landMarkIndex)
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
