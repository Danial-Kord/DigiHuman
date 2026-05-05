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
/// Extends <see cref="BlendShapeController"/> by mapping server ARKit-style weights onto mesh blendshapes
/// named like Oculus visemes (viseme_aa, viseme_PP, …) plus mouthOpen / mouthSmile / eye blinks.
/// Bind each field to the matching blendshape index (use Skinned Mesh Index −1 to broadcast to all renderers).
/// Consonant visemes use coarse ARKit heuristics — tune gains in Play mode.
/// </summary>
public class VisemeBlendShapeController : BlendShapeController
{
    [Header("Viseme routing")]
    [Tooltip("When true, zero ARKit mouth/lip/jaw blendshapes on the mesh after the base pass so visemes drive the mouth.")]
    [SerializeField] private bool suppressArkitMouthOnMesh = true;

    [Tooltip("Apply mesh mouth/smile/blinks, silence, consonants, laughter, and .001 duplicates.")]
    [SerializeField] private bool driveExtendedMeshAndVisemes = true;

    [Tooltip("How aa, oo, ee interact before applying to mesh.")]
    [SerializeField] private VisemeNormalizationMode normalizationMode = VisemeNormalizationMode.NormalizeTriplet;

    [Header("Natural speaking")]
    [Tooltip("After normalization, vowel weights are curved so highs don't slam to 100 (pow curve).")]
    [SerializeField] private float vowelEaseExponent = 1.55f;

    [Tooltip("Extra multiplier on vowel outputs after easing (0–1).")]
    [Range(0.15f, 1f)]
    [SerializeField] private float vowelStrengthMultiplier = 0.58f;

    [Tooltip("Hard cap on vowel-driven weights (aa / oo / ee splits).")]
    [SerializeField] private float vowelMaxWeight = 62f;

    [Tooltip("NormalizeTriplet scales peaks to this instead of 100 so vowels stay conversational.")]
    [SerializeField] private float normalizeTripletPeak = 76f;

    [Tooltip("mesh mouthOpen / mouthSmile curve: exponent & upper cap (0–100 scale before Unity blendshape).")]
    [SerializeField] private float meshEaseExponent = 1.35f;

    [SerializeField] private float meshMouthOpenCap = 48f;
    [SerializeField] private float meshSmileCap = 42f;

    [Tooltip("Multiplies all consonant / laughter heuristic outputs.")]
    [Range(0.05f, 1f)]
    [SerializeField] private float consonantStrengthMultiplier = 0.38f;

    [Tooltip("Max weight for any single consonant viseme heuristic.")]
    [SerializeField] private float consonantMaxWeight = 44f;

    [Tooltip("Max weight applied to viseme_sil.")]
    [SerializeField] private float silenceMaxWeight = 58f;

    [Header("Vowel visemes (primary targets)")]
    [SerializeField] private BlendShape visemeAa;
    [SerializeField] private BlendShape visemeO;
    [SerializeField] private BlendShape visemeU;
    [SerializeField] private BlendShape visemeE;
    [SerializeField] private BlendShape visemeI;

    [Tooltip("Fraction of oo strength applied to viseme_O (rest goes to viseme_U).")]
    [Range(0f, 1f)]
    [SerializeField] private float ooShareForO = 0.55f;

    [Tooltip("Fraction of ee strength applied to viseme_E (rest goes to viseme_I).")]
    [Range(0f, 1f)]
    [SerializeField] private float eeShareForE = 0.55f;

    [Header("Mesh expressions (mouthOpen, mouthSmile, eyeBlink*)")]
    [SerializeField] private BlendShape meshMouthOpen;
    [SerializeField] private BlendShape meshMouthSmile;
    [SerializeField] private BlendShape meshEyeBlinkLeft;
    [SerializeField] private BlendShape meshEyeBlinkRight;

    [Header("Viseme — silence")]
    [SerializeField] private BlendShape visemeSil;

