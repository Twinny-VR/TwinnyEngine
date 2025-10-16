using System.Linq;
using Twinny.UI;
using UnityEngine;

namespace Twinny.System
{


    public class CutoffOcclusion : MonoBehaviour
    {

        private Transform[] m_childs;
        private void Start()
        {
            m_childs = transform.GetComponentsInChildren<Transform>(true);
        }

        private void OnEnable()
        {
            MainInterface.OnCutoffChanged += OnCutoffChanged;
        }

        private void OnDisable()
        {
            MainInterface.OnCutoffChanged -= OnCutoffChanged;
        }

        private void OnCutoffChanged(float value)
        {
            foreach (var item in m_childs)
            {
                if(item == transform) continue;
                bool active = item.position.y < value;
                item.gameObject.SetActive(active);
            }

        }
    }

}