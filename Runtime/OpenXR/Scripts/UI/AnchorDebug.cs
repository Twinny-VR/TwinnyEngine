using System.Collections;
using TMPro;
using Twinny.System;
using UnityEngine;

namespace Twinny.UI
{
    public class AnchorDebug : MonoBehaviour
    {
        private Transform _transform;
        [SerializeField] private GameObject _debugVisual;

        private string _displayText;
        public string displayText { get => _displayText; set => _displayText = value;  }

        [SerializeField] private bool _showInfo = false;
        public bool showInfo { get => _showInfo; set { if (!TwinnyManager.config.isTestBuild) return;  _showInfo = value; _debugInfo.SetActive(value); }
        }
        [SerializeField] private GameObject _debugInfo;
        [SerializeField] private TextMeshProUGUI TMP_Info;

        // Start is called before the first frame update
        void Start()
        {
            _debugVisual.SetActive(TwinnyManager.config.isTestBuild);
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
            while (_showInfo)
            {
                TMP_Info.text = $"[{_displayText}]\nP:({_transform.position})\nR:({_transform.eulerAngles})"; 

                yield return new WaitForSeconds(1f);
            }
        }
    }
}
