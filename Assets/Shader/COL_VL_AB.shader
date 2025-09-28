Shader "Triniti/Character/COL_VL_AB" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("MainTex(RGB)", 2D) = "" {}
    _Brightness ("Brightness", Float) = 1.75
}

SubShader {
    Tags { "RenderType" = "Opaque" }
    LOD 200

    CGPROGRAM
    #pragma surface surf Standard fullforwardshadows
    #pragma target 3.0

    sampler2D _MainTex;
    fixed4 _Color;
    float _Brightness;

    struct Input {
        float2 uv_MainTex;
    };

    void surf(Input IN, inout SurfaceOutputStandard o) {
        fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
        o.Albedo = c.rgb;
        o.Emission = c.rgb * _Brightness;
    }
    ENDCG
}
}
