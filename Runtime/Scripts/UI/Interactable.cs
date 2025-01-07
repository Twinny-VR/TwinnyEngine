using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twinny.UI
{
    public class Interactable : MonoBehaviour
    {
        #region Fields
        [SerializeField] private MeshRenderer[] _highLightMeshes;
        [ColorUsage(true, true)]
        [SerializeField] private Color _normalColor; // Cor de destaque
        [ColorUsage(true, true)]
        [SerializeField] private Color _highLightColor; // Cor de destaque
        [SerializeField] private float _transitionTime = 2f; // Tempo de transi��o

        private float _transitionProgress = 0f; // Progresso da transi��o (0 a 1)
        [SerializeField]
        private bool _isHighLight = false; // Flag de highlight
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            // Garante que os materiais tenham a palavra-chave "_EMISSION" ativada
            foreach (var mesh in _highLightMeshes)
            {
                if (mesh != null)
                {
                    mesh.material.EnableKeyword("_EMISSION");
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            // Se o Highlight est� ativado
            if (_isHighLight)
            {
                // Aumenta o progresso para a cor emissiva
                _transitionProgress += Time.deltaTime / _transitionTime;
            }
            else
            {
                // Diminui o progresso de volta para a cor normal
                _transitionProgress -= Time.deltaTime / _transitionTime;
            }

            // Limita o progresso entre 0 e 1
            _transitionProgress = Mathf.Clamp01(_transitionProgress);

            // Aplica a interpola��o de cor
            for (int i = 0; i < _highLightMeshes.Length; i++)
            {
                if (_highLightMeshes[i] != null)
                {
                    // Se for Highlight, vai para a cor emissiva, sen�o volta para a cor padr�o (sem emiss�o)
                    Color targetColor = _isHighLight ? _highLightColor : _normalColor;

                    // Realiza a interpola��o suave para a cor de emiss�o
                    Color newEmissionColor = Color.Lerp(_highLightMeshes[i].material.GetColor("_EmissionColor"), targetColor, _transitionProgress);
                    _highLightMeshes[i].material.SetColor("_EmissionColor", newEmissionColor);

                    // Desativa a emiss�o caso a cor de transi��o esteja de volta ao normal
                    if (_transitionProgress == 0f)
                    {
                        _highLightMeshes[i].material.DisableKeyword("_EMISSION");
                    }
                    else
                    {
                        _highLightMeshes[i].material.EnableKeyword("_EMISSION");
                    }
                }
            }
        }

        // Fun��o para alternar o Highlight via Menu de Contexto no Inspector
        [ContextMenu("HIGHLIGHT")]
        private void SetHighLight2()
        {
            SetHighLight(!_isHighLight);
        }

        // Fun��o para ativar ou desativar o highlight
        public void SetHighLight(bool active = true)
        {
           // _transitionProgress = active ? _transitionProgress : 1f; // Se for falso, come�a do �ltimo valor de transi��o
            _isHighLight = active;
        }
    }
}
