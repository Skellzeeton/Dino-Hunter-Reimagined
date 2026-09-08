Shader "GGYY/Model/2COL_2TEX_AB+"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (1,1,1,1)
        _MainTex ("MainTex(RGB)", 2D) = "" {}
        _SkinColor ("Skin Color", Color) = (1,1,1,1)
        _SkinTex ("SkinTex(RGB)", 2D) = "" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
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
                float4 _MainColor;
                float4 _SkinColor;
                float4 _MainTex_ST;
                float4 _SkinTex_ST;
            CBUFFER_END
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_SkinTex);
            SAMPLER(sampler_SkinTex);
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
                output.uv = input.uv;
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
                return output;
            }
            half4 frag(Varyings input) : SV_Target
            {
                half4 color1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _MainColor;
                half4 color2 = SAMPLE_TEXTURE2D(_SkinTex, sampler_SkinTex, input.uv) * _SkinColor;
                half4 baseColor = saturate((color1 * color2 * 4.0 * color1.a * color2.a) + color1);
                half3 albedo = baseColor.rgb;
                half alpha = baseColor.a;
                half3 additionalLightContribution = 0;
                uint additionalLightsCount = GetAdditionalLightsCount();
                for (uint i = 0; i < additionalLightsCount; i++)
                {
                    Light light = GetAdditionalLight(i, input.positionWS);
                    float3 lightVector = light.direction;
                    float distance = length(lightVector);
                    float3 lightDir = lightVector / max(distance, 0.0001);
                    float atten = 1.0 / (1.0 + distance * distance);
                    float NdotL = max(0.0, dot(normalize(input.normalWS), lightDir));
                    half3 lightContribution = light.color.rgb * NdotL * atten;
                    float3 viewDir = normalize(GetCameraPositionWS() - input.positionWS);
                    float fresnelTerm = pow(1.0 - abs(dot(normalize(input.normalWS), viewDir)), 1.0);
                    half3 reflection = lightContribution * fresnelTerm * 0.75;
                    additionalLightContribution += (albedo * lightContribution) + reflection;
                }
                half3 finalColor = albedo + additionalLightContribution;
                finalColor = MixFog(finalColor, input.fogCoord);
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}