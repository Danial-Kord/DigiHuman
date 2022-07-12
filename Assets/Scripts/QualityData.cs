using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class QualityData : MonoBehaviour
{

    [SerializeField] private int renderMode;
    [SerializeField] private float lightIntensity = 1;

    public void OnApplyQualityData()
    {
        QualitySetting.Instancce.SetQuality(renderMode,lightIntensity);
    }
}
