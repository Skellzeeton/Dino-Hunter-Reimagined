Shader "Triniti/Character/COL_AB-PLUS" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("MainTex", 2D) = "white" {}
    _Brightness ("Brightness", Float) = 1.25
}
SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 50

    CGPROGRAM
    #pragma surface surf Lambert noforwardadd nolightmap noshadow
    #pragma target 3.0

    sampler2D _MainTex;
    fixed4 _Color;
    float _Brightness;

    struct Input {
        float2 uv_MainTex;
    };

    void surf(Input IN, inout SurfaceOutput o) {
        fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
        fixed3 finalColor = tex.rgb * _Color.rgb * _Brightness;

        o.Albedo = 0;
        o.Emission = finalColor;
        o.Alpha = tex.a * _Color.a;
    }
    ENDCG
}
Fallback Off
}
