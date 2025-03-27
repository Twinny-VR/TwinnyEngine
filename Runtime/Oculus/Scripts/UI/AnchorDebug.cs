using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Twinny.UI
{
    public class AnchorDebug : MonoBehaviour
    {
        private Transform _transform;

        [SerializeField] private bool _showInfo = false;

        [SerializeField] private GameObject _debugInfo;
        [SerializeField] private TextMeshProUGUI TMP_Coordinates;

        // Start is called before the first frame update
        void Start()
        {
            _transform = transform;
            _debugInfo.SetActive(_showInfo);
            if(_showInfo )
            StartCoroutine(SetCoordinatesText());
        }

        // Update is called once per frame
        void Update()
        {

        }


        IEnumerator SetCoordinatesText()
        {
            while (true)
            {
                TMP_Coordinates.text = $"P:({_transform.position})\nR:({_transform.eulerAngles})"; 

                yield return new WaitForSeconds(1f);
            }
        }
    }
}
