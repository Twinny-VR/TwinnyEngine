using System.Collections;
using System.Collections.Generic;
using Twinny.System;
using Twinny.XR;
using UnityEngine;

public class LandMarkLinkedObjects : MonoBehaviour
{

    [SerializeField] private LandMarkNode _landMarkNode;
    [SerializeField]
    private List<GameObject> _childreen = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        SceneFeatureXR.Instance.OnTeleportToLandMark += OnTeleportToLandMark;
        for (int i = 0; i < transform.childCount; i++)
        {
            _childreen.Add(transform.GetChild(i).gameObject);
        }
    }

    private void OnDisable()
    {
        SceneFeatureXR.Instance.OnTeleportToLandMark -= OnTeleportToLandMark;

    }

    private void OnTeleportToLandMark(int landMarkIndex)
    {


        int nodeIndex = SceneFeatureXR.Instance.GetLandMarkIndex(_landMarkNode);
        Debug.LogWarning($"[LandMarkLinkedObjects] Teleported to LandMark{landMarkIndex}|{nodeIndex}");

        bool active = ( nodeIndex == landMarkIndex);

        foreach (var item in _childreen)
        {
            item.SetActive(active);
        }


    }
}
