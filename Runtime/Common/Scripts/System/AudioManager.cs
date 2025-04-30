using System.Collections;
using System.Collections.Generic;
using Twinny.Helpers;
using Twinny.System;
using UnityEngine;
using UnityEngine.Audio;

namespace Twinny
{
    public class AudioManager : TSingleton<AudioManager>
    {
        #region Fields
        [SerializeField]
        private AudioMixer _audioMixer;
        #endregion


        #region Delegates
        public delegate void onVolumeChanged(float volume);
        public static onVolumeChanged OnVolumeChanged;
        public delegate void onVoipChanged(bool status);
        public static onVoipChanged OnVoipChanged;


        #endregion

        #region MonoBehaviour Methods


        #endregion





        /// <summary>
        /// This Method return current mixer volume.
        /// </summary>
        /// <param name="mixer">Exposed paramter mixer (MasterVolume is default).</param>
        /// <returns>Current mixer volume.</returns>
        public static float GetAudioVolume(string mixer = "MasterVolume")
        {
            float currentVolume = 0;
            Instance._audioMixer.GetFloat(mixer, out currentVolume);
            return currentVolume;
        }


        /// <summary>
        /// This methos sets the mixer volume value.
        /// </summary>
        /// <param name="volume">Value between -80f(mute) and 0f(normal).</param>
        /// <param name="mixer">Exposed paramter mixer (MasterVolume is default).</param>
        public static void SetAudioVolume(float volume, string mixer = "MasterVolume")
        {
            Instance._audioMixer.SetFloat(mixer, volume);
            OnVolumeChanged?.Invoke(volume);
        }

        /// <summary>
        /// Get primary recorder state
        /// Works only in multiplayer mode
        /// </summary>
        /// <returns>Returns if it's currently transmiting</returns>
        public static bool GetVoipStatus2()
        {


            return false;


        }


        /// <summary>
        /// Switch primary recorder transmission
        /// Works only in multiplayer mode
        /// </summary>
        public static void SetVoip2()
        {
        }

        /// <summary>
        /// Switch Master Mixer volume (Muted/Normal)
        /// </summary>
        public static void SetAudio()
        {
            float currentVolume = GetAudioVolume();
            SetAudioVolume((currentVolume == 0f) ? -80f : 0f);
        }

        /// <summary>
        /// Switch Mixer volume (Muted/Normal)
        /// </summary>
        /// <param name="status">True(Muted)/False(Normal)</param>
        /// <param name="mixer">Exposed paramter mixer.</param>
        public static void MuteAudio(bool status, string mixer)
        {
            SetAudioVolume(status ? -80f : 0f, mixer);
        }

    }
}
