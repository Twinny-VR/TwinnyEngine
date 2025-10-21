#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Twinny.GamePlay
{
    [CustomEditor(typeof(CustomSplineAnimate))]
    [CanEditMultipleObjects]
    public class CustomSplineAnimateEditor : UnityEditor.Editor
    {
        CustomSplineAnimate m_SplineAnimate;
        SerializedObject m_TransformSO;

        void OnEnable()
        {
            m_SplineAnimate = target as CustomSplineAnimate;
            if (m_SplineAnimate == null)
                return;

            m_TransformSO = new SerializedObject(m_SplineAnimate.transform);

            m_SplineAnimate.Updated += OnSplineAnimateUpdated;
        }

        void OnDisable()
        {
            if (m_SplineAnimate != null)
                m_SplineAnimate.Updated -= OnSplineAnimateUpdated;
        }

        public override void OnInspectorGUI()
        {
            // Desenha o inspetor padrão
            DrawDefaultInspector();

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Editor Controls", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();

            GUI.enabled = !m_SplineAnimate.IsPlaying;
            if (GUILayout.Button("▶ Play"))
            {
                OnPlayClicked();
            }

            GUI.enabled = m_SplineAnimate.IsPlaying;
            if (GUILayout.Button("⏸ Pause"))
            {
                OnPauseClicked();
            }

            GUI.enabled = true;
            if (GUILayout.Button("⏹ Reset"))
            {
                OnResetClicked();
            }

            GUILayout.EndHorizontal();
        }


        void OnPlayClicked()
        {
            if (!m_SplineAnimate.IsPlaying)
            {
                m_SplineAnimate.RecalculateAnimationParameters();
                if (m_SplineAnimate.NormalizedTime == 1f)
                    m_SplineAnimate.Restart(true);
                else
                    m_SplineAnimate.Play();
            }
        }

        void OnPauseClicked()
        {
            m_SplineAnimate.Pause();
        }

        void OnResetClicked()
        {
            m_SplineAnimate.RecalculateAnimationParameters();
            m_SplineAnimate.Restart(false);
           // RefreshProgressFields();
        }


        void OnSplineAnimateUpdated(Vector3 position, Quaternion rotation)
        {
            if (m_SplineAnimate == null)
                return;

            if (!EditorApplication.isPlaying)
            {
                m_TransformSO.Update();

                var localPosition = position;
                var localRotation = rotation;
                if (m_SplineAnimate.transform.parent != null)
                {
                    localPosition = m_SplineAnimate.transform.parent.worldToLocalMatrix.MultiplyPoint3x4(position);
                    localRotation = Quaternion.Inverse(m_SplineAnimate.transform.parent.rotation) * localRotation;
                }

                m_TransformSO.FindProperty("m_LocalPosition").vector3Value = localPosition;
                m_TransformSO.FindProperty("m_LocalRotation").quaternionValue = localRotation;

                m_TransformSO.ApplyModifiedProperties();
            }
        }
    }
}
#endif