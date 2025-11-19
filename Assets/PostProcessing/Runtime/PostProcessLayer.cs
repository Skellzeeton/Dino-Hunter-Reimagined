using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace UnityEngine.Rendering.PostProcessing
{
#if ENABLE_VR
    using XRSettings = UnityEngine.XR.XRSettings;
#endif
    
#if UNITY_2018_3_OR_NEWER
    [ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif
    [DisallowMultipleComponent, ImageEffectAllowedInSceneView]
    [AddComponentMenu("Rendering/Post-process Layer", 1000)]
    [RequireComponent(typeof(Camera))]
    public sealed class PostProcessLayer : MonoBehaviour
    {
        public enum Antialiasing
        {
            None,
            FastApproximateAntialiasing,
            SubpixelMorphologicalAntialiasing,
            TemporalAntialiasing
        }
        public Transform volumeTrigger;
        public LayerMask volumeLayer;
        public bool stopNaNPropagation = true;
        public bool finalBlitToCameraTarget = false;
        
        public Antialiasing antialiasingMode = Antialiasing.None;
        
        public TemporalAntialiasing temporalAntialiasing;
        
        public SubpixelMorphologicalAntialiasing subpixelMorphologicalAntialiasing;
        
        public FastApproximateAntialiasing fastApproximateAntialiasing;
        
        public Fog fog;

        Dithering dithering;
        public PostProcessDebugLayer debugLayer;

        [SerializeField]
        PostProcessResources m_Resources;
        
        [NonSerialized]
        PostProcessResources m_OldResources;

        [UnityEngine.Scripting.Preserve]
        
        [SerializeField]
        bool m_ShowToolkit;

        [UnityEngine.Scripting.Preserve]
        
        [SerializeField]
        bool m_ShowCustomSorter;
        
        public bool breakBeforeColorGrading = false;
        
        public sealed class SerializedBundleRef
        {
            public string assemblyQualifiedName;
            
            public PostProcessBundle bundle; 
        }

        [SerializeField]
        List<SerializedBundleRef> m_BeforeTransparentBundles;

        [SerializeField]
        List<SerializedBundleRef> m_BeforeStackBundles;

        [SerializeField]
        List<SerializedBundleRef> m_AfterStackBundles;
        
        public Dictionary<PostProcessEvent, List<SerializedBundleRef>> sortedBundles { get; private set; }
        
        public DepthTextureMode cameraDepthFlags { get; private set; }
        
        public bool haveBundlesBeenInited { get; private set; }

        Dictionary<Type, PostProcessBundle> m_Bundles;

        PropertySheetFactory m_PropertySheetFactory;
        CommandBuffer m_LegacyCmdBufferBeforeReflections;
        CommandBuffer m_LegacyCmdBufferBeforeLighting;
        CommandBuffer m_LegacyCmdBufferOpaque;
        CommandBuffer m_LegacyCmdBuffer;
        Camera m_Camera;
        PostProcessRenderContext m_CurrentContext;
        LogHistogram m_LogHistogram;

        bool m_SettingsUpdateNeeded = true;
        bool m_IsRenderingInSceneView = false;

        TargetPool m_TargetPool;

        bool m_NaNKilled = false;

        readonly List<PostProcessEffectRenderer> m_ActiveEffects = new List<PostProcessEffectRenderer>();
        readonly List<RenderTargetIdentifier> m_Targets = new List<RenderTargetIdentifier>();

        void OnEnable()
        {
            Init(null);

            if (!haveBundlesBeenInited)
                InitBundles();

            m_LogHistogram = new LogHistogram();
            m_PropertySheetFactory = new PropertySheetFactory();
            m_TargetPool = new TargetPool();

            debugLayer.OnEnable();

            if (RuntimeUtilities.scriptableRenderPipelineActive)
                return;

            InitLegacy();
        }

        void InitLegacy()
        {
            m_LegacyCmdBufferBeforeReflections = new CommandBuffer { name = "Deferred Ambient Occlusion" };
            m_LegacyCmdBufferBeforeLighting = new CommandBuffer { name = "Deferred Ambient Occlusion" };
            m_LegacyCmdBufferOpaque = new CommandBuffer { name = "Opaque Only Post-processing" };
            m_LegacyCmdBuffer = new CommandBuffer { name = "Post-processing" };

            m_Camera = GetComponent<Camera>();

#if !UNITY_2019_1_OR_NEWER
            m_Camera.forceIntoRenderTexture = true;
#endif

            m_Camera.AddCommandBuffer(CameraEvent.BeforeReflections, m_LegacyCmdBufferBeforeReflections);
            m_Camera.AddCommandBuffer(CameraEvent.BeforeLighting, m_LegacyCmdBufferBeforeLighting);
            m_Camera.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, m_LegacyCmdBufferOpaque);
            m_Camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, m_LegacyCmdBuffer);
            m_CurrentContext = new PostProcessRenderContext();
        }

#if UNITY_2019_1_OR_NEWER
        bool DynamicResolutionAllowsFinalBlitToCameraTarget()
        { 
            return (!m_Camera.allowDynamicResolution || (ScalableBufferManager.heightScaleFactor == 1.0 && ScalableBufferManager.widthScaleFactor == 1.0));
        }
#endif

#if UNITY_2019_1_OR_NEWER
        [ImageEffectUsesCommandBuffer]
        void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            if (finalBlitToCameraTarget && DynamicResolutionAllowsFinalBlitToCameraTarget())
                RenderTexture.active = dst;
            else
                Graphics.Blit(src, dst);
        }
