#if !OCULUS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Twinny.System.Cameras.CameraManager;

namespace Twinny.System.Cameras
{
    public class FirstPersonAgent : MonoBehaviour
    {
        #region Fields
        [SerializeField] private NavMeshAgent _navMeshAgent;
        [SerializeField] private Camera _mainCamera;
        #endregion

        #region Properties
        [SerializeField] private bool _isMoving;
        private GameObject _hitPoint;
        #endregion

        #region Delegates

        public delegate void onArrived(Vector3 position);
        public onArrived OnArrived;

        #endregion

        #region MonoBehaviour Methods
        private void Awake()
        {
            if (!_navMeshAgent) _navMeshAgent = GetComponent<NavMeshAgent>();
            if (!_mainCamera) _mainCamera = Camera.main;

            OnStateChanged += OnCameraStateChanged;
            OnCameraLocked += OnLockedInBuilding;
            InputMonitor.OnSelect += OnObjectSelected;

        }

        // Start is called before the first frame update
        void Start()
        {

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
            OnStateChanged -= OnCameraStateChanged;
            OnCameraLocked -= OnLockedInBuilding;

        }

        // Update is called once per frame
        void Update()
        {
            if (_isMoving && _navMeshAgent.isActiveAndEnabled && _navMeshAgent.isOnNavMesh && _navMeshAgent.remainingDistance <= .1f && !_navMeshAgent.pathPending)
            {
                OnArrived?.Invoke(transform.position);
            }

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

            if (brain.IsBlending) return;


            Debug.LogWarning($"SELECTED: {hit.collider.gameObject}.");


            NavMeshHit navMeshHit;
            if (NavMesh.SamplePosition(hit.point, out navMeshHit, 1.0f, NavMesh.AllAreas))
            {
                Vector3 navMeshPosition = navMeshHit.position;

                //float multiply = (state == State.THIRD ? Mathf.Abs(zoom) : 1f);


                float distance = Vector3.Distance(transform.position, navMeshPosition);

                if (distance < config.navigationDistanceMax)
                    NavigateTo(navMeshPosition);


            }

        }


        private void NavigateTo(Vector3 position)
        {
            if (_hitPoint) Destroy(_hitPoint);
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
#endif