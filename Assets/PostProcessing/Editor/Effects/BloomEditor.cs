using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.PostProcessing;
using UnityEditor.Rendering.PostProcessing;

namespace UnityEditor.Rendering.PostProcessing
{
    [PostProcessEditor(typeof(Bloom))]
    internal sealed class BloomEditor : PostProcessEffectEditor<Bloom>
    {
        SerializedParameterOverride m_Intensity, m_Threshold, m_SoftKnee, m_Clamp, m_Diffusion, m_AnamorphicRatio, m_Color, m_FastMode;
        SerializedParameterOverride m_DirtTexture, m_DirtIntensity, m_Downscale;

        public override void OnEnable()
        {
            m_Intensity = FindParameterOverride(x => x.intensity);
            m_Threshold = FindParameterOverride(x => x.threshold);
            m_SoftKnee = FindParameterOverride(x => x.softKnee);
            m_Clamp = FindParameterOverride(x => x.clamp);
            m_Diffusion = FindParameterOverride(x => x.diffusion);
            m_AnamorphicRatio = FindParameterOverride(x => x.anamorphicRatio);
            m_Color = FindParameterOverride(x => x.color);
            m_FastMode = FindParameterOverride(x => x.fastMode);
            m_DirtTexture = FindParameterOverride(x => x.dirtTexture);
            m_DirtIntensity = FindParameterOverride(x => x.dirtIntensity);
            m_Downscale = FindParameterOverride(x => x.downscale);
        }

        public override void OnInspectorGUI()
        {
            EditorUtilities.DrawHeaderLabel("Bloom");

            PropertyField(m_Intensity);
            PropertyField(m_Threshold);
            PropertyField(m_SoftKnee);
            PropertyField(m_Clamp);
            PropertyField(m_Diffusion);
            PropertyField(m_AnamorphicRatio);
            PropertyField(m_Color);
            PropertyField(m_FastMode);

            if (m_FastMode.overrideState.boolValue && !m_FastMode.value.boolValue && EditorUtilities.isTargetingConsolesOrMobiles)
                EditorGUILayout.HelpBox("For performance reasons it is recommended to use Fast Mode on mobile and console platforms.", MessageType.Warning);

            EditorGUILayout.Space();
            EditorUtilities.DrawHeaderLabel("Dirtiness");
            PropertyField(m_DirtTexture);
            PropertyField(m_DirtIntensity);

            EditorGUILayout.Space();
            EditorUtilities.DrawHeaderLabel("Performance");

            // allowed downscale options
            int[] values = new int[] { 1, 2, 4, 8 };
            string[] labels = new string[] { "1x (Full)", "2x (Half)", "4x (Quarter)", "8x (Eighth)" };

            EditorGUILayout.BeginHorizontal();
            bool ov = EditorGUILayout.ToggleLeft("Override", m_Downscale.overrideState.boolValue, GUILayout.Width(70f));
            if (ov != m_Downscale.overrideState.boolValue) m_Downscale.overrideState.boolValue = ov;

            EditorGUI.BeginDisabledGroup(!m_Downscale.overrideState.boolValue);
            int current = m_Downscale.value.intValue;
            int idx = 0;
            for (int i = 0; i < values.Length; i++) if (values[i] == current) { idx = i; break; }
            int sel = EditorGUILayout.Popup(idx, labels);
            m_Downscale.value.intValue = values[Mathf.Clamp(sel, 0, values.Length - 1)];
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            if (RuntimeUtilities.isVREnabled)
            {
                if ((m_DirtIntensity.overrideState.boolValue && m_DirtIntensity.value.floatValue > 0f)
                 || (m_DirtTexture.overrideState.boolValue && m_DirtTexture.value.objectReferenceValue != null))
                    EditorGUILayout.HelpBox("Using a dirt texture in VR is not recommended.", MessageType.Warning);
            }
        }
    }
}