#endif
        
        public void Init(PostProcessResources resources)
        {
            if (resources != null) m_Resources = resources;

            RuntimeUtilities.CreateIfNull(ref temporalAntialiasing);
            RuntimeUtilities.CreateIfNull(ref subpixelMorphologicalAntialiasing);
            RuntimeUtilities.CreateIfNull(ref fastApproximateAntialiasing);
            RuntimeUtilities.CreateIfNull(ref dithering);
            RuntimeUtilities.CreateIfNull(ref fog);
            RuntimeUtilities.CreateIfNull(ref debugLayer);
        }
        
        public void InitBundles()
        {
            if (haveBundlesBeenInited)
                return;

            RuntimeUtilities.CreateIfNull(ref m_BeforeTransparentBundles);
            RuntimeUtilities.CreateIfNull(ref m_BeforeStackBundles);
            RuntimeUtilities.CreateIfNull(ref m_AfterStackBundles);
            m_Bundles = new Dictionary<Type, PostProcessBundle>();

            foreach (var type in PostProcessManager.instance.settingsTypes.Keys)
            {
                var settings = (PostProcessEffectSettings)ScriptableObject.CreateInstance(type);
                var bundle = new PostProcessBundle(settings);
                m_Bundles.Add(type, bundle);
            }

            UpdateBundleSortList(m_BeforeTransparentBundles, PostProcessEvent.BeforeTransparent);
            UpdateBundleSortList(m_BeforeStackBundles, PostProcessEvent.BeforeStack);
            UpdateBundleSortList(m_AfterStackBundles, PostProcessEvent.AfterStack);
            sortedBundles = new Dictionary<PostProcessEvent, List<SerializedBundleRef>>(new PostProcessEventComparer())
            {
                { PostProcessEvent.BeforeTransparent, m_BeforeTransparentBundles },
                { PostProcessEvent.BeforeStack,       m_BeforeStackBundles },
                { PostProcessEvent.AfterStack,        m_AfterStackBundles }
            };
            haveBundlesBeenInited = true;
        }

        void UpdateBundleSortList(List<SerializedBundleRef> sortedList, PostProcessEvent evt)
        {
            var effects = m_Bundles.Where(kvp => kvp.Value.attribute.eventType == evt && !kvp.Value.attribute.builtinEffect)
                                   .Select(kvp => kvp.Value)
                                   .ToList();
            
            sortedList.RemoveAll(x =>
            {
                string searchStr = x.assemblyQualifiedName;
                return !effects.Exists(b => b.settings.GetType().AssemblyQualifiedName == searchStr);
            });


            foreach (var effect in effects)
            {
                string typeName = effect.settings.GetType().AssemblyQualifiedName;

                if (!sortedList.Exists(b => b.assemblyQualifiedName == typeName))
                {
                    var sbr = new SerializedBundleRef { assemblyQualifiedName = typeName };
                    sortedList.Add(sbr);
                }
            }

            foreach (var effect in sortedList)
            {
                string typeName = effect.assemblyQualifiedName;
                var bundle = effects.Find(b => b.settings.GetType().AssemblyQualifiedName == typeName);
                effect.bundle = bundle;
            }
        }

        void OnDisable()
        {
            if (m_Camera != null)
            {
                if (m_LegacyCmdBufferBeforeReflections != null)
                    m_Camera.RemoveCommandBuffer(CameraEvent.BeforeReflections, m_LegacyCmdBufferBeforeReflections);
                if (m_LegacyCmdBufferBeforeLighting != null)
                    m_Camera.RemoveCommandBuffer(CameraEvent.BeforeLighting, m_LegacyCmdBufferBeforeLighting);
                if (m_LegacyCmdBufferOpaque != null)
                    m_Camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, m_LegacyCmdBufferOpaque);
                if (m_LegacyCmdBuffer != null)
                    m_Camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, m_LegacyCmdBuffer);
            }

            temporalAntialiasing.Release();
            m_LogHistogram.Release();

            foreach (var bundle in m_Bundles.Values)
                bundle.Release();

            m_Bundles.Clear();
            m_PropertySheetFactory.Release();

            if (debugLayer != null)
                debugLayer.OnDisable();

            TextureLerper.instance.Clear();

            haveBundlesBeenInited = false;
        }
        
        void Reset()
        {
            volumeTrigger = transform;
        }

        void OnPreCull()
        {
            if (RuntimeUtilities.scriptableRenderPipelineActive)
                return;

            if (m_Camera == null || m_CurrentContext == null)
                InitLegacy();

#if UNITY_2019_3_OR_NEWER
            if(SystemInfo.usesLoadStoreActions)
#else
            if(Application.isMobilePlatform)
#endif
            {
                Rect r = m_Camera.rect;
                if(Mathf.Abs(r.x) > 1e-6f || Mathf.Abs(r.y) > 1e-6f || Mathf.Abs(1.0f - r.width) > 1e-6f || Mathf.Abs(1.0f - r.height) > 1e-6f)
                {
                    Debug.LogWarning("When used with builtin render pipeline, Postprocessing package expects to be used on a fullscreen Camera.\nPlease note that using Camera viewport may result in visual artefacts or some things not working.", m_Camera);
                }
            }
            

#if UNITY_2018_2_OR_NEWER
            if (!m_Camera.usePhysicalProperties)
#endif
                m_Camera.ResetProjectionMatrix();
            m_Camera.nonJitteredProjectionMatrix = m_Camera.projectionMatrix;

#if ENABLE_VR
            if (m_Camera.stereoEnabled)
            {
                m_Camera.ResetStereoProjectionMatrices();
                Shader.SetGlobalFloat(ShaderIDs.RenderViewportScaleFactor, XRSettings.renderViewportScale);
            }
            else
#endif
            {
                Shader.SetGlobalFloat(ShaderIDs.RenderViewportScaleFactor, 1.0f);
            }

            BuildCommandBuffers();
        }

        void OnPreRender()
        {
            if (RuntimeUtilities.scriptableRenderPipelineActive ||
                (m_Camera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Right))
                return;

            BuildCommandBuffers();
        }

        static bool RequiresInitialBlit(Camera camera, PostProcessRenderContext context)
        {
            return true;

            /*
#if UNITY_2019_1_OR_NEWER
            if (camera.allowMSAA)
                return true;
            if (RuntimeUtilities.scriptableRenderPipelineActive)
                return true;

            return false;
#else
            return true;
#endif
            */
        }

        void UpdateSrcDstForOpaqueOnly(ref int src, ref int dst, PostProcessRenderContext context, RenderTargetIdentifier cameraTarget, int opaqueOnlyEffectsRemaining)
        {
            if (src > -1)
                context.command.ReleaseTemporaryRT(src);

            context.source = context.destination;
            src = dst;

            if (opaqueOnlyEffectsRemaining == 1)
            {
                context.destination = cameraTarget;
            }
            else
            {
                dst = m_TargetPool.Get();
                context.destination = dst;
                context.GetScreenSpaceTemporaryRT(context.command, dst, 0, context.sourceFormat);
            }
        }

        void BuildCommandBuffers()
        {
            var context = m_CurrentContext;
            var sourceFormat = m_Camera.allowHDR ? RuntimeUtilities.defaultHDRRenderTextureFormat : RenderTextureFormat.Default;

            if (!RuntimeUtilities.isFloatingPointFormat(sourceFormat))
                m_NaNKilled = true;

            context.Reset();
            context.camera = m_Camera;
            context.sourceFormat = sourceFormat;

            m_LegacyCmdBufferBeforeReflections.Clear();
            m_LegacyCmdBufferBeforeLighting.Clear();
            m_LegacyCmdBufferOpaque.Clear();
            m_LegacyCmdBuffer.Clear();

            SetupContext(context);

            context.command = m_LegacyCmdBufferOpaque;
            TextureLerper.instance.BeginFrame(context);
            UpdateVolumeSystem(context.camera, context.command);

            var aoBundle = GetBundle<AmbientOcclusion>();
            var aoSettings = aoBundle.CastSettings<AmbientOcclusion>();
            var aoRenderer = aoBundle.CastRenderer<AmbientOcclusionRenderer>();

            bool aoSupported = aoSettings.IsEnabledAndSupported(context);
            bool aoAmbientOnly = aoRenderer.IsAmbientOnly(context);
            bool isAmbientOcclusionDeferred = aoSupported && aoAmbientOnly;
            bool isAmbientOcclusionOpaque = aoSupported && !aoAmbientOnly;

            var ssrBundle = GetBundle<ScreenSpaceReflections>();
            var ssrSettings = ssrBundle.settings;
            var ssrRenderer = ssrBundle.renderer;
            bool isScreenSpaceReflectionsActive = ssrSettings.IsEnabledAndSupported(context);

            if (isAmbientOcclusionDeferred)
            {
                var ao = aoRenderer.Get();

                context.command = m_LegacyCmdBufferBeforeReflections;
                ao.RenderAmbientOnly(context);

                context.command = m_LegacyCmdBufferBeforeLighting;
                ao.CompositeAmbientOnly(context);
            }
            else if (isAmbientOcclusionOpaque)
            {
                context.command = m_LegacyCmdBufferOpaque;
                aoRenderer.Get().RenderAfterOpaque(context);
            }

            bool isFogActive = fog.IsEnabledAndSupported(context);
            bool hasCustomOpaqueOnlyEffects = HasOpaqueOnlyEffects(context);
            int opaqueOnlyEffects = 0;
            opaqueOnlyEffects += isScreenSpaceReflectionsActive ? 1 : 0;
            opaqueOnlyEffects += isFogActive ? 1 : 0;
            opaqueOnlyEffects += hasCustomOpaqueOnlyEffects ? 1 : 0;
            var cameraTarget = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);

            if (opaqueOnlyEffects > 0)
            {
                var cmd = m_LegacyCmdBufferOpaque;
                context.command = cmd;
                context.source = cameraTarget;
                context.destination = cameraTarget;
                int srcTarget = -1;
                int dstTarget = -1;

                UpdateSrcDstForOpaqueOnly(ref srcTarget, ref dstTarget, context, cameraTarget, opaqueOnlyEffects + 1);

                if (RequiresInitialBlit(m_Camera, context) || opaqueOnlyEffects == 1)
                {
                    cmd.BuiltinBlit(context.source, context.destination, RuntimeUtilities.copyStdMaterial, stopNaNPropagation ? 1 : 0);
                    UpdateSrcDstForOpaqueOnly(ref srcTarget, ref dstTarget, context, cameraTarget, opaqueOnlyEffects);
                }

                if (isScreenSpaceReflectionsActive)
                {
                    ssrRenderer.Render(context);
                    opaqueOnlyEffects--;
                    UpdateSrcDstForOpaqueOnly(ref srcTarget, ref dstTarget, context, cameraTarget, opaqueOnlyEffects);
                }

                if (isFogActive)
                {
                    fog.Render(context);
                    opaqueOnlyEffects--;
                    UpdateSrcDstForOpaqueOnly(ref srcTarget, ref dstTarget, context, cameraTarget, opaqueOnlyEffects);
                }

                if (hasCustomOpaqueOnlyEffects)
                    RenderOpaqueOnly(context);

                cmd.ReleaseTemporaryRT(srcTarget);
            }
            
            int tempRt = -1;
            bool forceNanKillPass = (!m_NaNKilled && stopNaNPropagation && RuntimeUtilities.isFloatingPointFormat(sourceFormat));
            if (RequiresInitialBlit(m_Camera, context) || forceNanKillPass)
            {
                tempRt = m_TargetPool.Get();
                context.GetScreenSpaceTemporaryRT(m_LegacyCmdBuffer, tempRt, 0, sourceFormat, RenderTextureReadWrite.sRGB);
                m_LegacyCmdBuffer.BuiltinBlit(cameraTarget, tempRt, RuntimeUtilities.copyStdMaterial, stopNaNPropagation ? 1 : 0);
                if (!m_NaNKilled)
                    m_NaNKilled = stopNaNPropagation;

                context.source = tempRt;
            }
            else
            {
                context.source = cameraTarget;
            }

            context.destination = cameraTarget;

