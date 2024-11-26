using UnityEngine.Events;

namespace PlateauToolkit.Sandbox.Runtime
{
    public class PlateauSandboxBulkPlaceHierarchyItem
    {
        public int ID { get; set; }
        public string CategoryName { get; set; }
        public int Count { get; set; }
        public string PrefabName { get; set; } = string.Empty;
        public int PrefabConstantID { get; set; } = -1;
        public UnityEvent OnClicked { get; private set; } = new UnityEvent();
    }
}