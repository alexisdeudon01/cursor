using UnityEngine;

namespace Core.Maps
{
    [CreateAssetMenu(fileName = "GameElementLibrary", menuName = "Maps/Game Element Library")]
    public sealed class GameElementLibrary : ScriptableObject
    {
        [SerializeField] private GameElementDefinition[] definitions;

        public GameElementDefinition GetById(string id)
        {
            if (string.IsNullOrEmpty(id) || definitions == null)
            {
                return null;
            }

            for (int i = 0; i < definitions.Length; i++)
            {
                var definition = definitions[i];
                if (definition != null && definition.Id == id)
                {
                    return definition;
                }
            }

            return null;
        }
    }
}
