using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class UIAnimatorConverter
{
    [MenuItem("Tools/Convert Selected to UIAnimator")]
    private static void ConvertSelected()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            ConvertGameObject(go);
        }
    }

    [MenuItem("Tools/Convert Selected to UIAnimator", true)]
    private static bool ValidateConvertSelected()
    {
        return Selection.gameObjects.Length > 0;
    }

    private static void ConvertGameObject(GameObject go)
    {
        Animation legacyAnimation = go.GetComponent<Animation>();
        if (legacyAnimation == null)
        {
            Debug.LogWarning($"No legacy Animation component found on '{go.name}'. Skipping.");
            return;
        }

        AnimationClip[] clips = AnimationUtility.GetAnimationClips(legacyAnimation);
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning($"No animation clips found on '{go.name}'. Skipping.");
            return;
        }

        // Convert only the first clip (you can extend to handle multiple clips)
        AnimationClip clip = clips[0];
        Debug.Log($"Converting clip '{clip.name}' on '{go.name}' to UIAnimator (frame rate: {clip.frameRate} fps).");

        List<TransformKeyframe> keyframes = ExtractKeyframesFromClip(go, clip);

        if (keyframes.Count == 0)
        {
            Debug.LogWarning($"Clip '{clip.name}' contains no position/scale data. Skipping.");
            return;
        }

        UIAnimator anim = go.GetComponent<UIAnimator>();
        if (anim == null)
            anim = go.AddComponent<UIAnimator>();

        anim.keyframes = keyframes;
        anim.playOnAwake = legacyAnimation.playAutomatically;
        anim.loop = (clip.wrapMode == WrapMode.Loop);

        // Uncomment to automatically disable the legacy Animation component
        // legacyAnimation.enabled = false;

        EditorUtility.SetDirty(anim);
        EditorUtility.SetDirty(go);
        Debug.Log($"Converted '{clip.name}' on '{go.name}' successfully with {keyframes.Count} keyframes.");
    }

    private static List<TransformKeyframe> ExtractKeyframesFromClip(GameObject go, AnimationClip clip)
    {
        // Extract position and scale curves
        AnimationCurve posX = null, posY = null, posZ = null;
        AnimationCurve scaleX = null, scaleY = null, scaleZ = null;

        var bindings = AnimationUtility.GetCurveBindings(clip);
        foreach (var binding in bindings)
        {
            string prop = binding.propertyName;
            if (binding.type == typeof(Transform))
            {
                AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                switch (prop)
                {
                    case "m_LocalPosition.x": posX = curve; break;
                    case "m_LocalPosition.y": posY = curve; break;
                    case "m_LocalPosition.z": posZ = curve; break;
                    case "m_LocalScale.x": scaleX = curve; break;
                    case "m_LocalScale.y": scaleY = curve; break;
                    case "m_LocalScale.z": scaleZ = curve; break;
                }
            }
        }

        // If no position/scale curves at all, return empty
        if (posX == null && posY == null && posZ == null &&
        scaleX == null && scaleY == null && scaleZ == null)
        {
            return new List<TransformKeyframe>();
        }

        // Use current transform values as defaults for missing curves
        Vector3 defaultPos = go.transform.localPosition;
        Vector3 defaultScale = go.transform.localScale;

        // Collect all unique keyframe times from all curves
        HashSet<float> times = new HashSet<float>();
        CollectKeyframeTimes(posX, times);
        CollectKeyframeTimes(posY, times);
        CollectKeyframeTimes(posZ, times);
        CollectKeyframeTimes(scaleX, times);
        CollectKeyframeTimes(scaleY, times);
        CollectKeyframeTimes(scaleZ, times);

        // Convert to sorted list
        List<float> sortedTimes = times.ToList();
        sortedTimes.Sort();

        // Build the combined keyframes
        var keyframes = new List<TransformKeyframe>();
        foreach (float time in sortedTimes)
        {
            Vector3 pos = new Vector3(
                    posX != null ? posX.Evaluate(time) : defaultPos.x,
                    posY != null ? posY.Evaluate(time) : defaultPos.y,
                    posZ != null ? posZ.Evaluate(time) : defaultPos.z
            );

            Vector3 scale = new Vector3(
                    scaleX != null ? scaleX.Evaluate(time) : defaultScale.x,
                    scaleY != null ? scaleY.Evaluate(time) : defaultScale.y,
                    scaleZ != null ? scaleZ.Evaluate(time) : defaultScale.z
            );

            keyframes.Add(new TransformKeyframe { time = time, position = pos, scale = scale });
        }

        return keyframes;
    }

    private static void CollectKeyframeTimes(AnimationCurve curve, HashSet<float> times)
    {
        if (curve == null) return;
        foreach (Keyframe kf in curve.keys)
        {
            times.Add(kf.time);
        }
    }
}