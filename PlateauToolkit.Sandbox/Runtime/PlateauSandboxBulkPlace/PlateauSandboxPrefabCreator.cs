using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime
{
    public class PlateauSandboxPrefabCreator : MonoBehaviour
    {
        public GameObject CreatePrefab(string prefabName, GameObject prefab, Vector3 position, GameObject parent = null)
        {
            var parentTransform = parent != null ? parent.transform : this.transform;
            GameObject go = Instantiate(prefab, position, Quaternion.identity);
            go.name = prefabName;
            go.transform.SetParent(parentTransform);

            return go;
        }
    }
}