    [Header("Viseme — consonants (primary mesh)")]
    [SerializeField] private BlendShape visemePP;
    [SerializeField] private BlendShape visemeFF;
    [SerializeField] private BlendShape visemeTH;
    [SerializeField] private BlendShape visemeDD;
    [SerializeField] private BlendShape visemeKk;
    [SerializeField] private BlendShape visemeCH;
    [SerializeField] private BlendShape visemeSS;
    [SerializeField] private BlendShape visemeNn;
    [SerializeField] private BlendShape visemeRR;

    [Header("Viseme — .001 duplicates (e.g. teeth / inner mouth)")]
    [SerializeField] private BlendShape visemeAa001;
    [SerializeField] private BlendShape visemeDD001;
    [SerializeField] private BlendShape visemeKk001;
    [SerializeField] private BlendShape visemeCH001;
    [SerializeField] private BlendShape visemeSS001;
    [SerializeField] private BlendShape visemeNn001;
    [SerializeField] private BlendShape visemeRR001;

    [Header("Viseme — laughter")]
    [SerializeField] private BlendShape visemeLaughter;

    [Header("Mapping: aa (open vowel)")]
    [SerializeField] private float aaMouthOpenCoeff = 0.48f;
    [SerializeField] private float aaJawOpenCoeff = 0.38f;
    [SerializeField] private float aaLipSeparationCoeff = 0.22f;
    [SerializeField] private float aaPuckerPenaltyCoeff = 0.55f;

    [Header("Mapping: oo (rounded)")]
    [SerializeField] private float ooPuckerCoeff = 0.62f;
    [SerializeField] private float ooJawOpenCoeff = 0.14f;
    [Tooltip("Caps jaw contribution to oo (ARKit jaw 0–100).")]
    [SerializeField] private float ooJawOpenCap = 58f;

    [Header("Mapping: ee (spread / smile)")]
    [SerializeField] private float eeStretchCoeff = 0.48f;
    [SerializeField] private float eeSmileCoeff = 0.35f;
    [SerializeField] private float eeLipUpperCoeff = 0.22f;

    [Header("Heuristic — silence")]
    [SerializeField] private float silGain = 0.62f;
    [SerializeField] private float silVowelSuppression = 0.72f;

    [Header("Heuristic — consonants (tune 0–2)")]
    [SerializeField] private float ppGain = 0.42f;
    [SerializeField] private float ffGain = 0.38f;
    [SerializeField] private float thGain = 0.32f;
    [SerializeField] private float ddOpenGain = 0.22f;
    [SerializeField] private float kkPuckerGain = 0.42f;
    [SerializeField] private float kkJawGain = 0.12f;
    [SerializeField] private float chGain = 0.35f;
    [SerializeField] private float ssStretchGain = 0.38f;
    [SerializeField] private float ssLipGain = 0.32f;
    [SerializeField] private float nnGain = 0.48f;
    [SerializeField] private float rrJawAsymGain = 0.42f;
    [SerializeField] private float rrRollGain = 0.28f;

    [Header("Heuristic — laughter")]
    [SerializeField] private float laughterSmileGain = 0.48f;
    [SerializeField] private float laughterCheekGain = 0.32f;
    [SerializeField] private float laughterThresholdSmile = 38f;

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

        float aaOut = SoftenVowelOutput(aa);
        float ooOut = SoftenVowelOutput(oo);
        float eeOut = SoftenVowelOutput(ee);

        ApplyVisemeWeight(visemeAa, aaOut);
        ApplyOoSplit(ooOut);
        ApplyEeSplit(eeOut);

