#if OBSOLETE
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Twinny.Localization;
using Twinny.Helpers;
using UnityEngine;


namespace Twinny.UI
{

    [RequireComponent(typeof(TMP_Text))]

    /// <summary>
    /// This class manage TEXT field accord Localization Settings
    /// </summary>
    public class LocalizationText : MonoBehaviour
    {
        #region Cached Components
        private TMP_Text _textMesh;
        #endregion

        #region MonoBehaviour Methods
        // Start is called before the first frame update
        void Start()
        {
            _textMesh = GetComponent<TMP_Text>();
            SetText();
            LocalizationProvider.OnLanguageChanged += SetText;

        }

        // Start is called when component or object are removed
        private void OnDestroy()
        {
            LocalizationProvider.OnLanguageChanged -= SetText;
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Set cached text with translated word by local keyword
        /// </summary>
        private void SetText()
        {
            if (_textMesh == null)
                return;

            string keyWord = _textMesh.text;
                _textMesh.SetLocalizationText(keyWord);
        }
        #endregion
    }
}


namespace Twinny.Helpers
{
    /// <summary>
    /// This class is a extention to Text Mesh
    /// </summary>
    public static class TextMeshExtensions
    {
        /// <summary>
        // This method insert into a TextMesh a translated text
        /// </summary>
        /// <param name="key">KeyWord to tranlate</param>
        public static void SetLocalizationText(this TMP_Text textMesh, string key)
        {
            textMesh.text = LocalizationProvider.GetTranslated(key);
        }
    }

}

#endif