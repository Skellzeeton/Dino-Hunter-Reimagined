Shader "Triniti/Scene/COL_LM" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("MainTex", 2D) = "" {}
    _LightMap ("Lightmap (RGB)", 2D) = "white" {}
}
SubShader {
    Tags { "RenderType" = "Opaque" }
    Pass {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 3.5
        #pragma multi_compile_fog
        #include "UnityCG.cginc"
        struct appdata {
            float4 vertex : POSITION;
            float4 uv_MainTex : TEXCOORD0;
            float2 uv_LightMap : TEXCOORD1;
        };
        struct v2f {
            float2 uv_MainTex : TEXCOORD0;
            float2 uv_LightMap : TEXCOORD1;
            float4 vertex : SV_POSITION;
            UNITY_FOG_COORDS(2)
        };
        sampler2D _MainTex;
        sampler2D _LightMap;
        fixed4 _Color;
        float4 _MainTex_ST;
        v2f vert (appdata v) {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv_MainTex = TRANSFORM_TEX(v.uv_MainTex, _MainTex);
            o.uv_LightMap = v.uv_LightMap;
            UNITY_TRANSFER_FOG(o, o.vertex);
            return o;
        }
        fixed4 frag (v2f i) : SV_Target {
            fixed3 albedo = tex2D(_MainTex, i.uv_MainTex).rgb * (_Color.rgb * 0.05);
            half4 lm = tex2D(_LightMap, i.uv_LightMap) * 20;
            fixed3 finalColor = lm.rgb * albedo;
            fixed alpha = lm.a * _Color.a;
            UNITY_APPLY_FOG(i.fogCoord, finalColor);
            return fixed4(finalColor, alpha);
        }
        ENDCG
    }
}
}
