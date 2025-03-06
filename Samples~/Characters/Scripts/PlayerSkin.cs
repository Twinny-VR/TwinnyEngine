using System.IO;
using Fusion;
using UnityEngine;

namespace Twinny.XR
{
    public class PlayerSkin : NetworkBehaviour
    {

        #region SERIALIZE_FIELDS
        [Header("Components")]
        [SerializeField] private SkinnedMeshRenderer playerHeadMesh = null;
        [SerializeField] private GameObject[] playerHairMeshs = null;
        [SerializeField] private GameObject playerHandsMesh = null;
//        [SerializeField] private Salsa salsaScript = null;
//        [SerializeField] private GameObject micGameObject = null;
        [SerializeField] private GameObject jacketObj = null;

        [Header("Config")]
        [SerializeField] private float jawLerp = 30f;
        #endregion

        #region PRIVATE_FIELDS
        [Networked] private float jawOpen { get; set; }
        private float _jawOpenWeight;
        private int currentHair = 0;
        #endregion

        #region CONSTANT
        private const int JAWOPEN = 31;
        #endregion

        #region MonoBehaviour Methods
        void Awake()
        {
            if (!HasStateAuthority)
            {
                SelectHair();
                DisableComponents();
            }
        }

        private void Start()
        { 
        }

        #endregion


        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();


            if (HasStateAuthority)
            {
                jawOpen = playerHeadMesh.GetBlendShapeWeight(JAWOPEN);

            }
            else
            {
                _jawOpenWeight = Mathf.Lerp(_jawOpenWeight, jawOpen, Time.deltaTime * jawLerp);
                playerHeadMesh.SetBlendShapeWeight(JAWOPEN, _jawOpenWeight);
            }

        }

        #region PRIVATE_METHODS
        private void DisableComponents()
        {
            //salsaScript.enabled = false;
//            micGameObject.SetActive(false);
            playerHeadMesh.gameObject.SetActive(true);
            playerHandsMesh.SetActive(true);
            if(jacketObj != null)
            jacketObj.SetActive(true);
            playerHairMeshs[currentHair].SetActive(true);
        }

        private void SelectHair()
        {
            currentHair = Random.Range(0, playerHairMeshs.Length);
        }
        #endregion

        #region PUBLIC_METHODS
        public void DisEnableAvatarVisual(bool enable)
        {
            if (!HasStateAuthority)
            {
                playerHeadMesh.gameObject.SetActive(enable);
                playerHandsMesh.SetActive(enable);
                if (jacketObj != null)
                    jacketObj.SetActive(enable);
                playerHairMeshs[currentHair].SetActive(enable);
            }
        }


        #endregion

        #region Callback Methods

        public override void Spawned()
        {
            base.Spawned();
            Debug.LogWarning("AVATAR LOADED");
            if (HasStateAuthority)
            {
                int newLayer = LayerMask.NameToLayer("Hidden");

                foreach (Transform child in transform)
                {
                    child.gameObject.layer = newLayer;
                }
            }

        }
            #endregion

        }
    }
