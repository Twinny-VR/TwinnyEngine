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
        [SerializeField] private Material _fpsSkyBox;
        // public Material sceneSkyBox;
        public new InterestItem[] interestPoints;



        #region MonoBehaviour Methods

        private void OnEnable()
        {
            FirstPersonAgent.OnFpsMode += OnFpsMode;
            MainInterface.OnCutoffChanged += OnCutoffChanged;
        }

        private void OnDisable()
        {
            FirstPersonAgent.OnFpsMode -= OnFpsMode;
            MainInterface.OnCutoffChanged -= OnCutoffChanged;

        }

        protected override void Start()
        {
            base.Start();

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
            else
            {
                Debug.LogError($"[MobileSceneFeature] Not enough LandMarks in '{gameObject.scene.name}' scene.");
            }

        }

        #endregion

        private void OnCutoffChanged(float value)
        {
            Shader.SetGlobalFloat("_CutoffHeight", value);

        }

        private void OnFpsMode(bool status)
        {

            if (status && _fpsSkyBox != null)
                SetHDRI(_fpsSkyBox);
            else
                if (!status && sceneSkybox != null)
                SetHDRI(sceneSkybox);

        }
    }
}
