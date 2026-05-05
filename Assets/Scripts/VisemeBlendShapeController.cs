using UnityEngine;
/// <summary>
/// How aa / oo / ee strengths compete before writing viseme blendshapes.
/// </summary>
public enum VisemeNormalizationMode
{
    /// <summary>Each channel clamped to 0–100 independently.</summary>
    IndependentClamp,
    /// <summary>Scale all three so the largest becomes 100 (keeps relative balance).</summary>
    NormalizeTriplet,
}

/// <summary>
/// Extends <see cref="BlendShapeController"/> by mapping server ARKit-style mouth weights
/// onto Oculus-style viseme shapes (e.g. viseme_aa, viseme_O, viseme_U, viseme_E, viseme_I).
/// Eyes, brows, nose, and cheeks still use the base implementation.
/// </summary>
public class VisemeBlendShapeController : BlendShapeController
{
    [Header("Viseme routing")]
    [Tooltip("When true, zero ARKit mouth/lip/jaw blendshapes on the mesh after the base pass so only visemes drive the mouth.")]
    [SerializeField] private bool suppressArkitMouthOnMesh = true;

    [Tooltip("How aa, oo, ee interact before applying to mesh.")]
    [SerializeField] private VisemeNormalizationMode normalizationMode = VisemeNormalizationMode.NormalizeTriplet;

    [Header("Viseme blendshape targets")]
    [SerializeField] private BlendShape visemeAa;

    [SerializeField] private BlendShape visemeO;
    [SerializeField] private BlendShape visemeU;

    [SerializeField] private BlendShape visemeE;
    [SerializeField] private BlendShape visemeI;

    [Tooltip("Fraction of oo strength applied to viseme_O (rest goes to viseme_U). Ignored if the corresponding blendshape index is unset.")]
    [Range(0f, 1f)]
    [SerializeField] private float ooShareForO = 0.55f;

    [Tooltip("Fraction of ee strength applied to viseme_E (rest goes to viseme_I).")]
    [Range(0f, 1f)]
    [SerializeField] private float eeShareForE = 0.55f;

    [Header("Mapping: aa (open vowel)")]
    [SerializeField] private float aaMouthOpenCoeff = 0.85f;
    [SerializeField] private float aaJawOpenCoeff = 0.65f;
    [SerializeField] private float aaLipSeparationCoeff = 0.35f;
    [SerializeField] private float aaPuckerPenaltyCoeff = 0.9f;

    [Header("Mapping: oo (rounded)")]
    [SerializeField] private float ooPuckerCoeff = 1f;
    [SerializeField] private float ooJawOpenCoeff = 0.25f;
    [Tooltip("Caps jaw contribution to oo (ARKit jaw 0–100).")]
    [SerializeField] private float ooJawOpenCap = 70f;

    [Header("Mapping: ee (spread / smile)")]
    [SerializeField] private float eeStretchCoeff = 0.85f;
    [SerializeField] private float eeSmileCoeff = 0.55f;
    [SerializeField] private float eeLipUpperCoeff = 0.35f;

    public override void UpdateBlendShape()
    {
        MouthSnapshot snap = CaptureMouthSnapshot();

        base.UpdateBlendShape();

        if (suppressArkitMouthOnMesh)
            SuppressArkitMouthShapesOnMesh();

        float aa = ComputeAa(snap);
        float oo = ComputeOo(snap);
        float ee = ComputeEe(snap);
        ApplyNormalization(ref aa, ref oo, ref ee);

        ApplyVisemeWeight(visemeAa, aa);
        ApplyOoSplit(oo);
        ApplyEeSplit(ee);
    }

    private readonly struct MouthSnapshot
    {
        public readonly float MouthOpen;
        public readonly float MouthPucker;
        public readonly float JawOpen;
        public readonly float MouthStretchLeft;
        public readonly float MouthStretchRight;
        public readonly float MouthSmileLeft;
        public readonly float MouthSmileRight;
        public readonly float LipUpperUpLeft;
        public readonly float LipUpperUpRight;
        public readonly float LipLowerDownLeft;
        public readonly float LipLowerDownRight;

        public MouthSnapshot(BlendShapeController c)
        {
            MouthOpen = c.MouthOpen.weight;
            MouthPucker = c.MouthPucker.weight;
            JawOpen = c.JawOpen.weight;
            MouthStretchLeft = c.MouthStretchLeft.weight;
            MouthStretchRight = c.MouthStretchRight.weight;
            MouthSmileLeft = c.MouthSmileLeft.weight;
            MouthSmileRight = c.MouthSmileRight.weight;
            LipUpperUpLeft = c.LipUpperUpLeft.weight;
            LipUpperUpRight = c.LipUpperUpRight.weight;
            LipLowerDownLeft = c.LipLowerDownLeft.weight;
            LipLowerDownRight = c.LipLowerDownRight.weight;
        }
    }

