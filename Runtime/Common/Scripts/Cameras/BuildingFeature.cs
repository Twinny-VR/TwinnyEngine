using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Twinny.System.Cameras;
using UnityEngine;

public class BuildingFeature : OldInterestItem
{


    [Header("CACHED")]
    [SerializeField] private GameObject _prisma;
    [Header("SENSORS")]
    public Transform centralSensor;
    public Transform facadeTeleportNode;
    //public Transform sensorCentralLook;
    // Start is called before the first frame update
    void Start()
    {
        OldCameraManager.OnCameraLocked += OnCameralocked;
    }

    private void OnDestroy()
    {
        OldCameraManager.OnCameraLocked -= OnCameralocked;

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
    }
}
