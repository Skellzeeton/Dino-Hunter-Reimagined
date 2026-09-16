Shader "Triniti/Scene/COL_LM"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("MainTex", 2D) = "white" {}
        _LightMap ("Lightmap (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv_MainTex : TEXCOORD0;
                float2 uv_LightMap : TEXCOORD1;
            };
            struct v2f
            {
                float2 uv_MainTex : TEXCOORD0;
                float2 uv_LightMap : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float fogCoord : TEXCOORD2;
            };
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
            CBUFFER_END
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_LightMap);
            SAMPLER(sampler_LightMap);
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv_MainTex = TRANSFORM_TEX(v.uv_MainTex, _MainTex);
                o.uv_LightMap = v.uv_LightMap;
                o.fogCoord = ComputeFogFactor(o.vertex.z);
                return o;
            }
            half4 frag (v2f i) : SV_Target
            {
                half3 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv_MainTex).rgb * (_Color.rgb * 0.05);
                half4 lm = SAMPLE_TEXTURE2D(_LightMap, sampler_LightMap, i.uv_LightMap) * 20;
                half3 finalColor = lm.rgb * albedo;
                half alpha = lm.a * _Color.a;
                finalColor = MixFog(finalColor, i.fogCoord);
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
}