Shader "Triniti/Character/COL_VL_AB"
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
                half3 finalColor = albedo;
                uint additionalLightsCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0; lightIndex < additionalLightsCount; lightIndex++)
                {
                    Light light = GetAdditionalLight(lightIndex, input.positionWS);
                    float3 lightVector = light.direction;
                    float distance = length(lightVector);
                    float3 lightDir = lightVector / max(distance, 0.0001);
                    float atten = 1.0 / (1.0 + distance * distance);
                    float NdotL = max(0.0, dot(normalize(input.normalWS), lightDir));
                    half3 lightContribution = light.color.rgb * NdotL * atten;
                    float3 viewDir = normalize(GetCameraPositionWS() - input.positionWS);
                    float fresnelTerm = pow(1.0 - abs(dot(normalize(input.normalWS), viewDir)), 0.5);
                    half3 reflection = lightContribution * fresnelTerm * 0.5;
                    finalColor += albedo * lightContribution + reflection;
                }
                finalColor = MixFog(finalColor, input.fogCoord);
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}