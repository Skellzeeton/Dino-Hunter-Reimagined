Shader "Hidden/PostProcessing/Bloom"
{
    HLSLINCLUDE
        #include "../StdLib.hlsl"
        #include "../Colors.hlsl"
        #include "../Sampling.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        TEXTURE2D_SAMPLER2D(_BloomTex, sampler_BloomTex);
        TEXTURE2D_SAMPLER2D(_AutoExposureTex, sampler_AutoExposureTex);

        float4 _MainTex_TexelSize;
        float  _SampleScale;
        float4 _ColorIntensity;
        float4 _Threshold;
        float4 _Params;

        // Combined prefilter helper: applies exposure, clamp and threshold in one place
        inline half4 ApplyPrefilter(half4 src, float2 uv)
        {
            half ae = SAMPLE_TEXTURE2D(_AutoExposureTex, sampler_AutoExposureTex, uv).r;
            half4 c = SafeHDR(src) * ae;
            c = min(_Params.x, c);
            c = QuadraticThreshold(c, _Threshold.x, _Threshold.yzw);
            return c;
        }

        // Choose downsample method via branching once; both use shared sampling helpers
        half4 FragPrefilter13(VaryingsDefault i) : SV_Target
        {
            half4 s = DownsampleBox13Tap(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord, UnityStereoAdjustedTexelSize(_MainTex_TexelSize).xy);
            return ApplyPrefilter(s, i.texcoord);
        }

        half4 FragPrefilter4(VaryingsDefault i) : SV_Target
        {
            half4 s = DownsampleBox4Tap(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord, UnityStereoAdjustedTexelSize(_MainTex_TexelSize).xy);
            return ApplyPrefilter(s, i.texcoord);
        }

        half4 FragDownsample13(VaryingsDefault i) : SV_Target
        {
            return DownsampleBox13Tap(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord, UnityStereoAdjustedTexelSize(_MainTex_TexelSize).xy);
        }

        half4 FragDownsample4(VaryingsDefault i) : SV_Target
        {
            return DownsampleBox4Tap(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord, UnityStereoAdjustedTexelSize(_MainTex_TexelSize).xy);
        }

        // Single upsamples that reuse the same _BloomTex sampling logic
        half4 FragUpsampleTent(VaryingsDefault i) : SV_Target
        {
            half4 bloom = UpsampleTent(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord, UnityStereoAdjustedTexelSize(_MainTex_TexelSize).xy, _SampleScale);
            half4 baseCol = SAMPLE_TEXTURE2D(_BloomTex, sampler_BloomTex, i.texcoordStereo);
            return bloom + baseCol;
        }

        half4 FragUpsampleBox(VaryingsDefault i) : SV_Target
        {
            half4 bloom = UpsampleBox(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord, UnityStereoAdjustedTexelSize(_MainTex_TexelSize).xy, _SampleScale);
            half4 baseCol = SAMPLE_TEXTURE2D(_BloomTex, sampler_BloomTex, i.texcoordStereo);
            return bloom + baseCol;
        }

        // Debug overlays keep same look but avoid extra conversions
        half4 FragDebugOverlayThreshold(VaryingsDefault i) : SV_Target
        {
            half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);
            return half4(ApplyPrefilter(c, i.texcoord).rgb, 1.0);
        }

        half4 FragDebugOverlayTent(VaryingsDefault i) : SV_Target
        {
            half4 bloom = UpsampleTent(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord, UnityStereoAdjustedTexelSize(_MainTex_TexelSize).xy, _SampleScale);
            return half4(bloom.rgb * _ColorIntensity.w * _ColorIntensity.rgb, 1.0);
        }

        half4 FragDebugOverlayBox(VaryingsDefault i) : SV_Target
        {
            half4 bloom = UpsampleBox(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord, UnityStereoAdjustedTexelSize(_MainTex_TexelSize).xy, _SampleScale);
            return half4(bloom.rgb * _ColorIntensity.w * _ColorIntensity.rgb, 1.0);
        }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass { HLSLPROGRAM #pragma vertex VertDefault #pragma fragment FragPrefilter13 ENDHLSL }
        Pass { HLSLPROGRAM #pragma vertex VertDefault #pragma fragment FragPrefilter4 ENDHLSL }
        Pass { HLSLPROGRAM #pragma vertex VertDefault #pragma fragment FragDownsample13 ENDHLSL }
        Pass { HLSLPROGRAM #pragma vertex VertDefault #pragma fragment FragDownsample4 ENDHLSL }
        Pass { HLSLPROGRAM #pragma vertex VertDefault #pragma fragment FragUpsampleTent ENDHLSL }
        Pass { HLSLPROGRAM #pragma vertex VertDefault #pragma fragment FragUpsampleBox ENDHLSL }
        Pass { HLSLPROGRAM #pragma vertex VertDefault #pragma fragment FragDebugOverlayThreshold ENDHLSL }
        Pass { HLSLPROGRAM #pragma vertex VertDefault #pragma fragment FragDebugOverlayTent ENDHLSL }
        Pass { HLSLPROGRAM #pragma vertex VertDefault #pragma fragment FragDebugOverlayBox ENDHLSL }
    }
}
