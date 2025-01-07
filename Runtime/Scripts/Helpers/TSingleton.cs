using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Twinny.Helpers
{
    /// <summary>
    /// Twinny Singleton Helper
    /// This script turn a inheritance class to singleton
    /// </summary>
    /// <typeparam name="T">Class to turn singleton</typeparam>
    public abstract class TSingleton<T> : MonoBehaviour where T : TSingleton<T>
    {
        #region Singleton Instance
        private static T _instance;
        public static T Instance { get { return _instance; } }
        #endregion

        #region Fields
        [SerializeField] private bool _dontDestroyOnLoad = false;
        [SerializeField] private bool _overwriteInstance = false;

        #endregion

        /// <summary>
        /// Initializes the singleton
        /// </summary>
        public virtual void Init()
        {
            if (_dontDestroyOnLoad)
                DontDestroyOnLoad(this.gameObject);

            if (_overwriteInstance)
            {
                _instance = this as T;
            }
            else
            {
                if (!_instance) _instance = this as T;
                else
                    Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Twinny Singleton Helper
    /// This script turn a inheritance class to singleton
    /// </summary>
    /// <typeparam name="T">Class to turn singleton</typeparam>
    public abstract class NSingleton<T> : NetworkBehaviour where T : NSingleton<T>
    {
        #region Singleton Instance
        private static T _instance;
        public static T Instance { get { return _instance; } }
        #endregion

        #region Fields
        [SerializeField] private bool _dontDestroyOnLoad = false;
        [SerializeField] private bool _overwriteInstance = false;
        #endregion

        /// <summary>
        /// Initializes the singleton
        /// </summary>
        public virtual void Init()
        {

            if (_dontDestroyOnLoad)
                DontDestroyOnLoad(this.gameObject);

            if (_overwriteInstance)
            {
                _instance = this as T;
            }
            else
            {
                if (!_instance) _instance = this as T;
                else
                    Destroy(gameObject);
            }

        }
    }

}