using System.Collections;
using System.Collections.Generic;
using Twinny.System.Cameras;
using UnityEngine;

public class BuildingFeature : MonoBehaviour
{
    [SerializeField] private float _zoomLimitMin = -20f;
    [SerializeField] private float _zoomLimitMax = 100f;


    [Header("CACHED")]
    [SerializeField] private GameObject _prisma;

    [Header("SENSORS")]
    public Transform sensorCentral;
    // Start is called before the first frame update
    void Start()
    {
        CameraHandler.OnCameraLocked += OnCameralocked;
    }

    private void OnDestroy()
    {
        CameraHandler.OnCameraLocked -= OnCameralocked;

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnCameralocked(BuildingFeature building)
    {
      //  Debug.LogWarning($"CAMERA LOCKED: {building} THIS: {building == this}");
        _prisma.SetActive(building != this);
    }


    public Vector2 GetLimiters()
    {

        return new Vector2(_zoomLimitMin, _zoomLimitMax);
    }

}