    private MouthSnapshot CaptureMouthSnapshot()
    {
        return new MouthSnapshot(this);
    }

    private void ApplyOoSplit(float oo)
    {
        bool hasO = visemeO.num >= 0;
        bool hasU = visemeU.num >= 0;
        float shareO = Mathf.Clamp01(ooShareForO);
        if (hasO && hasU)
        {
            ApplyVisemeWeight(visemeO, oo * shareO);
            ApplyVisemeWeight(visemeU, oo * (1f - shareO));
        }
        else if (hasO)
            ApplyVisemeWeight(visemeO, oo);
        else if (hasU)
            ApplyVisemeWeight(visemeU, oo);
    }

    private void ApplyEeSplit(float ee)
    {
        bool hasE = visemeE.num >= 0;
        bool hasI = visemeI.num >= 0;
        float shareE = Mathf.Clamp01(eeShareForE);
        if (hasE && hasI)
        {
            ApplyVisemeWeight(visemeE, ee * shareE);
            ApplyVisemeWeight(visemeI, ee * (1f - shareE));
        }
        else if (hasE)
            ApplyVisemeWeight(visemeE, ee);
        else if (hasI)
            ApplyVisemeWeight(visemeI, ee);
    }

    private void ApplyVisemeWeight(BlendShape target, float weight0To100)
    {
        if (target.num < 0)
            return;
        UpdateBlendShapeWeight(target.skinnedMeshIndex, target.num, weight0To100);
    }

    private void SuppressArkitMouthShapesOnMesh()
    {
        Suppress(MouthSmileRight);
        Suppress(MouthSmileLeft);
        Suppress(MouthFrownRight);
        Suppress(MouthFrownLeft);
        Suppress(LipLowerDownLeft);
        Suppress(LipLowerDownRight);
        Suppress(LipUpperUpLeft);
        Suppress(LipUpperUpRight);
        Suppress(MouthLeft);
        Suppress(MouthRight);
        Suppress(MouthStretchLeft);
        Suppress(MouthStretchRight);
        Suppress(MouthLowerDownRight);
        Suppress(MouthLowerDownLeft);
        Suppress(MouthPressLeft);
        Suppress(MouthPressRight);
        Suppress(MouthOpen);
        Suppress(MouthPucker);
        Suppress(MouthShrugUpper);
        Suppress(JawOpen);
        Suppress(JawLeft);
        Suppress(JawRight);
        Suppress(MouthDimpleLeft);
        Suppress(MouthDimpleRight);
        Suppress(MouthRollLower);
        Suppress(MouthRollUpper);
    }

    private void Suppress(BlendShape blendShape)
    {
        if (blendShape.num < 0)
            return;
        UpdateBlendShapeWeight(blendShape.skinnedMeshIndex, blendShape.num, 0f);
    }

    private float ComputeAa(in MouthSnapshot s)
    {
        float lipSep = (s.LipUpperUpLeft + s.LipUpperUpRight + s.LipLowerDownLeft + s.LipLowerDownRight) * 0.25f;
        float openTerm = aaMouthOpenCoeff * s.MouthOpen + aaJawOpenCoeff * s.JawOpen + aaLipSeparationCoeff * lipSep;
        float pen = aaPuckerPenaltyCoeff * s.MouthPucker;
        return Mathf.Clamp(openTerm - pen, 0f, 100f);
    }

    private float ComputeOo(in MouthSnapshot s)
    {
        float jawPart = Mathf.Min(s.JawOpen, ooJawOpenCap);
        float v = ooPuckerCoeff * s.MouthPucker + ooJawOpenCoeff * jawPart;
        return Mathf.Clamp(v, 0f, 100f);
    }

    private float ComputeEe(in MouthSnapshot s)
    {
        float stretch = (s.MouthStretchLeft + s.MouthStretchRight) * 0.5f;
        float smile = (s.MouthSmileLeft + s.MouthSmileRight) * 0.5f;
        float lipUp = (s.LipUpperUpLeft + s.LipUpperUpRight) * 0.5f;
        float v = eeStretchCoeff * stretch + eeSmileCoeff * smile + eeLipUpperCoeff * lipUp;
        return Mathf.Clamp(v, 0f, 100f);
    }

    private void ApplyNormalization(ref float aa, ref float oo, ref float ee)
    {
        if (normalizationMode == VisemeNormalizationMode.IndependentClamp)
        {
            aa = Mathf.Clamp(aa, 0f, 100f);
            oo = Mathf.Clamp(oo, 0f, 100f);
            ee = Mathf.Clamp(ee, 0f, 100f);
            return;
        }

        float max = Mathf.Max(Mathf.Max(aa, oo), ee);
        const float eps = 1e-4f;
        if (max <= eps)
            return;

        float scale = 100f / max;
        aa *= scale;
        oo *= scale;
        ee *= scale;
    }
}
