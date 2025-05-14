using Concept.Helpers;
using Oculus.Interaction;
using Twinny.Helpers;
using UnityEngine;

namespace Twinny.UI
{

    /// <summary>
    /// To CurvedMenu works the CullingMask UI layer must be disabled.
    /// </summary>
    public class CurvedMenu : MonoBehaviour
    {
        private Transform _transform;
        [SerializeField]
        private Cylinder _cylinder;
        [SerializeField] private Transform _projectionMesh;
        [SerializeField] private Vector3 _positionOffset;
        [Range(0,90)]
        [SerializeField] private float _pitchRotation;

        private void Awake()
        {
            _transform = transform;
        }

        // Start is called before the first frame update
        void Start()
        {

            float radius = _cylinder.Radius;
            Vector3 position = _positionOffset;
            position.z = -radius + position.z;
            _transform.position = position;
            AsyncOperationExtensions.CallDelayedAction(() =>
            {
            Vector3 rotation = _projectionMesh.eulerAngles;
            rotation.x = _pitchRotation;
            _projectionMesh.eulerAngles = rotation;
            });
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