#if UNITY_2019_1_OR_NEWER
            if (finalBlitToCameraTarget && !RuntimeUtilities.scriptableRenderPipelineActive && DynamicResolutionAllowsFinalBlitToCameraTarget())
            {
                if (m_Camera.targetTexture)
                {
                    context.destination = m_Camera.targetTexture.colorBuffer;
                }
                else
                {
                    context.flip = true;
                    context.destination = Display.main.colorBuffer;
                }
            }
#endif

            context.command = m_LegacyCmdBuffer;

            Render(context);

            if (tempRt > -1)
                m_LegacyCmdBuffer.ReleaseTemporaryRT(tempRt);
        }

        void OnPostRender()
        {
            if (RuntimeUtilities.scriptableRenderPipelineActive)
                return;

            if (m_CurrentContext.IsTemporalAntialiasingActive())
            {
#if UNITY_2018_2_OR_NEWER
                if (m_CurrentContext.physicalCamera)
                    m_Camera.usePhysicalProperties = true;
                else
#endif
                    m_Camera.ResetProjectionMatrix();

                if (m_CurrentContext.stereoActive)
                {
                    if (RuntimeUtilities.isSinglePassStereoEnabled || m_Camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right)
                        m_Camera.ResetStereoProjectionMatrices();
                }
            }
        }
        
        public PostProcessBundle GetBundle<T>()
            where T : PostProcessEffectSettings
        {
            return GetBundle(typeof(T));
        }

        public PostProcessBundle GetBundle(Type settingsType)
        {
            Assert.IsTrue(m_Bundles.ContainsKey(settingsType), "Invalid type");
            return m_Bundles[settingsType];
        }


        public T GetSettings<T>()
            where T : PostProcessEffectSettings
        {
            return GetBundle<T>().CastSettings<T>();
        }
        
        public void BakeMSVOMap(CommandBuffer cmd, Camera camera, RenderTargetIdentifier destination, RenderTargetIdentifier? depthMap, bool invert, bool isMSAA = false)
        {
            var bundle = GetBundle<AmbientOcclusion>();
            var renderer = bundle.CastRenderer<AmbientOcclusionRenderer>().GetMultiScaleVO();
            renderer.SetResources(m_Resources);
            renderer.GenerateAOMap(cmd, camera, destination, depthMap, invert, isMSAA);
        }

        internal void OverrideSettings(List<PostProcessEffectSettings> baseSettings, float interpFactor)
        {
            foreach (var settings in baseSettings)
            {
                if (!settings.active)
                    continue;

                var target = GetBundle(settings.GetType()).settings;
                int count = settings.parameters.Count;

                for (int i = 0; i < count; i++)
                {
                    var toParam = settings.parameters[i];
                    if (toParam.overrideState)
                    {
                        var fromParam = target.parameters[i];
                        fromParam.Interp(fromParam, toParam, interpFactor);
                    }
                }
            }
        }
        
        void SetLegacyCameraFlags(PostProcessRenderContext context)
        {
            var flags = DepthTextureMode.None;

            foreach (var bundle in m_Bundles)
            {
                if (bundle.Value.settings.IsEnabledAndSupported(context))
                    flags |= bundle.Value.renderer.GetCameraFlags();
            }

            if (context.IsTemporalAntialiasingActive())
                flags |= temporalAntialiasing.GetCameraFlags();

            if (fog.IsEnabledAndSupported(context))
                flags |= fog.GetCameraFlags();

            if (debugLayer.debugOverlay != DebugOverlay.None)
                flags |= debugLayer.GetCameraFlags();

            context.camera.depthTextureMode |= flags;
            cameraDepthFlags = flags;
        }


        public void ResetHistory()
        {
            foreach (var bundle in m_Bundles)
                bundle.Value.ResetHistory();

            temporalAntialiasing.ResetHistory();
        }
        
        public bool HasOpaqueOnlyEffects(PostProcessRenderContext context)
        {
            return HasActiveEffects(PostProcessEvent.BeforeTransparent, context);
        }

        public bool HasActiveEffects(PostProcessEvent evt, PostProcessRenderContext context)
        {
            var list = sortedBundles[evt];

            foreach (var item in list)
            {
                bool enabledAndSupported = item.bundle.settings.IsEnabledAndSupported(context);

                if (context.isSceneView)
                {
                    if (item.bundle.attribute.allowInSceneView && enabledAndSupported)
                        return true;
                }
                else if (enabledAndSupported)
                {
                    return true;
                }
            }

            return false;
        }

        void SetupContext(PostProcessRenderContext context)
        {
            if (m_OldResources != m_Resources)
            {
                RuntimeUtilities.UpdateResources(m_Resources);
                m_OldResources = m_Resources;
            }

            m_IsRenderingInSceneView = context.camera.cameraType == CameraType.SceneView;
            context.isSceneView = m_IsRenderingInSceneView;
            context.resources = m_Resources;
            context.propertySheets = m_PropertySheetFactory;
            context.debugLayer = debugLayer;
            context.antialiasing = antialiasingMode;
            context.temporalAntialiasing = temporalAntialiasing;
            context.logHistogram = m_LogHistogram;

#if UNITY_2018_2_OR_NEWER
            context.physicalCamera = context.camera.usePhysicalProperties;
#endif

            SetLegacyCameraFlags(context);
            debugLayer.SetFrameSize(context.width, context.height);
            m_CurrentContext = context;
        }

        public void UpdateVolumeSystem(Camera cam, CommandBuffer cmd)
        {
            if (m_SettingsUpdateNeeded)
            {
                cmd.BeginSample("VolumeBlending");
                PostProcessManager.instance.UpdateSettings(this, cam);
                cmd.EndSample("VolumeBlending");
                m_TargetPool.Reset();
                
                if (RuntimeUtilities.scriptableRenderPipelineActive)
                    Shader.SetGlobalFloat(ShaderIDs.RenderViewportScaleFactor, 1f);
            }

            m_SettingsUpdateNeeded = false;
        }
        
        public void RenderOpaqueOnly(PostProcessRenderContext context)
        {
            if (RuntimeUtilities.scriptableRenderPipelineActive)
                SetupContext(context);

            TextureLerper.instance.BeginFrame(context);
            UpdateVolumeSystem(context.camera, context.command);

            RenderList(sortedBundles[PostProcessEvent.BeforeTransparent], context, "OpaqueOnly");
        }

        public void Render(PostProcessRenderContext context)
        {
            if (RuntimeUtilities.scriptableRenderPipelineActive)
                SetupContext(context);

            TextureLerper.instance.BeginFrame(context);
            var cmd = context.command;
            UpdateVolumeSystem(context.camera, context.command);
            int lastTarget = -1;
            RenderTargetIdentifier cameraTexture = context.source;

#if UNITY_2019_1_OR_NEWER
            if (context.stereoActive && context.numberOfEyes > 1 && context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePass)
            {
                cmd.SetSinglePassStereo(SinglePassStereoMode.None);
                cmd.DisableShaderKeyword("UNITY_SINGLE_PASS_STEREO");
            }
#endif

            for (int eye = 0; eye < context.numberOfEyes; eye++)
            {
                bool preparedStereoSource = false;

                if (stopNaNPropagation && !m_NaNKilled)
                {
                    lastTarget = m_TargetPool.Get();
                    context.GetScreenSpaceTemporaryRT(cmd, lastTarget, 0, context.sourceFormat);
                    if (context.stereoActive && context.numberOfEyes > 1)
                    {
                        if (context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePassInstanced)
                        {
                            cmd.BlitFullscreenTriangleFromTexArray(context.source, lastTarget, RuntimeUtilities.copyFromTexArraySheet, 1, false, eye);
                            preparedStereoSource = true;
                        }
                        else if (context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePass)
                        {
                            cmd.BlitFullscreenTriangleFromDoubleWide(context.source, lastTarget, RuntimeUtilities.copyStdFromDoubleWideMaterial, 1, eye);
                            preparedStereoSource = true;
                        }
                    }
                    else
                        cmd.BlitFullscreenTriangle(context.source, lastTarget, RuntimeUtilities.copySheet, 1);
                    context.source = lastTarget;
                    m_NaNKilled = true;
                }

                if (!preparedStereoSource && context.numberOfEyes > 1)
                {
                    lastTarget = m_TargetPool.Get();
                    context.GetScreenSpaceTemporaryRT(cmd, lastTarget, 0, context.sourceFormat);
                    if (context.stereoActive)
                    {
                        if (context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePassInstanced)
                        {
                            cmd.BlitFullscreenTriangleFromTexArray(context.source, lastTarget, RuntimeUtilities.copyFromTexArraySheet, 1, false, eye);
                            preparedStereoSource = true;
                        }
                        else if (context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePass)
                        {
                            cmd.BlitFullscreenTriangleFromDoubleWide(context.source, lastTarget, RuntimeUtilities.copyStdFromDoubleWideMaterial, stopNaNPropagation ? 1 : 0, eye);
                            preparedStereoSource = true;
                        }
                    }
                    context.source = lastTarget;
                }

                if (context.IsTemporalAntialiasingActive())
                {
                    if (!RuntimeUtilities.scriptableRenderPipelineActive)
                    {
                        if (context.stereoActive)
                        {
                            if (context.camera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Right)
                                temporalAntialiasing.ConfigureStereoJitteredProjectionMatrices(context);
                        }
                        else
                        {
                            temporalAntialiasing.ConfigureJitteredProjectionMatrix(context);
                        }
                    }

                    var taaTarget = m_TargetPool.Get();
                    var finalDestination = context.destination;
                    context.GetScreenSpaceTemporaryRT(cmd, taaTarget, 0, context.sourceFormat);
                    context.destination = taaTarget;
                    temporalAntialiasing.Render(context);
                    context.source = taaTarget;
                    context.destination = finalDestination;

                    if (lastTarget > -1)
                        cmd.ReleaseTemporaryRT(lastTarget);

                    lastTarget = taaTarget;
                }

                bool hasBeforeStackEffects = HasActiveEffects(PostProcessEvent.BeforeStack, context);
                bool hasAfterStackEffects = HasActiveEffects(PostProcessEvent.AfterStack, context) && !breakBeforeColorGrading;
                bool needsFinalPass = (hasAfterStackEffects
                    || (antialiasingMode == Antialiasing.FastApproximateAntialiasing) || (antialiasingMode == Antialiasing.SubpixelMorphologicalAntialiasing && subpixelMorphologicalAntialiasing.IsSupported()))
                    && !breakBeforeColorGrading;

                if (hasBeforeStackEffects)
                    lastTarget = RenderInjectionPoint(PostProcessEvent.BeforeStack, context, "BeforeStack", lastTarget);

                lastTarget = RenderBuiltins(context, !needsFinalPass, lastTarget, eye);

                if (hasAfterStackEffects)
                    lastTarget = RenderInjectionPoint(PostProcessEvent.AfterStack, context, "AfterStack", lastTarget);

                if (needsFinalPass)
                    RenderFinalPass(context, lastTarget, eye);

                if (context.stereoActive)
                    context.source = cameraTexture;
            }

#if UNITY_2019_1_OR_NEWER
            if (context.stereoActive && context.numberOfEyes > 1 && context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePass)
            {
                cmd.SetSinglePassStereo(SinglePassStereoMode.SideBySide);
                cmd.EnableShaderKeyword("UNITY_SINGLE_PASS_STEREO");
            }
#endif

            debugLayer.RenderSpecialOverlays(context);
            debugLayer.RenderMonitors(context);

            TextureLerper.instance.EndFrame();
            debugLayer.EndFrame();
            m_SettingsUpdateNeeded = true;
            m_NaNKilled = false;
        }

        int RenderInjectionPoint(PostProcessEvent evt, PostProcessRenderContext context, string marker, int releaseTargetAfterUse = -1)
        {
            int tempTarget = m_TargetPool.Get();
            var finalDestination = context.destination;

            var cmd = context.command;
            context.GetScreenSpaceTemporaryRT(cmd, tempTarget, 0, context.sourceFormat);
            context.destination = tempTarget;
            RenderList(sortedBundles[evt], context, marker);
            context.source = tempTarget;
            context.destination = finalDestination;

            if (releaseTargetAfterUse > -1)
                cmd.ReleaseTemporaryRT(releaseTargetAfterUse);

            return tempTarget;
        }

        void RenderList(List<SerializedBundleRef> list, PostProcessRenderContext context, string marker)
        {
            var cmd = context.command;
            cmd.BeginSample(marker);

            m_ActiveEffects.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                var effect = list[i].bundle;
                if (effect.settings.IsEnabledAndSupported(context))
                {
                    if (!context.isSceneView || (context.isSceneView && effect.attribute.allowInSceneView))
                        m_ActiveEffects.Add(effect.renderer);
                }
            }
            int count = m_ActiveEffects.Count;
            if (count == 1)
            {
                m_ActiveEffects[0].Render(context);
            }
            else
            {
                m_Targets.Clear();
                m_Targets.Add(context.source);

                int tempTarget1 = m_TargetPool.Get();
                int tempTarget2 = m_TargetPool.Get();

                for (int i = 0; i < count - 1; i++)
                    m_Targets.Add(i % 2 == 0 ? tempTarget1 : tempTarget2);

                m_Targets.Add(context.destination);
                context.GetScreenSpaceTemporaryRT(cmd, tempTarget1, 0, context.sourceFormat);
                if (count > 2)
                    context.GetScreenSpaceTemporaryRT(cmd, tempTarget2, 0, context.sourceFormat);

                for (int i = 0; i < count; i++)
                {
                    context.source = m_Targets[i];
                    context.destination = m_Targets[i + 1];
                    m_ActiveEffects[i].Render(context);
                }

                cmd.ReleaseTemporaryRT(tempTarget1);
                if (count > 2)
                    cmd.ReleaseTemporaryRT(tempTarget2);
            }

            cmd.EndSample(marker);
        }

        void ApplyFlip(PostProcessRenderContext context, MaterialPropertyBlock properties)
        {
            if (context.flip && !context.isSceneView)
                properties.SetVector(ShaderIDs.UVTransform, new Vector4(1.0f, 1.0f, 0.0f, 0.0f));
            else
                ApplyDefaultFlip(properties);
        }

        void ApplyDefaultFlip(MaterialPropertyBlock properties)
        {
            properties.SetVector(ShaderIDs.UVTransform, SystemInfo.graphicsUVStartsAtTop ? new Vector4(1.0f, -1.0f, 0.0f, 1.0f) : new Vector4(1.0f, 1.0f, 0.0f, 0.0f));
        }

        int RenderBuiltins(PostProcessRenderContext context, bool isFinalPass, int releaseTargetAfterUse = -1, int eye = -1)
        {
            var uberSheet = context.propertySheets.Get(context.resources.shaders.uber);
            uberSheet.ClearKeywords();
            uberSheet.properties.Clear();
            context.uberSheet = uberSheet;
            context.autoExposureTexture = RuntimeUtilities.whiteTexture;
            context.bloomBufferNameID = -1;

            if (isFinalPass && context.stereoActive && context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePassInstanced)
                uberSheet.EnableKeyword("STEREO_INSTANCING_ENABLED");

            var cmd = context.command;
            cmd.BeginSample("BuiltinStack");

            int tempTarget = -1;
            var finalDestination = context.destination;

            if (!isFinalPass)
            {
                tempTarget = m_TargetPool.Get();
                context.GetScreenSpaceTemporaryRT(cmd, tempTarget, 0, context.sourceFormat);
                context.destination = tempTarget;
                if (antialiasingMode == Antialiasing.FastApproximateAntialiasing && !fastApproximateAntialiasing.keepAlpha)
                    uberSheet.properties.SetFloat(ShaderIDs.LumaInAlpha, 1f);
            }
            
            int depthOfFieldTarget = RenderEffect<DepthOfField>(context, true);
            int motionBlurTarget = RenderEffect<MotionBlur>(context, true);
            if (ShouldGenerateLogHistogram(context))
                m_LogHistogram.Generate(context);
            RenderEffect<AutoExposure>(context);
            uberSheet.properties.SetTexture(ShaderIDs.AutoExposureTex, context.autoExposureTexture);
            RenderEffect<LensDistortion>(context);
            RenderEffect<ChromaticAberration>(context);
            RenderEffect<Bloom>(context);
            RenderEffect<Vignette>(context);
            RenderEffect<Grain>(context);

            if (!breakBeforeColorGrading)
                RenderEffect<ColorGrading>(context);

            if (isFinalPass)
            {
                uberSheet.EnableKeyword("FINALPASS");
                dithering.Render(context);
                ApplyFlip(context, uberSheet.properties);
            }
            else
            {
                ApplyDefaultFlip(uberSheet.properties);
            }

            if (context.stereoActive && context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePassInstanced)
            {
                uberSheet.properties.SetFloat(ShaderIDs.DepthSlice, eye);
                cmd.BlitFullscreenTriangleToTexArray(context.source, context.destination, uberSheet, 0, false, eye);
            }
            else if (isFinalPass && context.stereoActive && context.numberOfEyes > 1 && context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePass)
            {
                cmd.BlitFullscreenTriangleToDoubleWide(context.source, context.destination, uberSheet, 0, eye);
            }
#if LWRP_1_0_0_OR_NEWER || UNIVERSAL_1_0_0_OR_NEWER
            else if (isFinalPass)
                cmd.BlitFullscreenTriangle(context.source, context.destination, uberSheet, 0, false, context.camera.pixelRect);
#endif
            else
                cmd.BlitFullscreenTriangle(context.source, context.destination, uberSheet, 0);

            context.source = context.destination;
            context.destination = finalDestination;

            if (releaseTargetAfterUse > -1) cmd.ReleaseTemporaryRT(releaseTargetAfterUse);
            if (motionBlurTarget > -1) cmd.ReleaseTemporaryRT(motionBlurTarget);
            if (depthOfFieldTarget > -1) cmd.ReleaseTemporaryRT(depthOfFieldTarget);
            if (context.bloomBufferNameID > -1) cmd.ReleaseTemporaryRT(context.bloomBufferNameID);

            cmd.EndSample("BuiltinStack");

            return tempTarget;
        }

        void RenderFinalPass(PostProcessRenderContext context, int releaseTargetAfterUse = -1, int eye = -1)
        {
            var cmd = context.command;
            cmd.BeginSample("FinalPass");

            if (breakBeforeColorGrading)
            {
                var sheet = context.propertySheets.Get(context.resources.shaders.discardAlpha);
                if (context.stereoActive && context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePassInstanced)
                    sheet.EnableKeyword("STEREO_INSTANCING_ENABLED");

                if (context.stereoActive && context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePassInstanced)
                {
                    sheet.properties.SetFloat(ShaderIDs.DepthSlice, eye);
                    cmd.BlitFullscreenTriangleToTexArray(context.source, context.destination, sheet, 0, false, eye);
                }
                else if (context.stereoActive && context.numberOfEyes > 1 && context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePass)
                {
                    cmd.BlitFullscreenTriangleToDoubleWide(context.source, context.destination, sheet, 0, eye);
                }
                else
                    cmd.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
            }
            else
            {
                var uberSheet = context.propertySheets.Get(context.resources.shaders.finalPass);
                uberSheet.ClearKeywords();
                uberSheet.properties.Clear();
                context.uberSheet = uberSheet;
                int tempTarget = -1;

                if (context.stereoActive && context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePassInstanced)
                    uberSheet.EnableKeyword("STEREO_INSTANCING_ENABLED");

                if (antialiasingMode == Antialiasing.FastApproximateAntialiasing)
                {
                    uberSheet.EnableKeyword(fastApproximateAntialiasing.fastMode
                        ? "FXAA_LOW"
                        : "FXAA"
                    );

                    if (fastApproximateAntialiasing.keepAlpha)
                        uberSheet.EnableKeyword("FXAA_KEEP_ALPHA");
                }
                else if (antialiasingMode == Antialiasing.SubpixelMorphologicalAntialiasing && subpixelMorphologicalAntialiasing.IsSupported())
                {
                    tempTarget = m_TargetPool.Get();
                    var finalDestination = context.destination;
                    context.GetScreenSpaceTemporaryRT(context.command, tempTarget, 0, context.sourceFormat);
                    context.destination = tempTarget;
                    subpixelMorphologicalAntialiasing.Render(context);
                    context.source = tempTarget;
                    context.destination = finalDestination;
                }

                dithering.Render(context);

                ApplyFlip(context, uberSheet.properties);
                if (context.stereoActive && context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePassInstanced)
                {
                    uberSheet.properties.SetFloat(ShaderIDs.DepthSlice, eye);
                    cmd.BlitFullscreenTriangleToTexArray(context.source, context.destination, uberSheet, 0, false, eye);
                }
                else if (context.stereoActive && context.numberOfEyes > 1 && context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePass)
                {
                    cmd.BlitFullscreenTriangleToDoubleWide(context.source, context.destination, uberSheet, 0, eye);
                }
                else
#if LWRP_1_0_0_OR_NEWER || UNIVERSAL_1_0_0_OR_NEWER
                    cmd.BlitFullscreenTriangle(context.source, context.destination, uberSheet, 0, false, context.camera.pixelRect);
#else
                    cmd.BlitFullscreenTriangle(context.source, context.destination, uberSheet, 0);
#endif

                if (tempTarget > -1)
                    cmd.ReleaseTemporaryRT(tempTarget);
            }

            if (releaseTargetAfterUse > -1)
                cmd.ReleaseTemporaryRT(releaseTargetAfterUse);

            cmd.EndSample("FinalPass");
        }

        int RenderEffect<T>(PostProcessRenderContext context, bool useTempTarget = false)
            where T : PostProcessEffectSettings
        {
            var effect = GetBundle<T>();

            if (!effect.settings.IsEnabledAndSupported(context))
                return -1;

            if (m_IsRenderingInSceneView && !effect.attribute.allowInSceneView)
                return -1;

            if (!useTempTarget)
            {
                effect.renderer.Render(context);
                return -1;
            }

            var finalDestination = context.destination;
            var tempTarget = m_TargetPool.Get();
            context.GetScreenSpaceTemporaryRT(context.command, tempTarget, 0, context.sourceFormat);
            context.destination = tempTarget;
            effect.renderer.Render(context);
            context.source = tempTarget;
            context.destination = finalDestination;
            return tempTarget;
        }

        bool ShouldGenerateLogHistogram(PostProcessRenderContext context)
        {
            bool autoExpo = GetBundle<AutoExposure>().settings.IsEnabledAndSupported(context);
            bool lightMeter = debugLayer.lightMeter.IsRequestedAndSupported(context);
            return autoExpo || lightMeter;
        }
    }
}
