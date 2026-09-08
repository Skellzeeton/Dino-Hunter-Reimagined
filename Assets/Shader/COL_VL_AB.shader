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

                // Mimics the original ForwardBase pass: albedo without any lighting
                half3 finalColor = albedo;

                // Loop over all additional lights and add the same custom lighting
                // as the original ForwardAdd pass (diffuse + Fresnel reflection)
                uint additionalLightsCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0; lightIndex < additionalLightsCount; lightIndex++)
                {
                    Light light = GetAdditionalLight(lightIndex, input.positionWS);

                    // For point/spot lights, light.direction is the vector from surface to light
                    float3 lightVector = light.direction;
                    float distance = length(lightVector);
                    float3 lightDir = lightVector / max(distance, 0.0001);

                    // Custom attenuation: 1 / (1 + d²) (same as original)
                    float atten = 1.0 / (1.0 + distance * distance);

                    float NdotL = max(0.0, dot(normalize(input.normalWS), lightDir));
                    half3 lightContribution = light.color.rgb * NdotL * atten;

                    float3 viewDir = normalize(GetCameraPositionWS() - input.positionWS);
                    float fresnelTerm = pow(1.0 - abs(dot(normalize(input.normalWS), viewDir)), 0.5);
                    half3 reflection = lightContribution * fresnelTerm * 0.5;

                    finalColor += albedo * lightContribution + reflection;
                }

                // Apply fog (applied once to the combined result;
                // the original applied it per pass – this is the only visible approximation)
                finalColor = MixFog(finalColor, input.fogCoord);

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}