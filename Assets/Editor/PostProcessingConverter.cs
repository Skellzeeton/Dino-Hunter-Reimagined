using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.PostProcessing; // Old Post Processing Stack v2

public class PostProcessingConverter
{
    [MenuItem("Tools/Convert Post Processing Profiles to URP")]
    public static void ConvertProfiles()
    {
        string[] guids = AssetDatabase.FindAssets("t:PostProcessProfile", new[] { "Assets" });
        int convertedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PostProcessProfile oldProfile = AssetDatabase.LoadAssetAtPath<PostProcessProfile>(path);
            if (oldProfile == null)
                continue;

            // Prepare new asset path
            string directory = System.IO.Path.GetDirectoryName(path);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            string newFileName = fileName + "_URP";
            string newPath = AssetDatabase.GenerateUniqueAssetPath(System.IO.Path.Combine(directory, newFileName + ".asset"));

            // Create new VolumeProfile
            VolumeProfile newProfile = ScriptableObject.CreateInstance<VolumeProfile>();

            // Map each setting from the old profile
            foreach (var setting in oldProfile.settings)
            {
                if (setting == null)
                    continue;
                MapSetting(setting, newProfile);
            }

            // Save the new profile asset and its sub-assets
            AssetDatabase.CreateAsset(newProfile, newPath);
            // Save all added VolumeComponents as sub-assets
            foreach (var component in newProfile.components)
            {
                if (component != null)
                {
                    // Only add if not already part of the asset (it shouldn't be)
                    if (!AssetDatabase.IsSubAsset(component))
                    {
                        AssetDatabase.AddObjectToAsset(component, newProfile);
                    }
                }
            }

            convertedCount++;
            Debug.Log($"Converted: {path} -> {newPath}");
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Conversion Complete", $"Converted {convertedCount} profiles.", "OK");
    }

    // Helper: copy a parameter value and override state if the old parameter was overridden
    static void CopyParameter<T>(ParameterOverride<T> oldParam, VolumeParameter<T> newParam)
    {
        if (oldParam.overrideState)
        {
            newParam.value = oldParam.value;
            newParam.overrideState = true;
        }
        // else leave newParam.overrideState false (default)
    }

    static void MapSetting(PostProcessEffectSettings oldSetting, VolumeProfile newProfile)
    {
        // Bloom
        if (oldSetting is UnityEngine.Rendering.PostProcessing.Bloom oldBloom)
        {
            var bloom = newProfile.Add<UnityEngine.Rendering.Universal.Bloom>();
            bloom.active = oldBloom.enabled;

            CopyParameter(oldBloom.intensity, bloom.intensity);
            CopyParameter(oldBloom.threshold, bloom.threshold);
            // Soft knee maps to scatter (per user testing)
            CopyParameter(oldBloom.softKnee, bloom.scatter);
            CopyParameter(oldBloom.clamp, bloom.clamp);
            CopyParameter(oldBloom.color, bloom.tint);

            // fastMode maps inversely to highQualityFiltering
            if (oldBloom.fastMode.overrideState)
            {
                bloom.highQualityFiltering.value = !oldBloom.fastMode.value;
                bloom.highQualityFiltering.overrideState = true;
            }
        }
            // Color Grading
        else if (oldSetting is UnityEngine.Rendering.PostProcessing.ColorGrading oldCG)
        {
            // Color Adjustments
            var colorAdj = newProfile.Add<UnityEngine.Rendering.Universal.ColorAdjustments>();
            colorAdj.active = oldCG.enabled;

            CopyParameter(oldCG.postExposure, colorAdj.postExposure);
            CopyParameter(oldCG.contrast, colorAdj.contrast);
            CopyParameter(oldCG.saturation, colorAdj.saturation);
            CopyParameter(oldCG.colorFilter, colorAdj.colorFilter);

            // White Balance
            var wb = newProfile.Add<UnityEngine.Rendering.Universal.WhiteBalance>();
            wb.active = oldCG.enabled;

            CopyParameter(oldCG.temperature, wb.temperature);
            CopyParameter(oldCG.tint, wb.tint);

            // Tonemapping
            var tonemap = newProfile.Add<UnityEngine.Rendering.Universal.Tonemapping>();
            tonemap.active = oldCG.enabled;

            if (oldCG.tonemapper.overrideState)
            {
                switch (oldCG.tonemapper.value)
                {
                    case Tonemapper.None:
                        tonemap.mode.value = UnityEngine.Rendering.Universal.TonemappingMode.None;
                        break;
                    case Tonemapper.Neutral:
                        tonemap.mode.value = UnityEngine.Rendering.Universal.TonemappingMode.Neutral;
                        break;
                    case Tonemapper.ACES:
                        tonemap.mode.value = UnityEngine.Rendering.Universal.TonemappingMode.ACES;
                        break;
                    case Tonemapper.Custom:
                        Debug.LogWarning("Custom tonemapping is not supported in URP; setting to None.");
                        tonemap.mode.value = UnityEngine.Rendering.Universal.TonemappingMode.None;
                        break;
                }
                tonemap.mode.overrideState = true;
            }

            // Lift Gamma Gain (converts old trackballs)
            var liftGammaGain = newProfile.Add<UnityEngine.Rendering.Universal.LiftGammaGain>();
            liftGammaGain.active = oldCG.enabled;

            CopyParameter(oldCG.lift, liftGammaGain.lift);
            CopyParameter(oldCG.gamma, liftGammaGain.gamma);
            CopyParameter(oldCG.gain, liftGammaGain.gain);
        }
            // Grain → Film Grain
        else if (oldSetting is UnityEngine.Rendering.PostProcessing.Grain oldGrain)
        {
            var grain = newProfile.Add<UnityEngine.Rendering.Universal.FilmGrain>();
            grain.active = oldGrain.enabled;

            CopyParameter(oldGrain.intensity, grain.intensity);
            CopyParameter(oldGrain.size, grain.response);
        }
            // Vignette
        else if (oldSetting is UnityEngine.Rendering.PostProcessing.Vignette oldVig)
        {
            var vig = newProfile.Add<UnityEngine.Rendering.Universal.Vignette>();
            vig.active = oldVig.enabled;

            CopyParameter(oldVig.color, vig.color);
            CopyParameter(oldVig.intensity, vig.intensity);
            CopyParameter(oldVig.smoothness, vig.smoothness);
            CopyParameter(oldVig.rounded, vig.rounded);
        }
        else
        {
            Debug.LogWarning($"Unsupported effect type: {oldSetting.GetType().Name}. Skipping (you may need to convert manually).");
        }
    }
}