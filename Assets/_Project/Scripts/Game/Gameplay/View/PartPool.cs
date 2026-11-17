using UnityEngine;
using UnityEngine.Pool;

namespace ColorBlockJam.Game.Gameplay.View
{
    public sealed class PartPool
    {
        readonly GameObject _prefab;
        readonly Transform _storage;
        readonly ObjectPool<GameObject> _pool;

        public PartPool(GameObject prefab, Transform storage)
        {
            _prefab = prefab;
            _storage = storage;
            _pool = new ObjectPool<GameObject>(Create, OnGet, OnRelease, Object.Destroy);
        }

        public GameObject Get(Transform parent)
        {
            GameObject part = _pool.Get();
            part.transform.SetParent(parent, false);
            return part;
        }

        public void Release(GameObject part) => _pool.Release(part);

        GameObject Create() => Object.Instantiate(_prefab, _storage);

        void OnGet(GameObject part) => part.SetActive(true);

        void OnRelease(GameObject part)
        {
            part.SetActive(false);
            part.transform.SetParent(_storage, false);
        }
    }
}
