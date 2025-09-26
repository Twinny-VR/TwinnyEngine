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


        public enum ButtonType
        {
            START,
            QUIT,
            SETTINGS,
            CHANGE_SCENE,
            NAVIGATION,
            ACTION,
            RESET
        }


        private const string USSClassName = "twinny-card-element";

        [UxmlAttribute("Title")]
        public string title { get => m_titleLabel.text; set => m_titleLabel.text = value; }

        [UxmlAttribute("Description")]
        public string description { get => m_descLabel.text; set => m_descLabel.text = value; }

        private Texture2D m_background;

        [UxmlAttribute("Background")]
        public Texture2D background
        {
            get
            {
                return m_background;
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

        private Label m_titleLabel;
        private Label m_descLabel;
        private AspectElement m_backgroundElement;

        [UxmlAttribute]
        public ButtonType buttonType { get; set; } = ButtonType.START;


        [UxmlAttribute]
        public string stringParameter { get; set; }

        [UxmlAttribute]
        public int integerParameter{ get; set; }


        public event Action OnClickEvent;

        public CardElement() {

            var visualTree = Resources.Load<VisualTreeAsset>("CardElement");
            if (visualTree == null)
            {
                Debug.LogError("CardElement not found in Resources folder!");
                return;
            }

            var treeInstance = visualTree.CloneTree();


            var cardElement = treeInstance.Q<VisualElement>("Card");
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



            m_titleLabel = this.Q<Label>("TitleLabel");
            m_descLabel = this.Q<Label>("DescLabel");
            m_backgroundElement = this.Q<AspectElement>();
            m_backgroundElement.RegisterCallback<ClickEvent>(evt =>
            {
                OnClick();
                OnClickEvent?.Invoke();
            });

            AddToClassList(USSClassName);
            styleSheets.Add(Resources.Load<StyleSheet>(GetType().Name + "Styles"));

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);

        }


        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            SetBackground(m_background);
        }

        public void SetBackground(Texture2D texture) {

            if (texture == null)
            {
                Texture2D defaultTexture = Resources.Load<Texture2D>("Images/CardItem_PlaceHolder");
                m_backgroundElement.style.backgroundImage = defaultTexture? new StyleBackground(defaultTexture) : StyleKeyword.Null;
                return;
            }
                m_backgroundElement.style.backgroundImage = new StyleBackground(texture);

        }

        private void OnClick()
        {

            if (CanvasTransition.isTransitioning)
                return;
            switch (buttonType)
            {
                case ButtonType.START:
                    break;
                case ButtonType.SETTINGS:
                    break;
                case ButtonType.CHANGE_SCENE:
                    _ = LevelManager.Instance.ChangeScene(stringParameter, integerParameter);
                    break;
                case ButtonType.NAVIGATION:
                    LevelManager.NavigateTo(integerParameter);
                    break;
                case ButtonType.RESET:
                    _ = LevelManager.Instance.ResetExperience();
                    break;
                case ButtonType.QUIT:
                    Application.Quit();
                    break;
                case ButtonType.ACTION:
                    ActionManager.CallAction(stringParameter);
                    break;
            }
        }
    }

}