using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TransformKeyframe
{
    public float time;          // time in seconds
    public Vector3 position;    // local position
    public Vector3 scale;       // local scale
}

public class UIAnimator : MonoBehaviour
{
    [Header("Settings")]
    public bool playOnAwake = true;
    public bool loop = true;

    [Header("Keyframes")]
    public List<TransformKeyframe> keyframes = new List<TransformKeyframe>();

    private bool isPlaying = false;
    private float currentTime = 0f;
    private int currentSegment = -1; // index of the first keyframe in the current segment

    public bool IsPlaying => isPlaying;

    private void Awake()
    {
        if (playOnAwake)
            Play();
    }

    private void Update()
    {
        if (!isPlaying || keyframes.Count == 0)
            return;

        // Advance time
        currentTime += Time.deltaTime;
        float totalDuration = GetTotalDuration();

        if (loop)
        {
            if (totalDuration > 0f)
                currentTime %= totalDuration;
            else
                currentTime = 0f;
        }
        else
        {
            if (currentTime >= totalDuration)
            {
                currentTime = totalDuration;
                isPlaying = false;
            }
        }

        ApplyFrame(currentTime);
    }

    /// <summary>
    /// Starts playback from the beginning.
    /// </summary>
    public void Play()
    {
        currentTime = 0f;
        currentSegment = -1;
        isPlaying = true;
        ApplyFrame(0f);
    }

    /// <summary>
    /// Stops playback and freezes at the current time.
    /// </summary>
    public void Stop()
    {
        isPlaying = false;
    }

    /// <summary>
    /// Pauses playback; can be resumed with Resume().
    /// </summary>
    public void Pause()
    {
        isPlaying = false;
    }

    /// <summary>
    /// Resumes playback from the current time.
    /// </summary>
    public void Resume()
    {
        if (keyframes.Count > 0)
            isPlaying = true;
    }

    private float GetTotalDuration()
    {
        if (keyframes.Count == 0) return 0f;
        return keyframes[keyframes.Count - 1].time;
    }

    private void ApplyFrame(float time)
    {
        if (keyframes.Count == 0)
            return;

        // Clamp time to valid range
        float totalDuration = GetTotalDuration();
        float clampedTime = Mathf.Clamp(time, 0f, totalDuration);

        // If only one keyframe, use it directly
        if (keyframes.Count == 1)
        {
            ApplyKeyframe(keyframes[0]);
            return;
        }

        // Find the two surrounding keyframes
        int nextIndex = FindNextKeyframeIndex(clampedTime);
        if (nextIndex <= 0)
        {
            // Before first keyframe
            ApplyKeyframe(keyframes[0]);
            return;
        }
        if (nextIndex >= keyframes.Count)
        {
            // After last keyframe
            ApplyKeyframe(keyframes[keyframes.Count - 1]);
            return;
        }

        TransformKeyframe prev = keyframes[nextIndex - 1];
        TransformKeyframe next = keyframes[nextIndex];

        float segmentDuration = next.time - prev.time;
        if (segmentDuration <= 0f)
        {
            // Avoid division by zero
            ApplyKeyframe(prev);
            return;
        }

        float t = (clampedTime - prev.time) / segmentDuration;
        t = Mathf.Clamp01(t);

        Vector3 pos = Vector3.Lerp(prev.position, next.position, t);
        Vector3 scale = Vector3.Lerp(prev.scale, next.scale, t);

        transform.localPosition = pos;
        transform.localScale = scale;
    }

    private int FindNextKeyframeIndex(float time)
    {
        // Returns the index of the first keyframe whose time is >= the given time
        for (int i = 0; i < keyframes.Count; i++)
        {
            if (keyframes[i].time >= time)
                return i;
        }
        return keyframes.Count; // beyond end
    }

    private void ApplyKeyframe(TransformKeyframe kf)
    {
        transform.localPosition = kf.position;
        transform.localScale = kf.scale;
    }
}