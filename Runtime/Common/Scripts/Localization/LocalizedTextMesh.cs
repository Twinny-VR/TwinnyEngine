using System.Threading.Tasks;
using Concept.Localization;
using TMPro;
using UnityEngine;

namespace Twinny.UI
{

    /// <summary>
    /// This class manage TEXT field accord Localization Settings
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedTextMesh: MonoBehaviour
    {
        private const string DEFAULT_COLLECTION = "UI";

        public string collection = DEFAULT_COLLECTION;
        [SerializeField]
        private string m_key;
        public string key
        {
            get => m_key; set
            {
                m_key = value;
                _ = UpdateText();
            }
        }

        public string text { get => m_text.text; set => m_text.text = value; }

        private TMP_Text m_text;

        private void OnEnable()
        {
            LocalizationProvider.RegisterUpdateAction(UpdateText);
        }

        private void OnDisable()
        {
            LocalizationProvider.RemoveUpdateAction(UpdateText);
        }

        protected void Start()
        {
            _ = UpdateText();
        }

#if UNITY_EDITOR
        protected void OnValidate()
        {
            _ = UpdateText();
        }
#endif


        public async Task UpdateText()
        {
              if (m_key == string.Empty) return;
            if (m_text == null) m_text = GetComponent<TMP_Text>();
            if (collection == string.Empty) collection = DEFAULT_COLLECTION;
            var loc = await LocalizationProvider.GetLocalizedStringAsync(collection, m_key);
            if (loc.success)
                text = loc.text;
        }
    }


}