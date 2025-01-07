using System.Collections.Generic;
using System.Linq;
using TMPro;
using Twinny.Helpers;
using UnityEngine;

namespace Twinny.Localization
{


    /// <summary>
    /// This class controls the Language system.
    /// </summary>
    public class LanguageManager : TSingleton<LanguageManager>
    {
        #region Fields
        [SerializeField]
        private LanguageTable _currentLanguageTable;
        [SerializeField]
        public List<LanguageTable> languageTables = new List<LanguageTable>();
        #endregion

        #region CallBack Delegates
        public delegate void onLanguageChanged();
        public static onLanguageChanged OnLanguageChanged;
        #endregion

        #region MonoBehaviour Methods

        void Awake()
        {
            Init();
        }

        #endregion

        /// <summary>
        /// This method changes language table
        /// </summary>
        /// <param name="languageTable">Table with language strings</param>
        public static void SetLanguage(LanguageTable languageTable)
        {
            LanguageManager.Instance._currentLanguageTable = languageTable;
            OnLanguageChanged();
        }


        /// <summary>
        /// This method changes language table by Table Name
        /// </summary>
        /// <param name="languageTable">Table with language strings</param>
        public static void SetLanguage(string languageTable)
        {
            Instance._currentLanguageTable = Instance.languageTables.FirstOrDefault(t => t.name == languageTable);

            if (Instance._currentLanguageTable == null)
            {
                Debug.LogWarning($"[LanguageManager] '{languageTable}' file not found.");
            }


            OnLanguageChanged();
        }


        #region Static Methods

        /// <summary>
        /// This method get string key translation
        /// </summary>
        /// <param name="key">Word key to get a translation.</param>
        /// <returns>Returns the translated word.</returns>
        public static string GetTranslated(string key)
        {
            if(Instance && Instance._currentLanguageTable)
            foreach (var entry in Instance._currentLanguageTable.stringEntries)
            {
                if (entry.key == key)
                {
                    return entry.value;
                }
            }

            return key; // If not found returns the fallback key.
        }
        #endregion

        //TODO Deletar
        [ContextMenu("ENGLISH")]
        public void SetEnglish()
        {
            SetLanguage("EN");
        }

        [ContextMenu("PORTUGUES")]
        public void SetBr()
        {
            SetLanguage("PT-BR");
        }


        [ContextMenu("SPANOL")]
        public void SetEs()
        {
            SetLanguage("ES");
        }

    }




}