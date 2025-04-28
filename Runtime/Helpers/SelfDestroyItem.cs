using System.Collections;
using System.Collections.Generic;
using Twinny.Helpers;
using Twinny.UI;
using UnityEngine;

namespace Twinny
{
    public class SelfDestroyItem : MonoBehaviour
    {

        #region Properties
        [Tooltip("Time until self destroyed.")]
        [Info("seconds")]
        [SerializeField] private float _destroyAfter = 2f;
        #endregion


        // Start is called before the first frame update
        void Start()
        {
           _ = gameObject.DestroyIn(_destroyAfter);        
        }

    }
}
