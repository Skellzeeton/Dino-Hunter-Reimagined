using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(TUILabel))]
[AddComponentMenu("TUI/Control/TUI Label Color Randomizer")]
public class TUILabelColorRandomizer : MonoBehaviour
{
    [Tooltip("List of colors to cycle through. Adjust Size in the Inspector to add/remove colors.")]
    public List<Color> colors = new List<Color>
    {
        Color.white,
        Color.red
    };

    [Tooltip("How long (in seconds) to transition from one color to the next.")]
    public float transitionDuration = 2f;

    [Tooltip("If true, colors will Lerp smoothly; if false, they snap instantly.")]
    public bool smoothTransition = true;

    [Tooltip("Optional: another label randomizer to copy colors from instead of running locally.")]
    public TUILabelColorRandomizer syncSource;

    private TUILabel _label;
    private Color _from, _to;
    private float _timer;

    void Awake()
    {
        _label = GetComponent<TUILabel>();
        // Delay ResetCycle() until Start
    }

    void Start()
    {
        if (_label == null) 
            _label = GetComponent<TUILabel>();

        if (_label != null && syncSource == null && colors.Count >= 2)
            ResetCycle();
    }

    void OnValidate()
    {
        // Make sure our label reference is current
        _label = GetComponent<TUILabel>();

        // Only reset if we have enough colors and no sync source
        if (_label != null && syncSource == null && colors.Count >= 2)
            ResetCycle();
    }

    void Update()
    {
        if (_label == null) return;

        // If we have a sync source, mirror its color
        if (syncSource != null)
        {
            if (syncSource._label == null)
                syncSource._label = syncSource.GetComponent<TUILabel>();

            if (syncSource._label != null)
                _label.color = syncSource._label.color;

            return;
        }

        // Local cycling
        if (transitionDuration <= 0f || colors.Count < 2) return;

        _timer += Time.deltaTime;
        float t = Mathf.Clamp01(_timer / transitionDuration);

        if (smoothTransition)
            ApplyColor(Color.Lerp(_from, _to, t));
        else if (t >= 1f)
            ApplyColor(_to);

        if (_timer >= transitionDuration)
        {
            _from = _to;
            _to = PickRandomColor();
            _timer = 0f;
        }
    }

    private void ResetCycle()
    {
        if (colors.Count < 2) return;

        _timer = 0f;
        _from = PickRandomColor();
        _to = PickRandomColor();
        ApplyColor(_from);
    }

    private Color PickRandomColor()
    {
        if (colors == null || colors.Count == 0)
            return Color.white;

        // pick one at random, but not the same as _to
        Color pick = colors[Random.Range(0, colors.Count)];
        if (colors.Count > 1 && pick == _to)
            return PickRandomColor();
        return pick;
    }

    private void ApplyColor(Color c)
    {
        if (_label == null)
            _label = GetComponent<TUILabel>();

        if (_label != null)
            _label.color = c;
    }
}

