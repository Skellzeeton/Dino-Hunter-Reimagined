Shader "GGYY/Model/2COL_2TEX_AB" {
Properties {
    _MainColor ("Main Color", Color) = (1,1,1,1)
    _MainTex ("MainTex(RGB)", 2D) = "" {}
    _SkinColor ("Skin Color", Color) = (1,1,1,1)
    _SkinTex ("SkinTex(RGB)", 2D) = "" {}
}
SubShader { 
    Pass {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"
        struct appdata_t {
            float4 vertex : POSITION;
            float2 texcoord0 : TEXCOORD0;
        };
        struct v2f {
            float4 vertex : SV_POSITION;
            float2 texcoord0 : TEXCOORD0;
        };
        sampler2D _MainTex, _SkinTex;
        float4 _MainColor, _SkinColor, _MainTex_ST, _SkinTex_ST;
        v2f vert(appdata_t v) {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.texcoord0 = v.texcoord0;
            return o;
        }
        float4 frag(v2f i) : SV_TARGET {
            float2 uvMain = TRANSFORM_TEX(i.texcoord0, _MainTex);
            float2 uvSkin = TRANSFORM_TEX(i.texcoord0, _SkinTex);
            
            float4 color1 = tex2D(_MainTex, uvMain) * _MainColor;
            float4 color2 = tex2D(_SkinTex, uvSkin) * _SkinColor;
            float4 combined = (color1 * color2 * 4.0 * color1.a * color2.a) + color1;
            return saturate(combined); 
        }
        ENDCG
    }
}
}
