using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Twinny.Helpers;
using UnityEditor;
using UnityEngine;

namespace Twinny.Localization
{


    /// <summary>
    /// This class controls the Language system.
    /// </summary>
    public class LocalizationProvider
    {
        #region Fields
        private static LanguageTable _currentLanguageTable;
        private static List<LanguageTable> _languageTables = new List<LanguageTable>();
        #endregion

        #region CallBack Delegates
        public delegate void onLanguageChanged();
        public static onLanguageChanged OnLanguageChanged;
        #endregion

        #region Constructor
        static LocalizationProvider()
        {
            _languageTables = Resources.LoadAll<LanguageTable>("Tables").ToList();

            CultureInfo currentCulture = CultureInfo.CurrentCulture;
                SetLanguage(currentCulture.Name);
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// This method changes language table
        /// </summary>
        /// <param name="languageTable">Table with language strings</param>
        public static void SetLanguage(LanguageTable languageTable)
        {
            _currentLanguageTable = languageTable;
            OnLanguageChanged?.Invoke();
        }


        /// <summary>
        /// This method changes language table by Table Name
        /// </summary>
        /// <param name="languageTable">Table with language strings</param>
        public static void SetLanguage(string languageTable)
        {
            _currentLanguageTable = _languageTables.FirstOrDefault(t => t.name == languageTable);

            if (_currentLanguageTable == null)
            {
                Debug.LogWarning($"[LocalizationProvider] '{languageTable}' file not found.");
            }


            OnLanguageChanged?.Invoke();
        }



        /// <summary>
        /// This method get string key translation
        /// </summary>
        /// <param name="key">Word key to get a translation.</param>
        /// <returns>Returns the translated word.</returns>
        public static string GetTranslated(string key)
        {
            if(_currentLanguageTable != null)
            foreach (var entry in _currentLanguageTable.stringEntries)
            {
                if (entry.key == key)
                {
                    return entry.value;
                }
            }

            return key; // If not found returns the fallback key.
        }
        #endregion
       

    }




}