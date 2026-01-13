using UnityEngine;

namespace Core.Maps
{
    [CreateAssetMenu(fileName = "GameElementDefinition", menuName = "Maps/Game Element Definition")]
    public sealed class GameElementDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private GameObject prefab;
        [SerializeField] private Vector3 localOffset;
        [SerializeField] private Vector3 localEulerAngles;
        [SerializeField] private Vector3 localScale = Vector3.one;

        public string Id => id;
        public GameObject Prefab => prefab;
        public Vector3 LocalOffset => localOffset;
        public Vector3 LocalEulerAngles => localEulerAngles;
        public Vector3 LocalScale => localScale;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = name;
            }

            if (localScale == Vector3.zero)
            {
                localScale = Vector3.one;
            }
        }
    }
}
