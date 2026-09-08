Shader "Triniti/Character/COL_VL_AD"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("MainTex (RGB)", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _MainTex_ST;
            CBUFFER_END
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float fogCoord : TEXCOORD3;
            };
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
                return output;
            }
            half4 frag(Varyings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half3 albedo = texColor.rgb * _Color.rgb;
                half3 finalColor = albedo * 0.35;
                float3 normalWS = normalize(input.normalWS);
                Light mainLight = GetMainLight();
                half NdotL_main = max(0.0, dot(normalWS, mainLight.direction));
                half atten_main = mainLight.distanceAttenuation * mainLight.shadowAttenuation;
                half3 diffuse_main = albedo * mainLight.color.rgb * NdotL_main * atten_main;
                finalColor += diffuse_main;
                uint additionalLightsCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0; lightIndex < additionalLightsCount; lightIndex++)
                {
                    Light light = GetAdditionalLight(lightIndex, input.positionWS);
                    half NdotL = max(0.0, dot(normalWS, light.direction));
                    half atten = light.distanceAttenuation * light.shadowAttenuation;
                    half3 diffuse = albedo * light.color.rgb * NdotL * atten;
                    finalColor += diffuse;
                }
                finalColor = MixFog(finalColor, input.fogCoord);
                return half4(finalColor, texColor.a * _Color.a);
            }
            ENDHLSL
        }
    }
    FallBack Off
}