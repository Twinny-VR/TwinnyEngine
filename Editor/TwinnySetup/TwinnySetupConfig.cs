using UnityEngine;
using UnityEngine.UIElements;

namespace Twinny.Editor
{
    [CreateAssetMenu(menuName = "Twinny/Setup Config")]
    public class TwinnySetupConfig : ScriptableObject
    {
        public VisualTreeAsset visualTreeAsset;
        public VisualTreeAsset sideBarElement;
        public VisualTreeAsset[] sections;
    }
}
