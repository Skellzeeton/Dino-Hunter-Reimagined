// KeyframeSimplifierWindow.cs
//
// Tools > Animation > Keyframe Simplifier
//
// Pick one or more .anim clips, then either:
//   - "Simplify Keyframes"          lossy, tolerance-driven curve reduction
//   - "Remove Redundant Keyframes"  near-zero-tolerance dedupe only
//
// Both write via AnimClipKeyframeReducer, which patches the .anim YAML text
// directly instead of round-tripping through Unity's AnimationClip
// serializer - that's what keeps the resulting file small.

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KeyframeSimplifierTool
{
    public class KeyframeSimplifierWindow : EditorWindow
    {
        List<AnimationClip> targets = new List<AnimationClip>();

        float posTol = 0.0005f;
        float rotTol = 0.0005f;
        float eulerTol = 0.02f;
        float scaleTol = 0.0005f;
        float floatTol = 0.0005f;

        bool makeBackup = true;
        Vector2 scroll;
        Vector2 clipListScroll;
        List<AnimClipKeyframeReducer.ReductionResult> lastResults = new List<AnimClipKeyframeReducer.ReductionResult>();

        [MenuItem("Tools/Animation/Keyframe Simplifier")]
        public static void Open()
        {
            GetWindow<KeyframeSimplifierWindow>("Keyframe Simplifier");
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Edits the .anim file's YAML text directly and removes only the keyframe lines that are " +
                "no longer needed. Everything else in the file is left untouched, so file size stays close " +
                "to a clean/imported clip instead of growing the way a normal Unity re-save does.\n\n" +
                "Requires Force Text asset serialization (Project Settings > Editor > Asset Serialization). " +
                "Compressed rotation curves and object-reference (sprite swap) curves are not modified.",
                MessageType.Info);

            DrawTargetList();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Simplify tolerances (used by 'Simplify Keyframes' only)", EditorStyles.boldLabel);
            posTol = EditorGUILayout.FloatField(new GUIContent("Position", "Scene units"), posTol);
            rotTol = EditorGUILayout.FloatField(new GUIContent("Rotation (quaternion)", "Component units, roughly -1..1"), rotTol);
            eulerTol = EditorGUILayout.FloatField(new GUIContent("Rotation (euler)", "Degrees"), eulerTol);
            scaleTol = EditorGUILayout.FloatField(new GUIContent("Scale"), scaleTol);
            floatTol = EditorGUILayout.FloatField(new GUIContent("Float / generic curves"), floatTol);

            EditorGUILayout.Space();
            makeBackup = EditorGUILayout.ToggleLeft("Write a .bak backup before modifying", makeBackup);

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Preview Simplify", GUILayout.Height(26)))
                    RunPreview(CurrentTolerances());
                if (GUILayout.Button("Apply Simplify", GUILayout.Height(26)))
                    RunApply(CurrentTolerances());
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Preview Remove Redundant Only", GUILayout.Height(26)))
                    RunPreview(LosslessTolerances());
                if (GUILayout.Button("Apply Remove Redundant Only", GUILayout.Height(26)))
                    RunApply(LosslessTolerances());
            }

            DrawResults();
        }

        AnimClipKeyframeReducer.ToleranceSettings CurrentTolerances()
        {
            return new AnimClipKeyframeReducer.ToleranceSettings
            {
                position = posTol,
                rotation = rotTol,
                eulerDegrees = eulerTol,
                scale = scaleTol,
                floatCurve = floatTol
            };
        }

        AnimClipKeyframeReducer.ToleranceSettings LosslessTolerances()
        {
            return new AnimClipKeyframeReducer.ToleranceSettings
            {
                position = 0.00005f,
                rotation = 0.00005f,
                eulerDegrees = 0.005f,
                scale = 0.00005f,
                floatCurve = 0.00005f
            };
        }

        void DrawTargetList()
        {
            EditorGUILayout.LabelField("Clips", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Use Current Selection"))
                    targets = Selection.objects.OfType<AnimationClip>().ToList();

                if (GUILayout.Button("Clear"))
                {
                    targets.Clear();
                    lastResults.Clear();
                    GUIUtility.ExitGUI();
                }
            }

            // Scrollable clip list with a fixed max height
            float maxListHeight = Mathf.Min(200f, targets.Count * 22f + 10f);
            if (targets.Count > 8)
                maxListHeight = 200f;
            else if (targets.Count > 0)
                maxListHeight = targets.Count * 22f + 10f;

            clipListScroll = EditorGUILayout.BeginScrollView(clipListScroll, GUILayout.Height(maxListHeight));
            
            for (int i = 0; i < targets.Count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    targets[i] = (AnimationClip)EditorGUILayout.ObjectField(targets[i], typeof(AnimationClip), false);
                    if (GUILayout.Button("x", GUILayout.Width(20)))
                    {
                        targets.RemoveAt(i);
                        GUIUtility.ExitGUI();
                    }
                }
            }
            
            EditorGUILayout.EndScrollView();

            var dropped = (AnimationClip)EditorGUILayout.ObjectField("Add clip", null, typeof(AnimationClip), false);
            if (dropped != null && !targets.Contains(dropped))
                targets.Add(dropped);
        }

        void RunPreview(AnimClipKeyframeReducer.ToleranceSettings tol)
        {
            lastResults = targets
                .Where(t => t != null)
                .Select(c => AnimClipKeyframeReducer.BuildPlan(c, tol))
                .ToList();
        }

        void RunApply(AnimClipKeyframeReducer.ToleranceSettings tol)
        {
            var results = new List<AnimClipKeyframeReducer.ReductionResult>();
            foreach (var clip in targets.Where(t => t != null))
            {
                var plan = AnimClipKeyframeReducer.BuildPlan(clip, tol);
                if (plan.HadChanges)
                    AnimClipKeyframeReducer.Apply(plan, makeBackup);
                results.Add(plan);
            }
            lastResults = results;
            AssetDatabase.Refresh();
        }

        void DrawResults()
        {
            if (lastResults.Count == 0) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Results", EditorStyles.boldLabel);
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(220));

            foreach (var r in lastResults)
            {
                string status;
                if (r.Applied)
                    status = string.Format("{0}B -> {1}B", r.OriginalBytes, r.NewBytes);
                else if (r.ApplyFailed)
                    status = "failed - see warnings below";
                else if (r.HadChanges)
                    status = "preview only";
                else
                    status = "no change";

                EditorGUILayout.LabelField(string.Format("{0}: {1} -> {2} keyframes ({3})",
                    r.ClipName, r.TotalOriginalKeyframes, r.TotalKeptKeyframes, status));

                foreach (var w in r.Warnings)
                    EditorGUILayout.HelpBox(w, MessageType.Warning);
            }

            EditorGUILayout.EndScrollView();
        }
    }
}