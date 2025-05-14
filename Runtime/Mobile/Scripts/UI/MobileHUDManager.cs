using System;
using System.Reflection;
using Twinny.System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Twinny.UI
{
    public class MobileHUDManager : HUDManager
    {
        [SerializeField] private GameObject BT_Home;
        [SerializeField] private GameObject BT_Immersive;

        public static new MobileHUDManager Instance { get => _instance as MobileHUDManager; }




        #region MonoBheaviour Methods
        protected override void Start()
        {
            base.Start();

        }

        protected override void Update()
        {
            base.Update();



        }

        #endregion

        #region Public Methods
        public void ShowMainMenu(bool status)
        {

            if (status)
            {
                UIElementsProvider.ShowElement("MainMenu");
            }
            else
            {
                UIElementsProvider.HideElement("MainMenu");
            }
                ShowControlsMenu(!status);
        }
        #endregion

        #region UI Callbacks

        public override void OnLoadScene()
        {
            base.OnLoadScene();
            if (MobileSceneFeature.Instance)
            {
                BT_Immersive.SetActive(MobileLevelManager.currentInterest.allowFirstPerson);
                ShowMainMenu(false);
            }
        }

        #endregion

    }

}