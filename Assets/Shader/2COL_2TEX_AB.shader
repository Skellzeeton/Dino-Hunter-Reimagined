Shader "GGYY/Model/2COL_2TEX_AB" {
Properties {
    _MainColor ("Main Color", Color) = (1,1,1,1)
    _MainTex ("MainTex(RGB)", 2D) = "" {}
    _MainBrightness ("Main Brightness", Float) = 1.75
    _SkinColor ("Skin Color", Color) = (1,1,1,1)
    _SkinTex ("SkinTex(RGB)", 2D) = "" {}
}
SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 200

    CGPROGRAM
    #pragma surface surf Standard fullforwardshadows
    #pragma target 3.0

    sampler2D _MainTex;
    sampler2D _SkinTex;
    fixed4 _MainColor;
    fixed4 _SkinColor;
    float _MainBrightness;

    struct Input {
        float2 uv_MainTex;
        float2 uv_SkinTex;
    };

    void surf(Input IN, inout SurfaceOutputStandard o) {
        fixed4 color1 = tex2D(_MainTex, IN.uv_MainTex) * _MainColor * _MainBrightness; // ✅ Apply brightness
        fixed4 color2 = tex2D(_SkinTex, IN.uv_SkinTex) * _SkinColor;
        fixed3 combined = color1.rgb * color2.rgb * 1.75;
        o.Albedo = combined;
        o.Emission = combined + color1.rgb;
        o.Alpha = color1.a * color2.a;
    }
    ENDCG
}
Fallback "Diffuse"
}
