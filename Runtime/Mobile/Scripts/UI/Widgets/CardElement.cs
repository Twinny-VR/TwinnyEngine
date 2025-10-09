using UnityEngine;
using UnityEngine.UIElements;
using Concept.UI;
using System;
using Concept.Helpers;
using System.Data.Common;
using Twinny.System;

namespace Twinny.UI
{

    [UxmlElement]
    public partial class CardElement : VisualElement
    {
        private const string USSClassName = "twinny-card-element";

        [UxmlAttribute("title")]
        public string title { get => m_titleLabel.text; set => m_titleLabel.text = value; }

        [UxmlAttribute("description")]
        public string description { get => m_descLabel.text; set => m_descLabel.text = value; }

        private Texture2D m_background;

        [UxmlAttribute("background")]
        public Texture2D thumbnail
        {
            get
            {
               // return m_background;
                // Tenta obter do estilo resolvido
                var backgroundImage = m_backgroundElement.resolvedStyle.backgroundImage;
                if (backgroundImage != null)
                {
                    return backgroundImage.texture;
                }
                return null;
            }
            set
            {
                m_background = value;
                SetBackground(value);
            }
        }

        private bool m_expanded = true;
        [UxmlAttribute("expanded-info")]
        public bool expanded
        {
            get => m_expanded;

            set
            {
                m_expanded = value;
                m_footer.EnableInClassList("expanded", value);
                m_footer.style.display = DisplayStyle.Flex;

            }
        }

        private Label m_titleLabel;
        private Label m_descLabel;
        protected AspectElement m_backgroundElement;
        private VisualElement m_footer;

        [UxmlAttribute]
        public string stringParameter { get; set; }

        [UxmlAttribute]
        public int integerParameter { get; set; }


        public event Action OnClickEvent;

        public CardElement()
        {

            var visualTree = Resources.Load<VisualTreeAsset>("CardElement");
            if (visualTree == null)
            {
                Debug.LogError("CardElement not found in Resources folder!");
                return;
            }

            var treeInstance = visualTree.CloneTree();


            var cardElement = treeInstance.Q<VisualElement>().GetFirstOfType<AspectElement>();
            if (cardElement != null)
            {
                // Move todos os filhos do Card para este elemento
                while (cardElement.childCount > 0)
                {
                    var child = cardElement[0];
                    cardElement.Remove(child);
                    this.Add(child);
                }

                // Copia classes do Card
                this.ClearClassList();
                foreach (string className in cardElement.GetClasses())
                    this.AddToClassList(className);

                // NÃO precisa tentar copiar style, o USS já vai aplicar
            }
            else
            {
                visualTree.CloneTree(this);
            }


            m_footer = this.Q<VisualElement>("Footer");
            m_titleLabel = this.Q<Label>("TitleLabel");
            m_descLabel = this.Q<Label>("DescLabel");

            m_backgroundElement = this.Q<AspectElement>();
            m_backgroundElement.RegisterCallback<ClickEvent>(evt =>
            {
                OnClickEvent?.Invoke();
            });

            AddToClassList(USSClassName);
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);

            var styleName = GetType().Name + "Styles";
            var styleSheet = Resources.Load<StyleSheet>(styleName);

            if(styleSheet == null)
            {
                styleName = GetType().BaseType.Name + "Styles";
                styleSheet = Resources.Load<StyleSheet>(styleName);
            }

            if(styleSheet != null) styleSheets.Add(styleSheet);


        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            //Hide footer if starts false
            m_footer.style.display = (expanded) ? DisplayStyle.Flex : DisplayStyle.None;
            SetBackground(m_background);
        }


        private bool _backgroundApplied = false;

        public void SetBackground(Texture2D texture)
        {
            if (m_backgroundElement == null)
                return;

            if (float.IsNaN(m_backgroundElement.resolvedStyle.width) || float.IsNaN(m_backgroundElement.resolvedStyle.height))
            {
                // A geometria ainda não foi resolvida, espera o evento
                m_backgroundElement.RegisterCallback<GeometryChangedEvent>((evt) =>
                {
                    if (!_backgroundApplied) // evita aplicar múltiplas vezes
                    {
                        ApplyBackground(texture);
                        _backgroundApplied = true;
                    }
                });
                return;
            }

            ApplyBackground(texture);
        }

        private void ApplyBackground(Texture2D texture)
        {
            if (texture == null)
            {
                Texture2D defaultTexture = Resources.Load<Texture2D>("Images/CardItem_PlaceHolder");
                m_backgroundElement.style.backgroundImage = defaultTexture != null
                    ? defaultTexture
                    : StyleKeyword.Null;
                return;
            }

            m_backgroundElement.style.backgroundImage = CreateRuntimeTexture(texture);

        }

        Texture2D CreateRuntimeTexture(Texture2D src)
        {
            try
            {
                Texture2D tex = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
                tex.SetPixels(src.GetPixels());
                tex.Apply();
                return tex;
            }
            catch (Exception)
            {

                return null;
            }

        }


        public void SetBackgroundOLD(Texture2D texture)
        {

            if (texture == null)
            {
                Texture2D defaultTexture = Resources.Load<Texture2D>("Images/CardItem_PlaceHolder");
                m_backgroundElement.style.backgroundImage = defaultTexture ? new StyleBackground(defaultTexture) : StyleKeyword.Null;
                return;
            }
            m_backgroundElement.style.backgroundImage = new StyleBackground(texture);

        }
       
    }

}