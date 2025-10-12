using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class AnimationSimplifier : EditorWindow
{
    private AnimationClip sourceClip;
    private bool overwriteOriginal = false;
    private bool preserveKeyframesForUnity2017 = false;

    private enum SimplifyMode { Lossless, High, Normal, Low }
    private SimplifyMode mode = SimplifyMode.Normal;

    [MenuItem("Tools/Animation Simplifier")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AnimationSimplifier), false, "Animation Simplifier");
    }

    void OnGUI()
    {
        GUILayout.Label("Simplify AnimationClip", EditorStyles.boldLabel);
        sourceClip = (AnimationClip)EditorGUILayout.ObjectField("Source Clip", sourceClip, typeof(AnimationClip), false);
        mode = (SimplifyMode)EditorGUILayout.EnumPopup("Simplify Mode", mode);
        preserveKeyframesForUnity2017 = EditorGUILayout.Toggle("Preserve Keyframes (Unity 2017)", preserveKeyframesForUnity2017);
        overwriteOriginal = EditorGUILayout.Toggle("Overwrite Original", overwriteOriginal);

        if (GUILayout.Button("Simplify"))
        {
            Object[] targets = Selection.GetFiltered(typeof(AnimationClip), SelectionMode.Assets);
            if (sourceClip != null)
            {
                ProcessClip(sourceClip);
            }
            foreach (Object obj in targets)
            {
                AnimationClip clip = obj as AnimationClip;
                if (clip != null && clip != sourceClip)
                {
                    ProcessClip(clip);
                }
            }
        }
    }

    private void ProcessClip(AnimationClip clip)
    {
        string assetPath = AssetDatabase.GetAssetPath(clip);
        if (string.IsNullOrEmpty(assetPath)) return;

        AnimationClip newClip = Object.Instantiate(clip);
        newClip.name = clip.name + "_Simplified";

        var bindings = AnimationUtility.GetCurveBindings(newClip);
        foreach (var binding in bindings)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(newClip, binding);
            AnimationCurve simplified = SimplifyCurve(curve);
            AnimationUtility.SetEditorCurve(newClip, binding, simplified);
        }

        if (overwriteOriginal)
        {
            string tempPath = assetPath.Replace(".anim", "_temp.anim");
            AssetDatabase.CreateAsset(newClip, tempPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.DeleteAsset(assetPath);
            File.Move(tempPath, assetPath);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            Debug.Log("Animation overwritten: " + assetPath);
        }
        else
        {
            int lastSlash = assetPath.LastIndexOf('/');
            string folder = (lastSlash >= 0) ? assetPath.Substring(0, lastSlash) : "Assets";
            string newPath = folder + "/" + newClip.name + ".anim";
            AssetDatabase.CreateAsset(newClip, newPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Animation saved to: " + newPath);
        }
    }

    private AnimationCurve SimplifyCurve(AnimationCurve curve)
    {
        if (curve == null || curve.length < 2)
            return curve;

        List<Keyframe> keys = new List<Keyframe>(curve.keys);
        List<Keyframe> result = new List<Keyframe>();

        for (int i = 0; i < keys.Count; i++)
        {
            Keyframe current = keys[i];

            // Lossless: remove constant segments
            if (mode == SimplifyMode.Lossless && i > 0 && Mathf.Approximately(current.value, keys[i - 1].value))
                continue;

            // High/Normal/Low: remove near-duplicates
            if (i > 0 && mode != SimplifyMode.Lossless && !preserveKeyframesForUnity2017)
            {
                float timeDelta = current.time - keys[i - 1].time;
                float valueDelta = Mathf.Abs(current.value - keys[i - 1].value);

                float timeThreshold = (mode == SimplifyMode.High) ? 0.01f : (mode == SimplifyMode.Normal) ? 0.05f : 0.1f;
                float valueThreshold = (mode == SimplifyMode.High) ? 0.001f : (mode == SimplifyMode.Normal) ? 0.01f : 0.05f;

                if (timeDelta < timeThreshold && valueDelta < valueThreshold)
                    continue;
            }

            result.Add(current);
        }

        AnimationCurve simplified = new AnimationCurve(result.ToArray());

        if (!preserveKeyframesForUnity2017)
        {
            for (int i = 0; i < simplified.length; i++)
            {
                simplified.SmoothTangents(i, 0);
            }
        }

        return simplified;
    }
}
