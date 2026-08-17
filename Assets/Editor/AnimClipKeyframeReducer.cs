// AnimClipKeyframeReducer.cs
//
// Reads a .anim asset's raw YAML text, works out which keyframes are safe to
// drop (using AnimationUtility for the authoritative curve data), and writes
// the file back with ONLY those keyframe line-blocks removed. Everything else
// in the file - formatting, unrelated curves, header, events, sample rate,
// bounds, etc. - is left byte-for-byte identical. That's what keeps the
// resulting file small instead of ballooning the way a normal Unity re-save
// through AssetDatabase does.
//
// IMPORTANT LIMITATIONS (please read):
//  - Only works on Force Text serialized assets. Binary-serialized .anim
//    files are refused with a warning rather than risked.
//  - Skips m_CompressedRotationCurves entirely (bit-packed format, not safe
//    to hand-edit). Uncompressed rotation/euler/position/scale/float curves
//    are all supported.
//  - Object reference (PPtr) curves (e.g. sprite-swap animation) are not
//    touched in this version.
//  - Deviation is measured against straight-line interpolation between
//    surviving neighbours, not the true Hermite tangent shape. See
//    CurveReduction.cs for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Path = System.IO.Path;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KeyframeSimplifierTool
{
    public static class AnimClipKeyframeReducer
    {
        public struct ToleranceSettings
        {
            public float position;      // units
            public float rotation;      // quaternion component units, roughly -1..1
            public float eulerDegrees;  // degrees
            public float scale;         // units
            public float floatCurve;    // whatever units that property uses
        }

        public class ReductionPlanEntry
        {
            public string GroupLabel;
            public int OriginalCount;
            public int KeptCount;
            public List<LineRange> RangesToDelete = new List<LineRange>();
        }

        public struct LineRange
        {
            public int Start;      // inclusive
            public int EndExcl;    // exclusive
            public LineRange(int s, int e) { Start = s; EndExcl = e; }
        }

        public class ReductionResult
        {
            public string ClipName;
            public string AssetPath;
            public bool Applied;
            public bool ApplyFailed;
            public bool HadChanges;
            public long OriginalBytes;
            public long NewBytes;
            public int TotalOriginalKeyframes;
            public int TotalKeptKeyframes;
            public List<string> Warnings = new List<string>();
            public List<ReductionPlanEntry> Entries = new List<ReductionPlanEntry>();

            // internal state carried from BuildPlan to Apply
            internal List<string> PendingLines;
            internal List<LineRange> PendingDeleteRanges;
        }

        enum SectionKind { Position, Rotation, Scale, Euler, Float }

        class BindingGroup
        {
            public SectionKind Kind;
            public string Path;
            public string Attribute; // only meaningful for Float
            public string Label;
            public List<KeyValuePair<char, EditorCurveBinding>> AxisBindingsRaw = new List<KeyValuePair<char, EditorCurveBinding>>();
            public EditorCurveBinding[] AxisBindings;
        }

        // ------------------------------------------------------------------
        // Public API
        // ------------------------------------------------------------------

        public static ReductionResult BuildPlan(AnimationClip clip, ToleranceSettings tol)
        {
            var result = new ReductionResult { ClipName = clip.name };
            string assetPath = AssetDatabase.GetAssetPath(clip);
            result.AssetPath = assetPath;

            if (string.IsNullOrEmpty(assetPath) || System.IO.Path.GetExtension(assetPath).ToLowerInvariant() != ".anim")
            {
                result.Warnings.Add("Not a standalone .anim asset on disk (might be embedded in an FBX or a controller) - skipped.");
                return result;
            }

            string fullPath = System.IO.Path.GetFullPath(assetPath);
            if (!File.Exists(fullPath))
            {
                result.Warnings.Add("Could not find the asset file on disk.");
                return result;
            }

            string text = File.ReadAllText(fullPath);
            result.OriginalBytes = new FileInfo(fullPath).Length;

            if (!text.TrimStart().StartsWith("%YAML"))
            {
                result.Warnings.Add(
                    "This asset is binary-serialized, not text (YAML). Switch Edit > Project Settings > " +
                    "Editor > Asset Serialization to 'Force Text', reserialize (Assets > Reserialize All), " +
                    "then try again. Binary assets can't be safely hand-patched.");
                return result;
            }

            var lines = new List<string>(text.Replace("\r\n", "\n").Split('\n'));

            // Paths whose rotation is stored compressed - never touch these.
            var compressedPaths = new HashSet<string>();
            int compIdx = FindTopLevelKeyLine(lines, "m_CompressedRotationCurves");
            if (compIdx != -1 && !IsEmptyInline(lines[compIdx]))
            {
                foreach (var item in GetListItemRanges(lines, compIdx))
                {
                    string p = FindFieldInRange(lines, item, "path");
                    if (p != null) compressedPaths.Add(p);
                }
            }

            var bindings = AnimationUtility.GetCurveBindings(clip);
            var groups = GroupBindings(bindings);

            var allDeleteRanges = new List<LineRange>();

            foreach (var group in groups)
            {
                if (group.Kind == SectionKind.Rotation && compressedPaths.Contains(group.Path))
                {
                    result.Warnings.Add(string.Format(
                        "Skipped '{0}': rotation is stored as compressed curve data, which this tool doesn't edit.",
                        group.Path));
                    continue;
                }

                string sectionKey = SectionKeyFor(group.Kind);
                int sectionIdx = FindTopLevelKeyLine(lines, sectionKey);
                if (sectionIdx == -1 || IsEmptyInline(lines[sectionIdx]))
                    continue; // nothing on disk for this section

                var items = GetListItemRanges(lines, sectionIdx);

                LineRange? matchedItem = null;
                foreach (var item in items)
                {
                    string p = FindFieldInRange(lines, item, "path");
                    if (p != group.Path) continue;
                    if (group.Kind == SectionKind.Float)
                    {
                        string a = FindFieldInRange(lines, item, "attribute");
                        if (a != group.Attribute) continue;
                    }
                    matchedItem = item;
                    break;
                }

                if (matchedItem == null)
                {
                    result.Warnings.Add(string.Format("Could not locate on-disk block for '{0}' - skipped for safety.", group.Label));
                    continue;
                }

                int curveKeyIdx = FindFieldLineInRange(lines, matchedItem.Value, "m_Curve");
                if (curveKeyIdx == -1)
                {
                    result.Warnings.Add(string.Format("Unexpected file structure for '{0}' - skipped for safety.", group.Label));
                    continue;
                }
                var kfItems = GetListItemRanges(lines, curveKeyIdx);

                float[] times; float[][] values;
                if (!BuildSamples(clip, group, out times, out values))
                {
                    result.Warnings.Add(string.Format("Axis curves for '{0}' don't line up (different keyframe counts/times per axis) - skipped for safety.", group.Label));
                    continue;
                }

                if (kfItems.Count != times.Length)
                {
                    result.Warnings.Add(string.Format(
                        "Keyframe count mismatch for '{0}' between file ({1}) and loaded curve ({2}) - skipped for safety.",
                        group.Label, kfItems.Count, times.Length));
                    continue;
                }

                bool timesOk = true;
                for (int i = 0; i < times.Length; i++)
                {
                    string rawTime = FindFieldInRange(lines, kfItems[i], "time");
                    float fileTime = ParseFloatField(rawTime);
                    if (float.IsNaN(fileTime) || Mathf.Abs(fileTime - times[i]) > 0.001f)
                    {
                        timesOk = false;
                        break;
                    }
                }
                if (!timesOk)
                {
                    result.Warnings.Add(string.Format("Keyframe ordering/time mismatch for '{0}' - skipped for safety.", group.Label));
                    continue;
                }

                float t = ToleranceFor(group.Kind, tol);
                bool[] keep = CurveReduction.ReduceRDP(times, values, t);

                var entry = new ReductionPlanEntry { GroupLabel = group.Label, OriginalCount = times.Length };
                int kept = 0;
                for (int i = 0; i < keep.Length; i++)
                {
                    if (keep[i]) kept++;
                    else entry.RangesToDelete.Add(kfItems[i]);
                }
                entry.KeptCount = kept;
                result.Entries.Add(entry);
                result.TotalOriginalKeyframes += entry.OriginalCount;
                result.TotalKeptKeyframes += kept;
                allDeleteRanges.AddRange(entry.RangesToDelete);
            }

            result.HadChanges = allDeleteRanges.Count > 0;
            result.PendingLines = lines;
            result.PendingDeleteRanges = allDeleteRanges;
            return result;
        }

        /// <summary>
        /// Writes the planned changes to disk. If a write attempt fails (e.g.
        /// the file is briefly locked by another process - an antivirus
        /// scan, a VCS operation, an editor holding it open) the original
        /// content is restored from the backup and the write is retried, up
        /// to maxAttempts times, with a short pause between attempts.
        ///
        /// Retries are bounded rather than infinite: if the failure turns
        /// out to be permanent (read-only file, permissions, disk full),
        /// an unbounded retry loop would hang the Editor instead of
        /// reporting the problem. After the last attempt fails, the file is
        /// left in its original, unmodified state and the failure is
        /// recorded in result.Warnings / result.ApplyFailed.
        ///
        /// On success, the .bak safety backup is deleted - it only needs to
        /// exist while a write is still unconfirmed.
        /// </summary>
        public static bool Apply(ReductionResult result, bool backup, int maxAttempts = 5, int retryDelayMs = 250)
        {
            if (!result.HadChanges || result.PendingLines == null) return false;

            string fullPath = System.IO.Path.GetFullPath(result.AssetPath);
            string backupPath = fullPath + ".bak";
            string newContent = string.Join("\n", BuildFinalLines(result));

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                bool madeBackupThisAttempt = false;
                try
                {
                    if (backup)
                    {
                        File.Copy(fullPath, backupPath, true);
                        madeBackupThisAttempt = true;
                    }

                    File.WriteAllText(fullPath, newContent);
                    AssetDatabase.ImportAsset(result.AssetPath, ImportAssetOptions.ForceUpdate);

                    // Success - the backup has done its job, remove it.
                    if (madeBackupThisAttempt && File.Exists(backupPath))
                        File.Delete(backupPath);

                    result.Applied = true;
                    result.NewBytes = new FileInfo(fullPath).Length;
                    return true;
                }
                catch (Exception ex)
                {
                    result.Warnings.Add(string.Format(
                        "Attempt {0}/{1} to write '{2}' failed: {3}", attempt, maxAttempts, result.ClipName, ex.Message));

                    try
                    {
                        if (madeBackupThisAttempt && File.Exists(backupPath))
                        {
                            File.Copy(backupPath, fullPath, true);
                            File.Delete(backupPath);
                        }
                    }
                    catch
                    {
                        // Best-effort restore. If even this failed, leave the
                        // .bak in place so the original content isn't lost.
                    }

                    if (attempt < maxAttempts)
                        System.Threading.Thread.Sleep(retryDelayMs);
                }
            }

            result.ApplyFailed = true;
            result.Warnings.Add(string.Format(
                "Gave up on '{0}' after {1} attempts - left unmodified.", result.ClipName, maxAttempts));
            return false;
        }

        static List<string> BuildFinalLines(ReductionResult result)
        {
            var lines = new List<string>(result.PendingLines);
            var ranges = result.PendingDeleteRanges.OrderByDescending(r => r.Start).ToList();
            foreach (var r in ranges)
                lines.RemoveRange(r.Start, r.EndExcl - r.Start);
            return lines;
        }

        // ------------------------------------------------------------------
        // Binding grouping
        // ------------------------------------------------------------------

        static List<BindingGroup> GroupBindings(EditorCurveBinding[] bindings)
        {
            var map = new Dictionary<string, BindingGroup>();
            var floatList = new List<BindingGroup>();

            foreach (var b in bindings)
            {
                string prop = b.propertyName;
                string baseName; char axis;
                SectionKind kind;

                if (TryParseAxis(prop, out baseName, out axis) && TryMapBase(baseName, out kind))
                {
                    string key = kind + "|" + b.path;
                    BindingGroup g;
                    if (!map.TryGetValue(key, out g))
                    {
                        g = new BindingGroup { Kind = kind, Path = b.path, Label = b.path + " (" + kind + ")" };
                        map[key] = g;
                    }
                    g.AxisBindingsRaw.Add(new KeyValuePair<char, EditorCurveBinding>(axis, b));
                }
                else
                {
                    var g = new BindingGroup { Kind = SectionKind.Float, Path = b.path, Attribute = prop, Label = b.path + ":" + prop };
                    g.AxisBindingsRaw.Add(new KeyValuePair<char, EditorCurveBinding>('\0', b));
                    floatList.Add(g);
                }
            }

            var result = new List<BindingGroup>(map.Values);
            result.AddRange(floatList);

            const string order = "xyzw";
            foreach (var g in result)
            {
                g.AxisBindings = g.AxisBindingsRaw
                    .OrderBy(kv => order.IndexOf(kv.Key == '\0' ? 'x' : kv.Key))
                    .Select(kv => kv.Value)
                    .ToArray();
            }
            return result;
        }

        static bool TryParseAxis(string prop, out string baseName, out char axis)
        {
            baseName = prop;
            axis = '\0';
            if (prop.Length >= 2 && prop[prop.Length - 2] == '.')
            {
                char c = prop[prop.Length - 1];
                if (c == 'x' || c == 'y' || c == 'z' || c == 'w')
                {
                    baseName = prop.Substring(0, prop.Length - 2);
                    axis = c;
                    return true;
                }
            }
            return false;
        }

        static bool TryMapBase(string baseName, out SectionKind kind)
        {
            switch (baseName)
            {
                case "m_LocalPosition": kind = SectionKind.Position; return true;
                case "m_LocalScale": kind = SectionKind.Scale; return true;
                case "m_LocalRotation": kind = SectionKind.Rotation; return true;
                case "m_LocalEulerAnglesRaw":
                case "m_LocalEulerAngles": kind = SectionKind.Euler; return true;
                default: kind = SectionKind.Float; return false;
            }
        }

        static bool BuildSamples(AnimationClip clip, BindingGroup group, out float[] times, out float[][] values)
        {
            times = null;
            values = null;

            var curves = new AnimationCurve[group.AxisBindings.Length];
            for (int i = 0; i < group.AxisBindings.Length; i++)
            {
                curves[i] = AnimationUtility.GetEditorCurve(clip, group.AxisBindings[i]);
                if (curves[i] == null) return false;
            }

            int n = curves[0].length;
            for (int i = 1; i < curves.Length; i++)
                if (curves[i].length != n) return false;

            times = new float[n];
            values = new float[n][];
            for (int i = 0; i < n; i++)
            {
                times[i] = curves[0][i].time;
                values[i] = new float[curves.Length];
                for (int d = 0; d < curves.Length; d++)
                {
                    if (Mathf.Abs(curves[d][i].time - times[i]) > 0.0005f)
                        return false;
                    values[i][d] = curves[d][i].value;
                }
            }
            return true;
        }

        static string SectionKeyFor(SectionKind kind)
        {
            switch (kind)
            {
                case SectionKind.Position: return "m_PositionCurves";
                case SectionKind.Rotation: return "m_RotationCurves";
                case SectionKind.Scale: return "m_ScaleCurves";
                case SectionKind.Euler: return "m_EulerCurves";
                case SectionKind.Float: return "m_FloatCurves";
            }
            return null;
        }

        static float ToleranceFor(SectionKind kind, ToleranceSettings tol)
        {
            switch (kind)
            {
                case SectionKind.Position: return tol.position;
                case SectionKind.Rotation: return tol.rotation;
                case SectionKind.Scale: return tol.scale;
                case SectionKind.Euler: return tol.eulerDegrees;
                case SectionKind.Float: return tol.floatCurve;
            }
            return tol.floatCurve;
        }

        // ------------------------------------------------------------------
        // Minimal, purpose-built YAML line scanning
        // (Unity's asset YAML is emitted with very regular indentation, so a
        // full YAML parser isn't needed - we only need block boundaries.)
        // ------------------------------------------------------------------

        static int FindTopLevelKeyLine(List<string> lines, string key)
        {
            string needle = "  " + key + ":";
            for (int i = 0; i < lines.Count; i++)
                if (lines[i].StartsWith(needle)) return i;
            return -1;
        }

        static bool IsEmptyInline(string line)
        {
            int idx = line.IndexOf(':');
            if (idx < 0) return true;
            string rest = line.Substring(idx + 1).Trim();
            return rest == "[]" || rest == "{}";
        }

        static int GetIndent(string line)
        {
            int i = 0;
            while (i < line.Length && line[i] == ' ') i++;
            return i;
        }

        static bool IsListItemAt(string line, int indent)
        {
            return GetIndent(line) == indent
                && line.Length > indent + 1
                && line[indent] == '-'
                && line[indent + 1] == ' ';
        }

        /// <summary>
        /// Given the line index of a "key:" that introduces a YAML list
        /// (Unity emits list item markers at the SAME indent as the key, not
        /// indented further), returns the [start,end) line range of each item.
        /// </summary>
        static List<LineRange> GetListItemRanges(List<string> lines, int keyLineIdx)
        {
            var ranges = new List<LineRange>();
            int indent = GetIndent(lines[keyLineIdx]);
            int i = keyLineIdx + 1;
            int itemStart = -1;

            while (i < lines.Count)
            {
                string line = lines[i];
                if (line.Length == 0) { i++; continue; }

                if (IsListItemAt(line, indent))
                {
                    if (itemStart != -1) ranges.Add(new LineRange(itemStart, i));
                    itemStart = i;
                    i++;
                    continue;
                }

                int ind = GetIndent(line);
                if (ind <= indent) break; // dedented out of the list

                i++;
            }
            if (itemStart != -1) ranges.Add(new LineRange(itemStart, i));
            return ranges;
        }

        static string FindFieldInRange(List<string> lines, LineRange range, string field)
        {
            string needle = field + ":";
            for (int i = range.Start; i < range.EndExcl; i++)
            {
                string t = lines[i].TrimStart();
                if (t.StartsWith("- ")) t = t.Substring(2);
                if (t.StartsWith(needle))
                {
                    string v = t.Substring(needle.Length).Trim();
                    if (v.Length >= 2 &&
                        ((v[0] == '"' && v[v.Length - 1] == '"') || (v[0] == '\'' && v[v.Length - 1] == '\'')))
                    {
                        v = v.Substring(1, v.Length - 2);
                    }
                    return v;
                }
            }
            return null;
        }

        static int FindFieldLineInRange(List<string> lines, LineRange range, string field)
        {
            string needle = field + ":";
            for (int i = range.Start; i < range.EndExcl; i++)
            {
                string t = lines[i].TrimStart();
                if (t.StartsWith("- ")) t = t.Substring(2);
                if (t.StartsWith(needle)) return i;
            }
            return -1;
        }

        static float ParseFloatField(string s)
        {
            if (string.IsNullOrEmpty(s)) return float.NaN;
            float v;
            if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                return v;
            return float.NaN;
        }
    }
}