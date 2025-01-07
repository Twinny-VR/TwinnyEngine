using UnityEngine;
using Meta.XR.MultiplayerBlocks.Fusion;

#if PHOTON_VOICE_DEFINED
using Fusion;
//using POpusCodec.Enums;
using System.Collections;
#endif // PHOTON_VOICE_DEFINED

namespace Twinny.System.Network
{
    /// <summary>
    /// The class responsible for setting up the <see cref="Speaker"/> game object, with the requirements for having
    /// </summary>
    public class VoiceSetup : MonoBehaviour
    {
        /// <summary>
        /// The transform of the center eye anchor of player, usually present in the player's <see cref="OVRCameraRig"/>.
        /// </summary>
        /// <remarks>Must be set before the game object's <c>Awake()</c> method is called.</remarks>
        private Transform _centerEyeAnchor;

        /// <summary>
        /// The GameObject that will be set up and hold the speaker behaviours.
        /// </summary>
        /// <remarks>Must be set before the game object's <c>Awake()</c> method is called.</remarks>
        [SerializeField]
        private GameObject _speakerPrefab;
        public GameObject Speaker;

#if PHOTON_VOICE_DEFINED
        private void Awake()
        {

            /*
            // compose speaker prefab programmatically so we don't have prefab with missing scripts
            // when project doesn't have Photon Voice package installed.
            CustomNetworkObjectProvider.RegisterCustomNetworkObject(CustomSpeakerPrefabID, () =>
            {
                var voiceObject = new GameObject("Voice");
                var audioSource = voiceObject.AddComponent<AudioSource>();
                audioSource.bypassReverbZones = true;
                audioSource.spatialBlend = 1;
                var recorder = voiceObject.AddComponent<Recorder>();
                recorder.StopRecordingWhenPaused = true;
                recorder.SamplingRate = SamplingRate.Sampling48000;
                voiceObject.AddComponent<Speaker>();
                voiceObject.AddComponent<LipSyncPhotonFix>();
                voiceObject.AddComponent<MicAmplifier>().AmplificationFactor = 2;
                voiceObject.AddComponent<VoiceNetworkObject>();
                voiceObject.AddComponent<NetworkTransform>();
                var networkObject = voiceObject.AddComponent<NetworkObject>();
                // unfortunately ObjectInterest field is not public
                var objectInterestField = typeof(NetworkObject).GetField("ObjectInterest",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var areaOfInterestEnum = typeof(NetworkObject).GetNestedType("ObjectInterestModes",
                    BindingFlags.NonPublic).GetField("AreaOfInterest");
                if (objectInterestField != null && areaOfInterestEnum != null)
                {
                    objectInterestField.SetValue(networkObject, (int)areaOfInterestEnum.GetValue(null));
                }
                return voiceObject;
            });
            */
        }

        private void Start()
        {
            _centerEyeAnchor = Camera.main.transform;
        }

        private void OnEnable()
        {
            FusionBBEvents.OnSceneLoadDone += OnLoaded;
        }

        private void OnDisable()
        {
            FusionBBEvents.OnSceneLoadDone -= OnLoaded;
        }

        private void OnLoaded(NetworkRunner networkRunner)
        {
            StartCoroutine(SpawnSpeaker(networkRunner));
        }

        private IEnumerator SpawnSpeaker(NetworkRunner networkRunner)
        {
            if(Speaker == null)
            {

            while (networkRunner == null)
            {
                yield return null;
            }

            // Spawn speaker and parent it to centerEyeAnchor
            var speaker = networkRunner.Spawn(_speakerPrefab, _centerEyeAnchor.position, _centerEyeAnchor.rotation, networkRunner.LocalPlayer);
            Speaker = speaker.gameObject;// Instantiate(_speakerPrefab, _centerEyeAnchor.position, _centerEyeAnchor.rotation, _centerEyeAnchor.transform);
            Speaker.transform.SetParent(_centerEyeAnchor.transform);
            }
             
        }
#endif // PHOTON_VOICE_DEFINED
    }
}
