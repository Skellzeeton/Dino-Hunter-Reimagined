using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering.PostProcessing
{
    [Serializable]
    [PostProcess(typeof(BloomRenderer), "Unity/Bloom")]
    public sealed class Bloom : PostProcessEffectSettings
    {
        [Min(0f), Tooltip("Strength of the bloom filter.")]
        public FloatParameter intensity = new FloatParameter { value = 0f };

        [Min(0f), Tooltip("Filters out pixels under this level of brightness (gamma-space).")]
        public FloatParameter threshold = new FloatParameter { value = 1f };

        [Range(0f, 1f), Tooltip("Softness of the threshold.")]
        public FloatParameter softKnee = new FloatParameter { value = 0.5f };

        [Tooltip("Clamps pixels to control the bloom amount (gamma-space).")]
        public FloatParameter clamp = new FloatParameter { value = 65472f };

        [Range(1f, 10f), Tooltip("Extent of veiling effects; integer values recommended.")]
        public FloatParameter diffusion = new FloatParameter { value = 7f };

        [Range(-1f, 1f), Tooltip("Anamorphic ratio.")]
        public FloatParameter anamorphicRatio = new FloatParameter { value = 0f };

#if UNITY_2018_1_OR_NEWER
        [ColorUsage(false, true), Tooltip("Global tint of the bloom filter.")]
#else
        [ColorUsage(false, true, 0f, 8f, 0.125f, 3f), Tooltip("Global tint of the bloom filter.")]
#endif
        public ColorParameter color = new ColorParameter { value = Color.white };

        [FormerlySerializedAs("mobileOptimized")]
        [Tooltip("Lower quality for better performance.")]
        public BoolParameter fastMode = new BoolParameter { value = false };

        [Tooltip("Lens dirt texture.")]
        public TextureParameter dirtTexture = new TextureParameter { value = null };

        [Min(0f), Tooltip("Dirt intensity.")]
        public FloatParameter dirtIntensity = new FloatParameter { value = 0f };

        [Tooltip("Downscale factor for the bloom buffer. Allowed: 1, 2, 4, 8.")]
        public IntParameter downscale = new IntParameter { value = 1 };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value && intensity.value > 0f;
        }
    }

    [UnityEngine.Scripting.Preserve]
    internal sealed class BloomRenderer : PostProcessEffectRenderer<Bloom>
    {
        enum Pass
        {
            Prefilter13,
            Prefilter4,
            Downsample13,
            Downsample4,
            UpsampleTent,
            UpsampleBox,
            DebugOverlayThreshold,
            DebugOverlayTent,
            DebugOverlayBox
        }

        struct Level { internal int down, up; }
        const int kMaxPyramidSize = 16;
        Level[] m_Pyramid = new Level[kMaxPyramidSize];

        public override void Init()
        {
            for (int i = 0; i < kMaxPyramidSize; i++)
                m_Pyramid[i] = new Level { down = Shader.PropertyToID("_BloomMipDown" + i), up = Shader.PropertyToID("_BloomMipUp" + i) };
        }

        int ClampDownscale(int v)
        {
            if (v == 1 || v == 2 || v == 4 || v == 8) return v;
            return 1;
        }

        public override void Render(PostProcessRenderContext context)
        {
            var cmd = context.command;
            cmd.BeginSample("BloomPyramid");
            var sheet = context.propertySheets.Get(context.resources.shaders.bloom);
            sheet.properties.SetTexture(ShaderIDs.AutoExposureTex, context.autoExposureTexture);

            float ratio = Mathf.Clamp(settings.anamorphicRatio, -1f, 1f);
            float rw = ratio < 0f ? -ratio : 0f;
            float rh = ratio > 0f ?  ratio : 0f;

            int ds = ClampDownscale(settings.downscale.value);
            int baseTw = Mathf.FloorToInt(context.screenWidth / (2f - rw));
            int baseTh = Mathf.FloorToInt(context.screenHeight / (2f - rh));
            int tw = Mathf.Max(1, baseTw / ds);
            int th = Mathf.Max(1, baseTh / ds);

            bool singlePassDoubleWide = (context.stereoActive && (context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePass) && (context.camera.stereoTargetEye == StereoTargetEyeMask.Both));
            int twStereo = singlePassDoubleWide ? tw * 2 : tw;
            int s = Mathf.Max(twStereo, th);

            float logs = Mathf.Log(s, 2f) + Mathf.Min(settings.diffusion.value, 10f) - 10f;
            int logs_i = Mathf.FloorToInt(logs);
            int iterations = Mathf.Clamp(logs_i, 1, kMaxPyramidSize);
            float sampleScale = 0.5f + logs - logs_i;
            sheet.properties.SetFloat(ShaderIDs.SampleScale, sampleScale);

            float lthresh = Mathf.GammaToLinearSpace(settings.threshold.value);
            float knee = lthresh * settings.softKnee.value + 1e-5f;
            sheet.properties.SetVector(ShaderIDs.Threshold, new Vector4(lthresh, lthresh - knee, knee * 2f, 0.25f / knee));
            sheet.properties.SetVector(ShaderIDs.Params, new Vector4(Mathf.GammaToLinearSpace(settings.clamp.value), 0f, 0f, 0f));

            int qualityOffset = settings.fastMode ? 1 : 0;
            var lastDown = context.source;

            int curTwStereo = twStereo;
            int curTh = th;

            for (int i = 0; i < iterations; i++)
            {
                int mipDown = m_Pyramid[i].down;
                int mipUp = m_Pyramid[i].up;
                int pass = (i == 0) ? (int)Pass.Prefilter13 + qualityOffset : (int)Pass.Downsample13 + qualityOffset;

                context.GetScreenSpaceTemporaryRT(cmd, mipDown, 0, context.sourceFormat, RenderTextureReadWrite.Default, FilterMode.Bilinear, curTwStereo, curTh);
                context.GetScreenSpaceTemporaryRT(cmd, mipUp, 0, context.sourceFormat, RenderTextureReadWrite.Default, FilterMode.Bilinear, curTwStereo, curTh);
                cmd.BlitFullscreenTriangle(lastDown, mipDown, sheet, pass);

                lastDown = mipDown;

                // downsample dims
                if (singlePassDoubleWide)
                {
                    curTwStereo = (curTwStereo / 2) % 2 > 0 ? 1 + curTwStereo / 2 : curTwStereo / 2;
                }
                else curTwStereo = curTwStereo / 2;
                curTwStereo = Mathf.Max(curTwStereo, 1);
                curTh = Mathf.Max(curTh / 2, 1);
            }

            int lastUp = m_Pyramid[iterations - 1].down;
            for (int i = iterations - 2; i >= 0; i--)
            {
                int mipDown = m_Pyramid[i].down;
                int mipUp = m_Pyramid[i].up;
                cmd.SetGlobalTexture(ShaderIDs.BloomTex, mipDown);
                cmd.BlitFullscreenTriangle(lastUp, mipUp, sheet, (int)Pass.UpsampleTent + qualityOffset);
                lastUp = mipUp;
            }

            var linearColor = settings.color.value.linear;
            float intensity = RuntimeUtilities.Exp2(settings.intensity.value / 10f) - 1f;
            var shaderSettings = new Vector4(sampleScale, intensity, settings.dirtIntensity.value, iterations);

            if (context.IsDebugOverlayEnabled(DebugOverlay.BloomThreshold))
                context.PushDebugOverlay(cmd, context.source, sheet, (int)Pass.DebugOverlayThreshold);
            else if (context.IsDebugOverlayEnabled(DebugOverlay.BloomBuffer))
            {
                sheet.properties.SetVector(ShaderIDs.ColorIntensity, new Vector4(linearColor.r, linearColor.g, linearColor.b, intensity));
                context.PushDebugOverlay(cmd, m_Pyramid[0].up, sheet, (int)Pass.DebugOverlayTent + qualityOffset);
            }

            var dirtTex = settings.dirtTexture.value ?? RuntimeUtilities.blackTexture;
            var dirtRatio = (float)dirtTex.width / dirtTex.height;
            var screenRatio = (float)context.screenWidth / context.screenHeight;
            var dirtTileOffset = new Vector4(1f, 1f, 0f, 0f);

            if (dirtRatio > screenRatio)
            {
                dirtTileOffset.x = screenRatio / dirtRatio;
                dirtTileOffset.z = (1f - dirtTileOffset.x) * 0.5f;
            }
            else if (screenRatio > dirtRatio)
            {
                dirtTileOffset.y = dirtRatio / screenRatio;
                dirtTileOffset.w = (1f - dirtTileOffset.y) * 0.5f;
            }

            var uberSheet = context.uberSheet;
            if (settings.fastMode) uberSheet.EnableKeyword("BLOOM_LOW"); else uberSheet.EnableKeyword("BLOOM");
            uberSheet.properties.SetVector(ShaderIDs.Bloom_DirtTileOffset, dirtTileOffset);
            uberSheet.properties.SetVector(ShaderIDs.Bloom_Settings, shaderSettings);
            uberSheet.properties.SetColor(ShaderIDs.Bloom_Color, linearColor);
            uberSheet.properties.SetTexture(ShaderIDs.Bloom_DirtTex, dirtTex);

            cmd.SetGlobalTexture(ShaderIDs.BloomTex, lastUp);

            for (int i = 0; i < iterations; i++)
            {
                if (m_Pyramid[i].down != lastUp) cmd.ReleaseTemporaryRT(m_Pyramid[i].down);
                if (m_Pyramid[i].up != lastUp) cmd.ReleaseTemporaryRT(m_Pyramid[i].up);
            }

            cmd.EndSample("BloomPyramid");
            context.bloomBufferNameID = lastUp;
        }
    }
}
