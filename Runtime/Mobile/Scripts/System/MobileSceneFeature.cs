using System.Collections;
using System.Collections.Generic;
using Twinny.Helpers;
using Twinny.System.Cameras;
using Twinny.UI;
using UnityEngine;

namespace Twinny.System
{
    public class MobileSceneFeature : SceneFeature
    {

       // public Material sceneSkyBox;
        public new InterestItem[] interestPoints;



        #region MonoBehaviour Methods

#if UNITY_EDITOR
        private void OnValidate()
        {
        }
#endif

        protected override void Start()
        {
            Init();
           // SetHDRI(sceneSkyBox);
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
                MobileLevelManager.ChangeInterest(interestPoints[landMarkIndex]);
            }
            else {
                Debug.LogError($"[MobileSceneFeature] Not enough LandMarks in '{gameObject.scene.name}' scene.");
            }

        }

        #endregion
    }
}
