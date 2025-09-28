using UnityEngine;

public class iMaterialBrightnessSetter : MonoBehaviour
{
    public static float Brightness = 1.35f;
    private Renderer m_Renderer;
    private void Awake()
    {
        m_Renderer = GetComponent<Renderer>();
        if (m_Renderer != null && m_Renderer.material != null)
        {
            Shader shader = m_Renderer.material.shader;
            if (shader != null && shader.name == "Triniti/Character/COL_VL_AB")
            {
                m_Renderer.material.SetFloat("_Brightness", Brightness);
            }
        }
    }
}