using UnityEngine;

namespace Twinny.UI
{
    public class PlayerList : MonoBehaviour
    {
        [SerializeField] private HUDManagerXR _hudManager;
        [SerializeField] private Transform _playersList;
        [SerializeField] private GameObject _playerKnobPrefab;


        private void Start()
        {
            if (_playersList == null) _playersList = transform;
            if(_hudManager == null) _hudManager = FindFirstObjectByType<HUDManagerXR>();
            _hudManager.OnPlayerListEvent += ResizePlayersList;


        }

        private void OnDestroy()
        {
            _hudManager.OnPlayerListEvent -= ResizePlayersList;
        }

        public void ResizePlayersList(int count)
        {
            Debug.Log("ERA PRA RESIZAR PRA: " + count);

            foreach (Transform child in _playersList)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < count; i++)
            {

                Instantiate(_playerKnobPrefab, _playersList);
            }
        }
    }
}
