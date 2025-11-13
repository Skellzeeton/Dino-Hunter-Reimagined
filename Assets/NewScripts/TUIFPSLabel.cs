using UnityEngine;

[AddComponentMenu("TUI/Control/FPS Label")]
[RequireComponent(typeof(TUIDrawSprite))]
public class TUIFPSLabel : MonoBehaviour
{
    public TUIFont fontHD;
    public Color color = Color.white;
    public Color colorBK = Color.black;
    public TUIFont.Alignment alignment = TUIFont.Alignment.Left;
    public TUILabel.TUIPivot pivot = TUILabel.TUIPivot.TopLeft;
    public float scale = 1f;
    public int lineWidth = 0;

    private TUIDrawSprite drawSprite;
    private TUIGeometry geometry = new TUIGeometry();
    private float deltaTime = 0.0f;

    private void Start()
    {
        drawSprite = GetComponent<TUIDrawSprite>();
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z - 0.01f);
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void LateUpdate()
    {
        int fps = Mathf.RoundToInt(1f / deltaTime);
        string fpsText = "FPS: " + fps.ToString();
        Draw(fpsText);
    }

    private void Draw(string displayText)
    {
        if (fontHD != null)
        {
            drawSprite.material = fontHD.material;
            geometry.Clear();

            fontHD.Print(displayText, color, geometry.Vertices, geometry.Triangles, geometry.Uv, geometry.Colors, 0.5f * scale, false, alignment, lineWidth);

            Layout(geometry);
            drawSprite.Draw(geometry.Vertices, geometry.Triangles, geometry.Uv, geometry.Colors);
            drawSprite.SetColorBK(colorBK);
            drawSprite.SetClippingRect();
        }
    }

    private void Layout(TUIGeometry geo)
    {
        geo.RecalculateBounds();
        Bounds bounds = geo.Bounds;
        Matrix4x4 layoutMatrix = Matrix4x4.identity;
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

        layoutMatrix.SetTRS(offset, Quaternion.identity, Vector3.one);
        geo.TRS(layoutMatrix);
    }
}
