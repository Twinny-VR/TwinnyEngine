using UnityEngine;

namespace Twinny.System.Cameras
{
    public class InterestItem : MonoBehaviour
    {
        [Tooltip("If not 'virtualCamera' the First Person Agent will assume.")]
        [SerializeField] private CameraHandler _virtualCamera;
        public CameraHandler virtualCamera { get => _virtualCamera; }

        public bool allowFirstPerson { get => _virtualCamera.allowFirstPerson; }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
