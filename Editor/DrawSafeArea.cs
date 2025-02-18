#if UNITY_EDITOR
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Twinny.System;

namespace Twinny.Editor
{

    [CustomEditor(typeof(LandMarkNode))]
    public class DrawSafeArea : UnityEditor.Editor
    {

        private void OnSceneGUI()
        {
            // Verifica se o objeto tem o componente LandMarkNode
            LandMarkNode landMarkNode = (LandMarkNode)target;

            if (landMarkNode != null)
            {
                // Posi��o no ch�o (ajustando o Y para 0)
                Vector3 position = landMarkNode.transform.position;
                position.y = 0f;

                // Tamanho do ret�ngulo
                Vector3 size = new Vector3(2.5f, 0f, 1.5f);

                // Cor do gizmo
                Handles.color = Color.red;

                // Desenhando os cantos arredondados (com 0.2f de raio nos cantos)
                float radius = 0.1f;

                // C�lculo das posi��es dos quatro cantos
                Vector3[] corners = new Vector3[4];
                corners[0] = position + new Vector3(-size.x / 2, 0f, -size.z / 2); // canto inferior esquerdo
                corners[1] = position + new Vector3(size.x / 2, 0f, -size.z / 2);  // canto inferior direito
                corners[2] = position + new Vector3(-size.x / 2, 0f, size.z / 2);  // canto superior esquerdo
                corners[3] = position + new Vector3(size.x / 2, 0f, size.z / 2);   // canto superior direito
                Vector3 offset = new Vector3(0f, 0f, .25f);
                // Desenhando os arcos nos 4 cantos, ajustando as rota��es corretamente
                // Canto inferior esquerdo (Rota��o 90� no sentido anti-hor�rio)
                Handles.DrawWireArc(corners[0] + new Vector3(radius, 0f, .1f), Vector3.up, Vector3.left, -90f, radius);
                // Canto inferior direito (Rota��o 90� no sentido hor�rio)
                Handles.DrawWireArc(corners[1] + new Vector3(-radius, 0f, .1f), Vector3.up, Vector3.right, 90f, radius);
                // Canto superior esquerdo (Rota��o -90� no sentido anti-hor�rio)
                Handles.DrawWireArc(corners[2] + new Vector3(radius, 0f, -.1f), Vector3.up, Vector3.left, 90f, radius);
                // Canto superior direito (Rota��o -90� no sentido hor�rio)
                Handles.DrawWireArc(corners[3] + new Vector3(-radius, 0f, -.1f), Vector3.up, Vector3.right, -90f, radius);

                // Desenhando as linhas retas do ret�ngulo, ajustando para as bordas arredondadas
                Handles.DrawLine(corners[0] + new Vector3(radius, 0f, 0f), corners[1] - new Vector3(radius, 0f, 0f)); // Linha inferior
                Handles.DrawLine(corners[1] + new Vector3(0f, 0f, radius), corners[3] - new Vector3(0f, 0f, radius)); // Linha direita
                Handles.DrawLine(corners[3] - new Vector3(radius, 0f, 0f), corners[2] + new Vector3(radius, 0f, 0f)); // Linha superior
                Handles.DrawLine(corners[2] - new Vector3(0f, 0f, radius), corners[0] + new Vector3(0f, 0f, radius)); // Linha esquerda

            }
        }

    }
}
#endif