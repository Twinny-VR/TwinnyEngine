using System.Collections.Generic;
using System.Linq;
using Twinny.UI;
using UnityEditor;
using UnityEngine;

namespace Twinny.System
{


    public class CutoffOcclusion : MonoBehaviour
    {
        private const string LAYER_NAME = "CutoffOccludee";

        private List<Transform> m_childs;
        private void Start()
        {

            int layer = LayerMask.NameToLayer(LAYER_NAME);


                m_childs = new List<Transform>();

                // pega todos os filhos, ativos e inativos
                Transform[] allChildren = transform.GetComponentsInChildren<Transform>(true);

                foreach (var child in allChildren)
                {
                    if (child.gameObject.layer == layer)
                    {
                        m_childs.Add(child);
                    }
                }


        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            int layer = LayerMask.NameToLayer(LAYER_NAME);
            if (layer == -1)
            {
                Debug.LogWarning($"Layer '{LAYER_NAME}' n�o existe!");
                RegisterLayer(LAYER_NAME);
            }

        }

#endif

        private void OnEnable()
        {
            MainInterface.OnCutoffChanged += OnCutoffChanged;
        }

        private void OnDisable()
        {
            MainInterface.OnCutoffChanged -= OnCutoffChanged;
        }

        private void OnCutoffChanged(float value)
        {
            foreach (var item in m_childs)
            {
                if (item == transform) continue;
                bool active = item.position.y < value;
                item.gameObject.SetActive(active);
            }

        }




        private void RegisterLayer(string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                Debug.LogWarning("O nome da layer n�o pode ser vazio!");
                return;
            }

            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = tagManager.FindProperty("layers");

            // Checa se a layer j� existe
            for (int i = 0; i < layersProp.arraySize; i++)
            {
                SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);
                if (sp.stringValue == layerName)
                {
                    Debug.Log($"Layer '{layerName}' j� existe no �ndice {i}.");
                    return; // Sai do m�todo, sem duplicar
                }
            }

            // Procura a primeira posi��o vazia a partir do �ndice 8
            for (int i = 8; i < layersProp.arraySize; i++)
            {
                SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(sp.stringValue))
                {
                    sp.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    Debug.Log($"Layer '{layerName}' criada no �ndice {i}.");
                    return;
                }
            }

            Debug.LogWarning("N�o foi poss�vel criar a layer. N�o h� slots vazios dispon�veis.");
        }

    }

}