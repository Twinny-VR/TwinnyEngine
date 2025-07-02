using Twinny.System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Twinny.Editor
{
    [UxmlElement("Section")]
    public partial class Section : VisualElement
    {
        [UxmlAttribute("section-platform")] public Platform sectionPlatform { get; set; }
        [UxmlAttribute("section-name")] public string sectionName { get; set; }
        [UxmlAttribute("section-title")] public string sectionTitle { get; set; }

        [UxmlAttribute("section-icon")] public Sprite sectionIcon { get; set; }

        public VisualElement sectionButton;
        public VisualTreeAsset sectionLayout;

        public Section() { }

        }



}