        if (driveExtendedMeshAndVisemes)
            ApplyExtendedMeshAndVisemes(in snap, aaOut, ooOut, eeOut);
    }

    private readonly struct MouthSnapshot
    {
        public readonly float MouthOpen;
        public readonly float MouthPucker;
        public readonly float JawOpen;
        public readonly float JawLeft;
        public readonly float JawRight;
        public readonly float MouthStretchLeft;
        public readonly float MouthStretchRight;
        public readonly float MouthSmileLeft;
        public readonly float MouthSmileRight;
        public readonly float LipUpperUpLeft;
        public readonly float LipUpperUpRight;
        public readonly float LipLowerDownLeft;
        public readonly float LipLowerDownRight;
        public readonly float MouthPressLeft;
        public readonly float MouthPressRight;
        public readonly float MouthRollLower;
        public readonly float MouthRollUpper;
        public readonly float NoseSneerLeft;
        public readonly float NoseSneerRight;
        public readonly float CheekSquintLeft;
        public readonly float CheekSquintRight;
        public readonly float EyeBlinkLeft;
        public readonly float EyeBlinkRight;

        public MouthSnapshot(BlendShapeController c)
        {
            MouthOpen = c.MouthOpen.weight;
            MouthPucker = c.MouthPucker.weight;
            JawOpen = c.JawOpen.weight;
            JawLeft = c.JawLeft.weight;
            JawRight = c.JawRight.weight;
            MouthStretchLeft = c.MouthStretchLeft.weight;
            MouthStretchRight = c.MouthStretchRight.weight;
            MouthSmileLeft = c.MouthSmileLeft.weight;
            MouthSmileRight = c.MouthSmileRight.weight;
            LipUpperUpLeft = c.LipUpperUpLeft.weight;
            LipUpperUpRight = c.LipUpperUpRight.weight;
            LipLowerDownLeft = c.LipLowerDownLeft.weight;
            LipLowerDownRight = c.LipLowerDownRight.weight;
            MouthPressLeft = c.MouthPressLeft.weight;
            MouthPressRight = c.MouthPressRight.weight;
            MouthRollLower = c.MouthRollLower.weight;
            MouthRollUpper = c.MouthRollUpper.weight;
            NoseSneerLeft = c.NoseSneerLeft.weight;
            NoseSneerRight = c.NoseSneerRight.weight;
            CheekSquintLeft = c.CheekSquintLeft.weight;
            CheekSquintRight = c.CheekSquintRight.weight;
            EyeBlinkLeft = c.EyeBlinkLeft.weight;
            EyeBlinkRight = c.EyeBlinkRight.weight;
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

    /// <summary>Writes <paramref name="weight"/> to primary and optional duplicate when indices are set.</summary>
    private void ApplyPrimaryAndDuplicate(BlendShape primary, BlendShape duplicate001, float weight)
    {
        ApplyVisemeWeight(primary, weight);
        ApplyVisemeWeight(duplicate001, weight);
    }

    private void ApplyExtendedMeshAndVisemes(in MouthSnapshot snap, float aa, float oo, float ee)
    {
        float smileAvg = (snap.MouthSmileLeft + snap.MouthSmileRight) * 0.5f;
        float stretchAvg = (snap.MouthStretchLeft + snap.MouthStretchRight) * 0.5f;
        float lipUpAvg = (snap.LipUpperUpLeft + snap.LipUpperUpRight) * 0.5f;

        ApplyVisemeWeight(meshMouthOpen, SoftenMeshOpen(snap.MouthOpen));
        ApplyVisemeWeight(meshMouthSmile, SoftenMeshSmile(smileAvg));
        ApplyVisemeWeight(meshEyeBlinkLeft, snap.EyeBlinkLeft);
        ApplyVisemeWeight(meshEyeBlinkRight, snap.EyeBlinkRight);

        ApplyVisemeWeight(visemeAa001, aa);

        float vowelPeak = Mathf.Max(aa, Mathf.Max(oo, ee));
        float mouthBusy = Mathf.Clamp01((snap.MouthOpen + snap.JawOpen) * 0.0028f);
        float sil = (1f - Mathf.Clamp01(vowelPeak / 100f) * silVowelSuppression - mouthBusy * 0.55f) * 100f * silGain;
        ApplyVisemeWeight(visemeSil, Mathf.Clamp(sil * consonantStrengthMultiplier, 0f, silenceMaxWeight));

        float press = (snap.MouthPressLeft + snap.MouthPressRight) * 0.5f;
        ApplyVisemeWeight(visemePP, ClampConsonant(press * ppGain));
        ApplyVisemeWeight(visemeFF, ClampConsonant((lipUpAvg * 0.55f + stretchAvg * 0.32f) * ffGain));
        ApplyVisemeWeight(visemeTH, ClampConsonant(((snap.MouthRollLower + snap.MouthRollUpper) * 0.5f) * thGain));

        float ddW = ClampConsonant(Mathf.Min(snap.MouthOpen, 48f) * ddOpenGain + press * 0.2f);
        ApplyPrimaryAndDuplicate(visemeDD, visemeDD001, ddW);

        float kkW = ClampConsonant(snap.MouthPucker * kkPuckerGain + Mathf.Min(snap.JawOpen, 42f) * kkJawGain);
        ApplyPrimaryAndDuplicate(visemeKk, visemeKk001, kkW);

        float smileInv = Mathf.Clamp(48f - smileAvg * 0.48f, 0f, 48f);
        float chW = ClampConsonant(snap.MouthPucker * chGain + smileInv * 0.16f);
        ApplyPrimaryAndDuplicate(visemeCH, visemeCH001, chW);

        float ssW = ClampConsonant(stretchAvg * ssStretchGain + lipUpAvg * ssLipGain * 0.32f);
        ApplyPrimaryAndDuplicate(visemeSS, visemeSS001, ssW);

        float noseAvg = (snap.NoseSneerLeft + snap.NoseSneerRight) * 0.5f;
        float nnW = ClampConsonant(noseAvg * nnGain + Mathf.Clamp(68f - snap.MouthOpen, 0f, 68f) * 0.11f);
        ApplyPrimaryAndDuplicate(visemeNn, visemeNn001, nnW);

        float rrW = ClampConsonant(
            Mathf.Abs(snap.JawLeft - snap.JawRight) * rrJawAsymGain + snap.MouthRollLower * rrRollGain);
        ApplyPrimaryAndDuplicate(visemeRR, visemeRR001, rrW);

        float cheekAvg = (snap.CheekSquintLeft + snap.CheekSquintRight) * 0.5f;
        float laugh = Mathf.Clamp(
            Mathf.Max(0f, smileAvg - laughterThresholdSmile) * laughterSmileGain + cheekAvg * laughterCheekGain,
            0f,
            100f);
        ApplyVisemeWeight(visemeLaughter, ClampConsonant(laugh));
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
        const float cap = 100f;
        if (normalizationMode == VisemeNormalizationMode.IndependentClamp)
        {
            aa = Mathf.Clamp(aa, 0f, cap);
            oo = Mathf.Clamp(oo, 0f, cap);
            ee = Mathf.Clamp(ee, 0f, cap);
            return;
        }

        float max = Mathf.Max(Mathf.Max(aa, oo), ee);
        const float eps = 1e-4f;
        if (max <= eps)
            return;

        float peak = Mathf.Clamp(normalizeTripletPeak, 35f, 100f);
        float scale = peak / max;
        aa *= scale;
        oo *= scale;
        ee *= scale;
    }

    /// <summary>Compresses strong ARKit peaks so lip shapes stay in conversational range.</summary>
    private float SoftenVowelOutput(float weight0To100)
    {
        float n = Mathf.Clamp01(weight0To100 / 100f);
        float exp = Mathf.Max(vowelEaseExponent, 1.01f);
        float eased = Mathf.Pow(n, exp) * 100f * vowelStrengthMultiplier;
        return Mathf.Min(eased, vowelMaxWeight);
    }

    private float SoftenMeshOpen(float mouthOpen0To100)
    {
        float n = Mathf.Clamp01(mouthOpen0To100 / 100f);
        float exp = Mathf.Max(meshEaseExponent, 1.01f);
        return Mathf.Min(Mathf.Pow(n, exp) * meshMouthOpenCap, meshMouthOpenCap);
    }

    private float SoftenMeshSmile(float smile0To100)
    {
        float n = Mathf.Clamp01(smile0To100 / 100f);
        float exp = Mathf.Max(meshEaseExponent, 1.01f);
        return Mathf.Min(Mathf.Pow(n, exp) * meshSmileCap, meshSmileCap);
    }

    private float ClampConsonant(float raw)
    {
        float v = raw * consonantStrengthMultiplier;
        return Mathf.Clamp(v, 0f, consonantMaxWeight);
    }
}
