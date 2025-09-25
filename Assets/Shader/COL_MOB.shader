Shader "Triniti/Character/COL_AB-PLUS" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("MainTex(RGB)", 2D) = "white" {}
    _Brightness ("Brightness", Float) = 1.25
}
SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 50

    Pass {
        Tags { "LightMode"="Always" }

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        float4 _Color;
        float _Brightness;

        struct appdata {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f {
            float4 pos : SV_POSITION;
            float2 uv : TEXCOORD0;
        };

        v2f vert(appdata v) {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;
            return o;
        }

        fixed4 frag(v2f i) : SV_Target {
            fixed4 tex = tex2D(_MainTex, i.uv);
            fixed3 finalColor = tex.rgb * _Color.rgb * _Brightness;
            return fixed4(finalColor, tex.a * _Color.a);
        }
        ENDCG
    }
}
Fallback Off
}
