// CurveReduction.cs
//
// Pure keyframe-reduction math used by the Anim Keyframe Simplifier tool.
// Lives under an Editor folder alongside the rest of the tool (not because it
// needs UnityEditor itself, but so it isn't compiled into player builds).

namespace KeyframeSimplifierTool
{
    public static class CurveReduction
    {
        /// <summary>
        /// Ramer-Douglas-Peucker style reduction over N-dimensional samples that
        /// share one time axis (e.g. a grouped Vector3/Quaternion curve, or a
        /// single float curve with dims == 1).
        ///
        /// A point is dropped only if every dimension stays within "tolerance"
        /// of the straight line between its surviving neighbours. This checks
        /// deviation from *linear* interpolation of value, not the curve's
        /// actual Hermite tangent shape - simple and predictable, matching what
        /// most curve-reduction tools do. It means a keyframe that exists purely
        /// to carry a sharp tangent change (value itself barely moves) might not
        /// be flagged as significant - kept deliberately conservative for that
        /// reason rather than trying to be clever about tangents.
        /// </summary>
        public static bool[] ReduceRDP(float[] times, float[][] values, float tolerance)
        {
            int n = times.Length;
            var keep = new bool[n];
            if (n == 0) return keep;
            keep[0] = true;
            keep[n - 1] = true;
            if (n <= 2) return keep;
            Recurse(times, values, 0, n - 1, tolerance, keep);
            return keep;
        }

        static void Recurse(float[] times, float[][] values, int startIdx, int endIdx, float tol, bool[] keep)
        {
            if (endIdx <= startIdx + 1) return;

            float t0 = times[startIdx];
            float t1 = times[endIdx];
            float dt = t1 - t0;
            int dims = values[startIdx].Length;

            int worstIdx = -1;
            float worstDev = 0f;

            for (int i = startIdx + 1; i < endIdx; i++)
            {
                float u = dt > 1e-9f ? (times[i] - t0) / dt : 0f;
                float localWorst = 0f;
                for (int d = 0; d < dims; d++)
                {
                    float v0 = values[startIdx][d];
                    float v1 = values[endIdx][d];
                    float interp = v0 + (v1 - v0) * u;
                    float dev = values[i][d] - interp;
                    if (dev < 0f) dev = -dev;
                    if (dev > localWorst) localWorst = dev;
                }
                if (localWorst > worstDev)
                {
                    worstDev = localWorst;
                    worstIdx = i;
                }
            }

            if (worstIdx == -1 || worstDev <= tol)
                return; // everything strictly between start/end can be dropped

            keep[worstIdx] = true;
            Recurse(times, values, startIdx, worstIdx, tol, keep);
            Recurse(times, values, worstIdx, endIdx, tol, keep);
        }
    }
}
