#if !OCULUS
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Twinny.System.Cameras;
using Twinny.UI;
using UnityEngine;

public class BuildingFeature : MonoBehaviour
{
    [Header("Camera Overrides")]
    [SerializeField] private float _zoomLimitMin = -20f;
    [SerializeField] private float _zoomLimitMax = 100f;

    [SerializeField] private bool _overrideBlend;
    public bool overrideBlend { get { return _overrideBlend; } }
    public CinemachineBlendDefinition customBlend;



    [Header("CACHED")]
    [SerializeField] private GameObject _prisma;
    [SerializeField] private bool _hidePrismaOnSelect = false;
    [SerializeField] private HintHUD _hintHUD;
    [Header("SENSORS")]
    public InterestItem sensorCentral;
    public Transform facadeTeleportNode;
    //public Transform sensorCentralLook;
    // Start is called before the first frame update
    void Start()
    {
        CameraManager.OnCameraLocked += OnCameralocked;
    }

    private void OnDestroy()
    {
        CameraManager.OnCameraLocked -= OnCameralocked;

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnCameralocked(BuildingFeature building)
    {
      //  Debug.LogWarning($"CAMERA LOCKED: {building} THIS: {building == this}");
        if(_prisma != null) 
        _prisma.SetActive(building != this );
        if (_hintHUD != null)
        {
            _hintHUD.Fold(!building || ( building && building != this));

        }
    }


    public Vector2 GetLimiters()
    {

        return new Vector2(_zoomLimitMin, _zoomLimitMax);
    }

}
#endif