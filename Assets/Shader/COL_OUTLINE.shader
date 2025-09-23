Shader "Triniti/Character/COL_OUTLINE" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _OutlineColor ("Outline Color", Color) = (0,0,0,1)
    _Outline ("Outline width", Range (.01, 1)) = 0.01
    _MainTex ("MainTex (RGB)", 2D) = "white" {}
}

SubShader {
    Tags { "RenderType" = "Opaque" }
    LOD 200
    CGPROGRAM
    #pragma surface surf Standard fullforwardshadows
    #pragma target 3.0

    sampler2D _MainTex;
    fixed4 _Color;

    struct Input {
        float2 uv_MainTex;
    };

    void surf(Input IN, inout SurfaceOutputStandard o) {
        fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
        o.Albedo = c.rgb;
        o.Emission = c.rgb * 1.35;
    }
    ENDCG
    Pass {
        Name "OUTLINE"
        Cull Front
        ZWrite On
        ColorMask RGB
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"

        struct appdata {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float4 texcoord : TEXCOORD0;
        };

        struct v2f {
            float4 pos : POSITION;
            float4 color : COLOR;
            float2 uv : TEXCOORD0;
        };

        uniform float _Outline;
        uniform float4 _OutlineColor;
        sampler2D _MainTex;
        float4 _MainTex_ST;

        v2f vert(appdata v) {
            v2f o;
            v.vertex.xyz += v.normal * _Outline;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
            o.color = _OutlineColor;
            return o;
        }

        half4 frag(v2f i) : COLOR {
            return i.color;
        }
        ENDCG
    }
}

Fallback "Diffuse"
}
