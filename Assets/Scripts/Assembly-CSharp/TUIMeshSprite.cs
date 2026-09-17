using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu("TUI/Control/Mesh Sprite")]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class TUIMeshSprite : TUINeedUpdateBase
{
	[SerializeField]
	protected bool onlyUseRetinaTexture = true;

	public string m_texture;

	public Color m_color = Color.white;

	public bool m_flipX;

	public bool m_flipY;

	private string baseShader = string.Empty;

	private static string grayShader = "Triniti/TUI/TUIGrayStyle";

	[SerializeField]
	protected TUILabel.TUIPivot pivot = TUILabel.TUIPivot.Center;

	[SerializeField]
	protected bool grayStyle;

	[SerializeField]
	protected bool useCustomize;

	[SerializeField]
	protected Texture customizeTexture;

	[SerializeField]
	protected Rect customizeRect;

	protected Material customizeMaterial;

	public GameObject m_hideClipObj;

	public GameObject m_showClipObj;

	public List<TUIRect> otherClips;

	private List<TUIRect> showClipRectList = new List<TUIRect>();

	protected MeshFilter meshFilter;

	protected MeshRenderer meshRender;

	private Material sharedMat;

	private Material grayMat;

	public bool NeedLaterUpdate;

	// ---------------------------------------------------------------------
	// Shared mesh cache
	// ---------------------------------------------------------------------

	private struct MeshKey : System.IEquatable<MeshKey>
	{
		public int texId;
		public Vector4 uv;
		public Rect rect;
		public Color color;
		public int pivot;
		public bool hasHide;
		public Rect hideRect;

		public bool Equals(MeshKey o)
		{
			return texId == o.texId
			&& uv == o.uv
			&& rect == o.rect
			&& color == o.color
			&& pivot == o.pivot
			&& hasHide == o.hasHide
			&& (!hasHide || hideRect == o.hideRect);
		}

		public override bool Equals(object o)
		{
			return o is MeshKey k && Equals(k);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int h = 17;
				h = h * 31 + texId;
				h = h * 31 + uv.GetHashCode();
				h = h * 31 + rect.GetHashCode();
				h = h * 31 + color.GetHashCode();
				h = h * 31 + pivot;
				h = h * 31 + (hasHide ? 1 : 0);
				if (hasHide) h = h * 31 + hideRect.GetHashCode();
				return h;
			}
		}
	}

	private static readonly Dictionary<MeshKey, Mesh> s_MeshCache = new Dictionary<MeshKey, Mesh>();
	private static Mesh s_EmptyMesh;
	private static bool s_Hooked;

	private static Mesh EmptyMesh
	{
		get
		{
			if (s_EmptyMesh == null)
			{
				s_EmptyMesh = new Mesh();
				s_EmptyMesh.name = "TUIMeshSprite_Empty";
				s_EmptyMesh.hideFlags = HideFlags.DontSave;
			}
			return s_EmptyMesh;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void InitializeHook()
	{
		if (s_Hooked)
		{
			return;      // <-- this early-out was already there for the event subscriptions…
		}                //     …but ClearMeshCache() was sitting AFTER it, outside the guard.
		s_Hooked = true;
		SceneManager.sceneUnloaded += OnSceneUnloaded;
#if UNITY_EDITOR
		EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
		ClearMeshCache();   // <-- moved INSIDE the guard
	}

	private static void OnSceneUnloaded(Scene scene)
	{
		ClearMeshCache();
	}

#if UNITY_EDITOR
	private static void OnPlayModeStateChanged(PlayModeStateChange state)
	{
		if (state == PlayModeStateChange.EnteredEditMode)
		{
			ClearMeshCache();
		}
	}
#endif

	public static void ClearMeshCache()
	{
		foreach (Mesh m in s_MeshCache.Values)
		{
			if (m != null)
			{
				if (Application.isPlaying) Object.Destroy(m);
				else Object.DestroyImmediate(m);
			}
		}
		s_MeshCache.Clear();

		if (s_EmptyMesh != null)
		{
			if (Application.isPlaying) Object.Destroy(s_EmptyMesh);
			else Object.DestroyImmediate(s_EmptyMesh);
			s_EmptyMesh = null;
		}
	}

	// ---------------------------------------------------------------------

	public TUITextureInfo texInfo
	{
		get
		{
			TUI component = base.transform.root.gameObject.GetComponent<TUI>();
			if (null == component || m_texture == null)
			{
				return null;
			}
			if (onlyUseRetinaTexture)
			{
				return component.GetTextureInfo(m_texture, true);
			}
			return component.GetTextureInfo(m_texture);
		}
	}

	public string texture
	{
		set
		{
			if (m_texture != value)
			{
				m_texture = value;
				base.NeedUpdate = true;
			}
		}
	}

	public Color color
	{
		get
		{
			return m_color;
		}
		set
		{
			if (m_color != value)
			{
				base.NeedUpdate = true;
				m_color = value;
			}
		}
	}

	public float alpha
	{
		get
		{
			return m_color.a;
		}
		set
		{
			if (m_color.a != value)
			{
				base.NeedUpdate = true;
				m_color.a = value;
			}
		}
	}

	public bool flipX
	{
		get
		{
			return m_flipX;
		}
		set
		{
			if (m_flipX != value)
			{
				base.NeedUpdate = true;
				m_flipX = value;
			}
		}
	}

	public bool flipY
	{
		get
		{
			return m_flipY;
		}
		set
		{
			if (m_flipY != value)
			{
				base.NeedUpdate = true;
				m_flipY = value;
			}
		}
	}

	public bool UseCustomize
	{
		get
		{
			return useCustomize;
		}
		set
		{
			if (value != useCustomize)
			{
				useCustomize = value;
				base.NeedUpdate = true;
			}
		}
	}

	public bool GrayStyle
	{
		get
		{
			return grayStyle;
		}
		set
		{
			if (grayStyle != value)
			{
				grayStyle = value;
				SetGray(grayStyle);
			}
		}
	}

	public Texture CustomizeTexture
	{
		get
		{
			return customizeTexture;
		}
		set
		{
			if (value != customizeTexture)
			{
				customizeTexture = value;
				base.NeedUpdate = true;
			}
		}
	}

	public Rect CustomizeRect
	{
		get
		{
			return customizeRect;
		}
		set
		{
			if (value != customizeRect)
			{
				customizeRect = value;
				base.NeedUpdate = true;
			}
		}
	}

	public void Start()
	{
		InitializeHook();
		meshFilter = base.gameObject.GetComponent<MeshFilter>();
		meshRender = base.gameObject.GetComponent<MeshRenderer>();
		meshFilter.sharedMesh = EmptyMesh;
		meshRender.castShadows = false;
		meshRender.receiveShadows = false;
		UpdateMesh();
	}

	// NOTE: OnDestroy no longer destroys meshFilter.sharedMesh - meshes are now
	// shared across instances and owned by the static cache (see ClearMeshCache).

	private void LateUpdate()
	{
		if (NeedLaterUpdate)
		{
			NeedLaterUpdate = false;
			base.NeedUpdate = true;
		}
		UpdateMesh();
	}

	public void ForceUpdate()
	{
		base.NeedUpdate = true;
		UpdateMesh();
	}

	protected virtual Material CreateUITextureMaterial()
	{
		return TUITool.CreateUITextureMaterial();
	}

	protected virtual void UpdateMesh()
	{
		if (meshFilter == null || meshRender == null || !base.NeedUpdate)
		{
			return;
		}
		base.NeedUpdate = false;
		Material material;
		Rect rect;
		if (useCustomize)
		{
			if (null == customizeTexture)
			{
				meshFilter.sharedMesh = EmptyMesh;
				return;
			}
			if (null == customizeMaterial)
			{
				customizeMaterial = CreateUITextureMaterial();
			}
			customizeMaterial.mainTexture = customizeTexture;
			material = customizeMaterial;
			rect = customizeRect;
		}
		else
		{
			if (null == texInfo || null == texInfo.material || null == texInfo.material.mainTexture)
			{
				meshFilter.sharedMesh = EmptyMesh;
				return;
			}
			material = texInfo.material;
			rect = texInfo.rect;
		}
		sharedMat = material;
		baseShader = material.shader.name;
		SetGray(GrayStyle);
		float num = (float)material.mainTexture.width * 1f;
		float num2 = (float)material.mainTexture.height * 1f;
		float num3 = rect.width;
		float num4 = rect.height;
		if (useCustomize || onlyUseRetinaTexture || TUI.IsRetina())
		{
			num3 /= 2f;
			num4 /= 2f;
		}
		float num5 = num3;
		float num6 = num4;
		Vector4 vector = new Vector4(rect.xMin / num, 1f - rect.yMin / num2, rect.xMax / num, 1f - rect.yMax / num2);
		if (flipX)
		{
			float x = vector.x;
			vector.x = vector.z;
			vector.z = x;
		}
		if (flipY)
		{
			float y = vector.y;
			vector.y = vector.w;
			vector.w = y;
		}
		float num7 = (vector.x + vector.z) * 0.5f;
		float num8 = (vector.z - vector.x) / num5;
		float num9 = (vector.y + vector.w) * 0.5f;
		float num10 = (vector.y - vector.w) / num6;
		Rect rect2 = default(Rect);
		rect2.xMin = num5 * -0.5f;
		rect2.xMax = num5 * 0.5f;
		rect2.yMin = num6 * -0.5f;
		rect2.yMax = num6 * 0.5f;
		showClipRectList.Clear();
		showClipRectList.AddRange(otherClips);
		if (null != m_showClipObj)
		{
			showClipRectList.Add(m_showClipObj.GetComponent<TUIRect>());
		}
		foreach (TUIRect showClipRect in showClipRectList)
		{
			if (null != showClipRect)
			{
				Rect rectLocal = showClipRect.GetRectLocal(base.transform);
				Rect rect3 = TUIRect.RectIntersect(rect2, rectLocal);
				if (rect3.width <= 0f || rect3.height <= 0f)
				{
					meshFilter.sharedMesh = EmptyMesh;
					return;
				}
				vector.x = num7 + num8 * rect3.xMin;
				vector.z = num7 + num8 * rect3.xMax;
				vector.w = num9 + num10 * rect3.yMin;
				vector.y = num9 + num10 * rect3.yMax;
				rect2 = rect3;
			}
		}

		// ------------------------------------------------------------------
		// Build the cache key now that all geometry inputs are known.
		// ------------------------------------------------------------------
		MeshKey key = default(MeshKey);
		key.texId = material.mainTexture != null ? material.mainTexture.GetInstanceID() : 0;
		key.uv = vector;
		key.rect = rect2;
		key.color = color;
		key.pivot = (int)pivot;
		key.hasHide = false;
		key.hideRect = default(Rect);

		if (null != m_hideClipObj)
		{
			TUIRect component = m_hideClipObj.GetComponent<TUIRect>();
			if (null != component)
			{
				Rect rectLocal2 = component.GetRectLocal(base.transform);
				Rect rect4 = TUIRect.RectIntersect(rect2, rectLocal2);
				if (rect4.width > 0f && rect4.height > 0f)
				{
					key.hasHide = true;
					key.hideRect = rectLocal2;

					// Cache lookup for the hide-clip case.
					if (s_MeshCache.TryGetValue(key, out Mesh cachedHide) && cachedHide != null)
					{
						meshFilter.sharedMesh = cachedHide;
						return;
					}

					List<Vector3> list = new List<Vector3>();
					List<Vector2> list2 = new List<Vector2>();
					List<Color> list3 = new List<Color>();
					List<int> list4 = new List<int>();
					int num11 = 0;
					if (rectLocal2.xMin > rect2.xMin)
					{
						Rect rect5 = new Rect(rect2.xMin, rect2.yMin, rectLocal2.xMin - rect2.xMin, rect2.height);
						Vector4 zero = Vector4.zero;
						zero.x = num7 + num8 * rect5.xMin;
						zero.z = num7 + num8 * rect5.xMax;
						zero.w = num9 + num10 * rect5.yMin;
						zero.y = num9 + num10 * rect5.yMax;
						list.Add(new Vector3(rect5.xMin, rect5.yMax, 0f));
						list.Add(new Vector3(rect5.xMax, rect5.yMax, 0f));
						list.Add(new Vector3(rect5.xMax, rect5.yMin, 0f));
						list.Add(new Vector3(rect5.xMin, rect5.yMin, 0f));
						list2.Add(new Vector2(zero.x, zero.y));
						list2.Add(new Vector2(zero.z, zero.y));
						list2.Add(new Vector2(zero.z, zero.w));
						list2.Add(new Vector2(zero.x, zero.w));
						list3.Add(color); list3.Add(color); list3.Add(color); list3.Add(color);
						list4.Add(num11 * 4);
						list4.Add(num11 * 4 + 1);
						list4.Add(num11 * 4 + 2);
						list4.Add(num11 * 4);
						list4.Add(num11 * 4 + 2);
						list4.Add(num11 * 4 + 3);
						num11++;
					}
					if (rectLocal2.xMax < rect2.xMax)
					{
						Rect rect6 = new Rect(rectLocal2.xMax, rect2.yMin, rect2.xMax - rectLocal2.xMax, rect2.height);
						Vector4 zero2 = Vector4.zero;
						zero2.x = num7 + num8 * rect6.xMin;
						zero2.z = num7 + num8 * rect6.xMax;
						zero2.w = num9 + num10 * rect6.yMin;
						zero2.y = num9 + num10 * rect6.yMax;
						list.Add(new Vector3(rect6.xMin, rect6.yMax, 0f));
						list.Add(new Vector3(rect6.xMax, rect6.yMax, 0f));
						list.Add(new Vector3(rect6.xMax, rect6.yMin, 0f));
						list.Add(new Vector3(rect6.xMin, rect6.yMin, 0f));
						list2.Add(new Vector2(zero2.x, zero2.y));
						list2.Add(new Vector2(zero2.z, zero2.y));
						list2.Add(new Vector2(zero2.z, zero2.w));
						list2.Add(new Vector2(zero2.x, zero2.w));
						list3.Add(color); list3.Add(color); list3.Add(color); list3.Add(color);
						list4.Add(num11 * 4);
						list4.Add(num11 * 4 + 1);
						list4.Add(num11 * 4 + 2);
						list4.Add(num11 * 4);
						list4.Add(num11 * 4 + 2);
						list4.Add(num11 * 4 + 3);
						num11++;
					}
					if (rectLocal2.yMax < rect2.yMax)
					{
						Rect rect7 = new Rect(rect4.xMin, rectLocal2.yMax, rect4.width, rect2.yMax - rectLocal2.yMax);
						Vector4 zero3 = Vector4.zero;
						zero3.x = num7 + num8 * rect7.xMin;
						zero3.z = num7 + num8 * rect7.xMax;
						zero3.w = num9 + num10 * rect7.yMin;
						zero3.y = num9 + num10 * rect7.yMax;
						list.Add(new Vector3(rect7.xMin, rect7.yMax, 0f));
						list.Add(new Vector3(rect7.xMax, rect7.yMax, 0f));
						list.Add(new Vector3(rect7.xMax, rect7.yMin, 0f));
						list.Add(new Vector3(rect7.xMin, rect7.yMin, 0f));
						list2.Add(new Vector2(zero3.x, zero3.y));
						list2.Add(new Vector2(zero3.z, zero3.y));
						list2.Add(new Vector2(zero3.z, zero3.w));
						list2.Add(new Vector2(zero3.x, zero3.w));
						list3.Add(color); list3.Add(color); list3.Add(color); list3.Add(color);
						list4.Add(num11 * 4);
						list4.Add(num11 * 4 + 1);
						list4.Add(num11 * 4 + 2);
						list4.Add(num11 * 4);
						list4.Add(num11 * 4 + 2);
						list4.Add(num11 * 4 + 3);
						num11++;
					}
					if (rect2.yMin < rectLocal2.yMin)
					{
						Rect rect8 = new Rect(rect4.xMin, rect2.yMin, rect4.width, rectLocal2.yMin - rect2.yMin);
						Vector4 zero4 = Vector4.zero;
						zero4.x = num7 + num8 * rect8.xMin;
						zero4.z = num7 + num8 * rect8.xMax;
						zero4.w = num9 + num10 * rect8.yMin;
						zero4.y = num9 + num10 * rect8.yMax;
						list.Add(new Vector3(rect8.xMin, rect8.yMax, 0f));
						list.Add(new Vector3(rect8.xMax, rect8.yMax, 0f));
						list.Add(new Vector3(rect8.xMax, rect8.yMin, 0f));
						list.Add(new Vector3(rect8.xMin, rect8.yMin, 0f));
						list2.Add(new Vector2(zero4.x, zero4.y));
						list2.Add(new Vector2(zero4.z, zero4.y));
						list2.Add(new Vector2(zero4.z, zero4.w));
						list2.Add(new Vector2(zero4.x, zero4.w));
						list3.Add(color); list3.Add(color); list3.Add(color); list3.Add(color);
						list4.Add(num11 * 4);
						list4.Add(num11 * 4 + 1);
						list4.Add(num11 * 4 + 2);
						list4.Add(num11 * 4);
						list4.Add(num11 * 4 + 2);
						list4.Add(num11 * 4 + 3);
						num11++;
					}

					Mesh builtHide = new Mesh();
					builtHide.hideFlags = HideFlags.DontSave;
					builtHide.vertices = list.ToArray();
					builtHide.uv = list2.ToArray();
					builtHide.colors = list3.ToArray();
					builtHide.triangles = list4.ToArray();

					meshFilter.sharedMesh = builtHide;
					s_MeshCache[key] = builtHide;
					return;
				}
			}
		}

		// ------------------------------------------------------------------
		// Cache lookup for the plain single-quad case.
		// ------------------------------------------------------------------
		if (s_MeshCache.TryGetValue(key, out Mesh cachedNormal) && cachedNormal != null)
		{
			meshFilter.sharedMesh = cachedNormal;
			return;
		}

		Vector3 zero5 = Vector3.zero;
		Matrix4x4 matrix4x = Layout(new Bounds(Vector3.zero, new Vector3(num5, num6, 0f)));

		Mesh built = new Mesh();
		built.hideFlags = HideFlags.DontSave;
		built.vertices = new Vector3[4]
		{
			matrix4x.MultiplyPoint3x4(new Vector3(rect2.xMin, rect2.yMax, 0f) - zero5),
			matrix4x.MultiplyPoint3x4(new Vector3(rect2.xMax, rect2.yMax, 0f) - zero5),
			matrix4x.MultiplyPoint3x4(new Vector3(rect2.xMax, rect2.yMin, 0f) - zero5),
			matrix4x.MultiplyPoint3x4(new Vector3(rect2.xMin, rect2.yMin, 0f) - zero5)
		};
		built.uv = new Vector2[4]
		{
			new Vector2(vector.x, vector.y),
			new Vector2(vector.z, vector.y),
			new Vector2(vector.z, vector.w),
			new Vector2(vector.x, vector.w)
		};
		built.colors = new Color[4] { color, color, color, color };
		built.triangles = new int[6] { 0, 1, 2, 0, 2, 3 };

		meshFilter.sharedMesh = built;
		s_MeshCache[key] = built;
	}

	private void SetGray(bool gray)
	{
		if (!(null != meshRender))
		{
			return;
		}
		if (Application.isPlaying)
		{
			if (gray)
			{
				if (meshRender.sharedMaterial.shader.name != grayShader)
				{
					meshRender.sharedMaterial = new Material(sharedMat);
					if (null != meshRender.sharedMaterial)
					{
						meshRender.sharedMaterial.shader = Shader.Find(grayShader);
					}
				}
			}
			else
			{
				meshRender.sharedMaterial = sharedMat;
			}
		}
		else
		{
			meshRender.sharedMaterial = sharedMat;
		}
	}

	private Matrix4x4 Layout(Bounds bounds)
	{
		Matrix4x4 identity = Matrix4x4.identity;
		Vector3 zero = Vector3.zero;
		switch (pivot)
		{
			case TUILabel.TUIPivot.Center:
				zero.x = bounds.center.x * -1f;
				zero.y = bounds.center.y * -1f;
				break;
			case TUILabel.TUIPivot.Left:
				zero.x = bounds.min.x * -1f;
				zero.y = bounds.center.y * -1f;
				break;
			case TUILabel.TUIPivot.Right:
				zero.x = bounds.max.x * -1f;
				zero.y = bounds.center.y * -1f;
				break;
			case TUILabel.TUIPivot.Top:
				zero.x = bounds.center.x * -1f;
				zero.y = bounds.max.y * -1f;
				break;
			case TUILabel.TUIPivot.Bottom:
				zero.x = bounds.center.x * -1f;
				zero.y = bounds.min.y * -1f;
				break;
			case TUILabel.TUIPivot.BottomLeft:
				zero.x = bounds.min.x * -1f;
				zero.y = bounds.min.y * -1f;
				break;
			case TUILabel.TUIPivot.TopLeft:
				zero.x = bounds.min.x * -1f;
				zero.y = bounds.max.y * -1f;
				break;
			case TUILabel.TUIPivot.TopRight:
				zero.x = bounds.max.x * -1f;
				zero.y = bounds.max.y * -1f;
				break;
			case TUILabel.TUIPivot.BottomRight:
				zero.x = bounds.max.x * -1f;
				zero.y = bounds.min.y * -1f;
				break;
		}
		identity.SetTRS(zero, Quaternion.identity, Vector3.one);
		return identity;
	}
}