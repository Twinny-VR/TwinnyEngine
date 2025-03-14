using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Twinny.UI
{


public class HintHUD : MonoBehaviour
{

    private Animator _animator;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();   
    }

    // Update is called once per frame
    void Update()
    {
     transform.LookAt(Camera.main.transform.position);     
    }

    public void Fold(bool status)
    {
        _animator.SetBool("folded", status);
    }

    public void OnClick()
        {
            Debug.LogWarning("VISITAR");
        }

}

}