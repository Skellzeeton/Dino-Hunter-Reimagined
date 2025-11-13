using UnityEngine;

[AddComponentMenu("TUI/Control/Scene FPS Label")]
[ExecuteInEditMode]
[RequireComponent(typeof(TUIDrawSprite))]
public class SceneFPSLabel : MonoBehaviour
{
    public TUIFont fontHD;
    public Color color = Color.white;
    public Color backgroundColor = Color.black;
    public TUIFont.Alignment alignment = TUIFont.Alignment.Left;
    public TUILabel.TUIPivot pivot = TUILabel.TUIPivot.TopLeft;
    public float scale = 1f;
    public int lineWidth = 0;

    private TUIDrawSprite drawSprite;
    private TUIGeometry geometry = new TUIGeometry();
    private float deltaTime = 0f;

    private void Start()
    {
        drawSprite = GetComponent<TUIDrawSprite>();
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z - 0.01f);
    }

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.5f;
    }

    private void LateUpdate()
    {
        int fps = Mathf.RoundToInt(1f / deltaTime);
        string label = "FPS: " + fps;
        RenderLabel(label);
    }

    private void RenderLabel(string labelText)
    {
        if (fontHD == null || drawSprite == null) return;

        drawSprite.material = fontHD.material;
        geometry.Clear();

        fontHD.Print(labelText, color, geometry.Vertices, geometry.Triangles, geometry.Uv, geometry.Colors, 0.5f * scale, false, alignment, lineWidth);

        ApplyLayout(geometry);
        drawSprite.Draw(geometry.Vertices, geometry.Triangles, geometry.Uv, geometry.Colors);
        drawSprite.SetColorBK(backgroundColor);
        drawSprite.SetClippingRect();
    }

    private void ApplyLayout(TUIGeometry geo)
    {
        geo.RecalculateBounds();
        Bounds bounds = geo.Bounds;
        Vector3 offset = Vector3.zero;

        switch (pivot)
        {
            case TUILabel.TUIPivot.Center:       offset = -bounds.center; break;
            case TUILabel.TUIPivot.TopLeft:      offset = new Vector3(-bounds.min.x, -bounds.max.y); break;
            case TUILabel.TUIPivot.TopRight:     offset = new Vector3(-bounds.max.x, -bounds.max.y); break;
            case TUILabel.TUIPivot.BottomLeft:   offset = new Vector3(-bounds.min.x, -bounds.min.y); break;
            case TUILabel.TUIPivot.BottomRight:  offset = new Vector3(-bounds.max.x, -bounds.min.y); break;
            case TUILabel.TUIPivot.Top:          offset = new Vector3(-bounds.center.x, -bounds.max.y); break;
            case TUILabel.TUIPivot.Bottom:       offset = new Vector3(-bounds.center.x, -bounds.min.y); break;
            case TUILabel.TUIPivot.Left:         offset = new Vector3(-bounds.min.x, -bounds.center.y); break;
            case TUILabel.TUIPivot.Right:        offset = new Vector3(-bounds.max.x, -bounds.center.y); break;
        }

        Matrix4x4 layout = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one);
        geo.TRS(layout);
    }
}
