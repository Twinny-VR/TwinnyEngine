#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEngine;

namespace Twinny.XR
{
    [CustomEditor(typeof(VRTargets))]
    public class VRTargetsEditor : UnityEditor.Editor
    {
        private Transform referenceRoot;

        public override void OnInspectorGUI()
        {
            VRTargets vrTargetsScript = (VRTargets)target;

            // Campo para arrastar o Transform pai
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Auto-Fill Settings", EditorStyles.boldLabel);
            referenceRoot = (Transform)EditorGUILayout.ObjectField("Reference Root", referenceRoot, typeof(Transform), true);

            if (GUILayout.Button("Auto-Fill Targets From Reference Root"))
            {
                if(referenceRoot == null) referenceRoot = FindFirstObjectByType<OVRManager>().transform;

                if (referenceRoot == null)
                {
                    Debug.LogWarning("No OVRManager found.");
                    return;
                }

                AutoFill(vrTargetsScript, referenceRoot);
                EditorUtility.SetDirty(vrTargetsScript);
                Debug.Log("VR Targets auto-filled based on Reference Root.");
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // Exibe os campos padrão do inspector
            base.OnInspectorGUI();
        }

        public static bool GetXRPlugin(string loaderName)
        {
            // Tenta pegar o XRGeneralSettings do build atual
            var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);
            if (generalSettings == null)
            {
                Debug.Log("⚠️ XR General Settings não foi encontrado.");
                return false;
            }

            if (generalSettings.AssignedSettings == null)
            {
                Debug.Log("⚠️ Nenhum XR Manager foi atribuído.");
                return false;
            }

            var managerSettings = generalSettings.AssignedSettings;
            if (!managerSettings.activeLoaders.Any())
            {
                Debug.Log("ℹ️ XR Management está instalado, mas nenhum loader (plugin) está configurado.");
                return false;
            }

            foreach (var loader in managerSettings.activeLoaders)
            {
                if (loader != null && loader.name == loaderName)
                {
                    return true;
                }
            }

            return false;
        }

        private OVRHand GetHand(Transform root, string side)
        {
            return root.GetComponentsInChildren<OVRHand>().FirstOrDefault(hand => hand.name.IndexOf(side, StringComparison.OrdinalIgnoreCase) >= 0);

        }

        private void AutoFill(VRTargets vrTargets, Transform root)
        {
            Transform Find(string name) => root.FindDeepChild(name);

            if (GetXRPlugin("OculusLoader"))
            {
                Transform leftHand = Find("OculusHand_L");
                Transform rightHand = Find("OculusHand_R");




                if (leftHand == null || rightHand == null)
                {
                    Debug.LogError("Oculus Hands not found!");
                    return;
                }

                Transform leftTarget = null;
                    leftTarget = leftHand.Find("Left Hand VR Target");
                    if(leftTarget == null) leftTarget = new GameObject("Left Hand VR Target").transform;
                    leftTarget.SetParent(leftHand);
                    leftTarget.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                Transform rightTarget = null;
                    rightTarget = rightHand.Find("Right Hand VR Target");
                    if(rightTarget == null) rightTarget = new GameObject("Right Hand VR Target").transform;
                    rightTarget.SetParent(rightHand);
                    rightTarget.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

                var t = new VRTargets.Targets
                {
                    leftHandOVR = GetHand(root, "Left"),
                    rightHandOVR = GetHand(root, "Right"),

                    head = Find("CenterEyeAnchor"),

                    leftHand = leftTarget,
                    leftHandIndex = Find("b_l_index3"),
                    leftHandMiddle = Find("b_l_middle3"),
                    leftHandRing = Find("b_l_ring3"),
                    leftHandPink = Find("b_l_pinky3"),
                    leftHandThumb = Find("b_l_thumb3"),

                    rightHand = rightTarget,
                    rightHandIndex = Find("b_r_index3"),
                    rightHandMiddle = Find("b_r_middle3"),
                    rightHandRing = Find("b_r_ring3"),
                    rightHandPink = Find("b_r_pinky3"),
                    rightHandThumb = Find("b_r_thumb3")
                };
                vrTargets.targets = t;
            }
            else
            if (GetXRPlugin("OpenXRLoader"))
            {

                Transform leftHand = Find("OpenXRLeftHand");
                Transform rightHand = Find("OpenXRRightHand");

                if(leftHand == null || rightHand == null)
                {
                    Debug.LogError("OpenXR Hands not found!");
                    vrTargets.targets = null;
                    return;
                }

                Transform leftTarget = null;
                    leftTarget = leftHand.Find("Left Hand VR Target");
                    if (leftTarget == null) leftTarget = new GameObject("Left Hand VR Target").transform;
                    leftTarget.SetParent(leftHand);
                    leftTarget.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                Transform rightTarget = null;
                    rightTarget = rightHand.Find("Right Hand VR Target");
                    if (rightTarget == null) rightTarget = new GameObject("Right Hand VR Target").transform;
                    rightTarget.SetParent(rightHand);
                    rightTarget.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

                var t = new VRTargets.Targets
                {

                    leftHandOVR = GetHand(root, "Left"),
                    rightHandOVR = GetHand(root, "Right"),

                    head = Find("CenterEyeAnchor"),

                    leftHand = leftTarget,
                    leftHandIndex = leftHand.FindDeepChild("XRHand_IndexDistal"),
                    leftHandMiddle = leftHand.FindDeepChild("XRHand_MiddleDistal"),
                    leftHandRing = leftHand.FindDeepChild("XRHand_RingDistal"),
                    leftHandPink = leftHand.FindDeepChild("XRHand_LittleDistal"),
                    leftHandThumb = leftHand.FindDeepChild("XRHand_ThumbDistal"),

                    rightHand = rightTarget,
                    rightHandIndex = rightHand.FindDeepChild("XRHand_IndexDistal"),
                    rightHandMiddle = rightHand.FindDeepChild("XRHand_MiddleDistal"),
                    rightHandRing = rightHand.FindDeepChild("XRHand_RingDistal"),
                    rightHandPink = rightHand.FindDeepChild("XRHand_LittleDistal"),
                    rightHandThumb = rightHand.FindDeepChild("XRHand_ThumbDistal")
                };
                vrTargets.targets = t;

            }
        }

    }

    /// <summary>
    /// Extensão para busca recursiva de um filho pelo nome.
    /// </summary>
    public static class TransformExtensions
    {
        public static Transform FindDeepChild(this Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;

                Transform result = child.FindDeepChild(name);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
#endif