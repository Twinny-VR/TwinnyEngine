#if !OCULUS
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Twinny.System.Cameras;
using Twinny.UI;
using UnityEngine;

public class BuildingFeature : InterestItem
{


    [Header("CACHED")]
    [SerializeField] private GameObject _prisma;
    [SerializeField] private bool _hidePrismaOnSelect = false;
    [SerializeField] private HintHUD _hintHUD;
    [Header("SENSORS")]
    public Transform centralSensor;
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
        _prisma.SetActive(building == null);
        if (_hintHUD != null)
        {
            _hintHUD.Fold(!building || ( building && building != this));

        }
    }
}
#endif