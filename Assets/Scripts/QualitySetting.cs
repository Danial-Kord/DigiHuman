using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class QualitySetting : MonoSingleton<QualitySetting>
{

    [SerializeField] private RenderPipelineAsset[] renderPipelineAssets;
    [SerializeField] private Light light;
    public void SetQuality(int renderMode, float lightIntensity)
    {
        GraphicsSettings.defaultRenderPipeline = renderPipelineAssets[renderMode];
        QualitySettings.renderPipeline = renderPipelineAssets[renderMode];
        light.intensity = lightIntensity;
    }
    
}
