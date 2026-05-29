using UnityEngine;
using UnityEditor;
using System.IO;

public class CameraShot : EditorWindow
{
    Camera camera;
    Vector2Int dimensions;
    TextureFormat textureFormat = TextureFormat.RGBA32;

    string path;

    [MenuItem("Tools/Camera Shot")]
    public static void ShowWindow()
    {
        GetWindow<CameraShot>("Camera Shot");
    }

    void OnGUI()
    {
        camera = (Camera)EditorGUILayout.ObjectField(camera, typeof(Camera), true);
        textureFormat = (TextureFormat)EditorGUILayout.EnumPopup("Texture Format:", textureFormat);
        dimensions = EditorGUILayout.Vector2IntField("Dimensions: ", dimensions);
        this.path = EditorGUILayout.TextField("Path: ", this.path);

        if (camera == null) return;

        if (!GUILayout.Button("Shot")) return;

        RenderTexture rt = new(dimensions.x, dimensions.y, 24);
        Texture2D tex = new(dimensions.x, dimensions.y, textureFormat, false);

        camera.targetTexture = rt;
        camera.Render();
        RenderTexture.active = rt;

        tex.ReadPixels(new Rect(0, 0, dimensions.x, dimensions.y), 0, 0);
        tex.Apply();

        camera.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);

        string path = string.Empty;

        for (uint i = 0; i < uint.MaxValue; i++)
        {
            path = System.IO.Path.Combine(Application.dataPath, this.path + "_" + i.ToString() + ".png");

            if (!File.Exists(path)) break;
        }

        if (!Directory.Exists(System.IO.Path.GetDirectoryName(path))) Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));

        File.WriteAllBytes(path, tex.EncodeToPNG());

        AssetDatabase.Refresh();
    }
}
