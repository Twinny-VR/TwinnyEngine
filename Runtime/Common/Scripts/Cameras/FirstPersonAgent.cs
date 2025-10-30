using System.Threading.Tasks;
using Concept.Core;
using Concept.Helpers;
using Twinny.UI;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using static Twinny.System.Cameras.CameraManager;

namespace Twinny.System.Cameras
{
    public class FirstPersonAgent : TSingleton<FirstPersonAgent>
    {
        #region Fields
        [SerializeField] private NavMeshAgent _navMeshAgent;
       [SerializeField] private CameraHandler _fpsCamera;
        [SerializeField] private bool _isMoving;
        private static bool _isActive;
        public static bool isActive { get => _isActive; }

        #endregion

        #region Properties
        private GameObject _hitPoint;
        #endregion

        #region Delegates

        public delegate void onFpsMode(bool status);
        public static onFpsMode OnFpsMode;

        public delegate void onArrived(Vector3 position);
        public onArrived OnArrived;

        #endregion

        #region MonoBehaviour Methods


        protected override void Awake()
        {
            base.Awake();
            if (!_navMeshAgent) _navMeshAgent = GetComponent<NavMeshAgent>();
     //       if (!_mainCamera) _mainCamera = Camera.main;

            //OnStateChanged += OnCameraStateChanged;
            //OnCameraLocked += OnLockedInBuilding;
            InputMonitor.OnSelect += OnObjectSelected;

        }


        protected override void Start()
        {
            base.Start();

            OnArrived += OnArrivedAtDestination;

            if (config)
            {
                _navMeshAgent.speed = config.navigationSpeed;
                _navMeshAgent.angularSpeed = config.navigationangularSpeed;
                _navMeshAgent.acceleration = config.navigationAcceleration;
            }

        }


        private void OnDestroy()
        {
            InputMonitor.OnSelect -= OnObjectSelected;
           // OnStateChanged -= OnCameraStateChanged;
           // OnCameraLocked -= OnLockedInBuilding;

        }


        [ContextMenu("GET DISTANCE")]
        public void GetDistance() {

            Debug.LogWarning($"MOVING: {_isMoving} DISTANCE:{Vector3.Distance(transform.position,_navMeshAgent.destination)} REMAINING:{_navMeshAgent.remainingDistance} PENDING:{_navMeshAgent.pathPending}");

        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
            if (_isMoving && _navMeshAgent.isActiveAndEnabled && _navMeshAgent.isOnNavMesh && _navMeshAgent.remainingDistance <= .25f && !_navMeshAgent.pathPending)
            {
                OnArrived?.Invoke(transform.position);
            }

            //if(_navMeshAgent.isActiveAndEnabled && _navMeshAgent.isOnNavMesh && _navMeshAgent.remainingDistance < .25f) Debug.LogWarning("CHEGANDO!");

        }
        /*
            // Verifica se o botão esquerdo do mouse foi pressionado
            if (Input.GetMouseButtonDown(0))
            {
                // Cria um raycast do clique do mouse
                RaycastHit hit;
                Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);


                if(Physics.Raycast(ray, out hit, Mathf.Infinity)) { 


                    NavMeshHit navMeshHit;
                    if (NavMesh.SamplePosition(hit.point, out navMeshHit, 1.0f, NavMesh.AllAreas))
                    {
                        Vector3 navMeshPosition = navMeshHit.position;

                        _navMeshAgent.destination = navMeshPosition;

                    }

                }

            }
        }
    */
        #endregion

        void TestNavMeshAtPosition(Vector3 testPos)
        {
            NavMeshHit hit;
            bool success = NavMesh.SamplePosition(testPos, out hit, 5f, NavMesh.AllAreas);
            Debug.Log($"NavMesh Test: {success}, Position: {hit.position}, Distance: {hit.distance}");

            if (!success)
            {
                Debug.LogError("NavMesh is not working on mobile! Check bake settings and build inclusion.");
            }
        }


        private void OnCameraStateChanged(State state)
        {
            _navMeshAgent.enabled = state == State.FPS;
        }

        private void OnLockedInBuilding(BuildingFeature building)
        {
            if (building)
            {
                transform.position = building.facadeTeleportNode.position;
                transform.rotation = building.facadeTeleportNode.rotation;
            }

        }

        public void OnObjectSelected(RaycastHit hit)
        {
            var brain = Camera.main.GetComponent<CinemachineBrain>();

            if (brain.IsBlending) return;
            
            Debug.LogWarning("CLICOU EM: " +hit.collider.gameObject.name );

            NavMeshHit navMeshHit;
            if (NavMesh.SamplePosition(hit.point, out navMeshHit, 1.0f, NavMesh.AllAreas))
            {
                Vector3 navMeshPosition = navMeshHit.position;

                //float multiply = (state == State.THIRD ? Mathf.Abs(zoom) : 1f);


                float distance = Vector3.Distance(transform.position, navMeshPosition);
                Debug.LogWarning("CAN NAVIGATE: " +(distance < config.navigationMaxDistance) );

                if (distance < config.navigationMaxDistance)
                    NavigateTo(navMeshPosition);


            }
                Debug.LogWarning("NAVMESH HIT: " +navMeshHit );

        }

        public static void TakeControl(Transform node = null)
        {
            Debug.LogWarning("NAVIGATE TO: " + node);
            if(node) TeleportTo(node);
            TakeControl(true);

#if UNITY_ANDROID || UNITY_IOS
            Debug.Log("=== MOBILE NAVMESH TEST ===");
           Instance.TestNavMeshAtPosition(Instance.transform.position);
#endif

        }
        public static void TakeControl(bool status)
        {
            _isActive = status;
            if (status)
                CallbackHub.CallAction<ICameraCallBacks>(callback => callback.OnChangeCamera(Instance._fpsCamera));
            OnFpsMode?.Invoke(status);
            Instance._navMeshAgent.enabled = status;
        }


        public static void TeleportTo(Transform node)
        {

            // OnCameraLocked?.Invoke(null);
            if (Instance && node)
            {
                Instance._navMeshAgent.enabled = false;
                Instance.transform.position = node.position;
                Instance.transform.rotation = node.rotation;
            }
        }


        private void NavigateTo(Vector3 position)
        {
            Debug.LogWarning("NAVIGATE TO: " + position);

            if (_hitPoint) Destroy(_hitPoint);
            if (config.hitPointPrefab) 
            _hitPoint = Instantiate(config.hitPointPrefab, position, config.hitPointPrefab.transform.rotation);
            _navMeshAgent.destination = position;
            _isMoving = true;

        }

        private void OnArrivedAtDestination(Vector3 position)
        {
            _isMoving = false;
            if (_hitPoint) Destroy(_hitPoint);

        }
    }
}
