using UnityEngine;
using UnityEngine.UI;

namespace Concept.UI
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasRenderer))]
    public class NetworkImage : Image
    {
        public string imageUrl;
        protected override void Start()
        {
            base.Start();

        }
    }
}
