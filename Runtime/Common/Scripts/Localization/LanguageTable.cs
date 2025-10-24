#if OBSOLETE
using System;
using UnityEngine;

namespace Twinny.Localization
{

    /// <summary>
    /// This class creates a Language Table file
    /// </summary>
    [CreateAssetMenu(fileName = "NewLanguageTable", menuName = "Twinny/Localization/Language Table", order = 1)]
    public class LanguageTable : ScriptableObject
    {
        public string languageName = "New Language";
        public StringEntry[] stringEntries;
    }

    [Serializable]
    public class StringEntry
    {
        public string key;
        public string value;
    }

    [Serializable]
    public class LanguageEntry
    {
        public string key;
        public LanguageTable table;
    }

}

#endif