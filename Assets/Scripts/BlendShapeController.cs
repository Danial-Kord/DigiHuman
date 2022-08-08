using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct BlendShape
{
    public int num;
    [HideInInspector]public float weight;
}

public class BlendShapeController : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;

    
    public int blinkFunctionSelect;

    [Header("Blend Shapes")]
    public BlendShape EyeBlinkLeft = new BlendShape(){
        num = -1,
        weight = 0
    };
    public BlendShape EyeBlinkRight = new BlendShape(){
        num = -1,
        weight = 0
    };
    public BlendShape MouthSmileRight = new BlendShape(){
        num = -1,
        weight = 0
    };
    public BlendShape MouthSmileLeft = new BlendShape(){
        num = -1,
        weight = 0
    };
    public BlendShape MouthFrownRight = new BlendShape(){
        num = -1,
        weight = 0
    };
    public BlendShape MouthFrownLeft = new BlendShape(){
        num = -1,
        weight = 0
    };
    public BlendShape MouthLeft = new BlendShape(){
        num = -1,
        weight = 0
    };
    public BlendShape MouthRight = new BlendShape(){
        num = -1,
        weight = 0
    };
    public BlendShape MouthLowerDownRight = new BlendShape(){
        num = -1,
        weight = 0
    };
    public BlendShape MouthLowerDownLeft = new BlendShape(){
        num = -1,
        weight = 0
    };
    public BlendShape MouthPressLeft = new BlendShape(){
        num = -1,
        weight = 0
    };
    public BlendShape MouthPressRight = new BlendShape(){
        num = -1,
        weight = 0
    };
    public BlendShape MouthClose = new BlendShape(){
        num = -1,
        weight = 0
    };
    public BlendShape MouthPucker = new BlendShape(){
        num = -1,
        weight = 0
    };
    public BlendShape MouthShrugUpper = new BlendShape(){
        num = -1,
        weight = 0
    };
    public BlendShape JawOpen = new BlendShape(){
        num = -1,
        weight = 0
    };
    public BlendShape JawLeft = new BlendShape(){
        num = -1,
        weight = 0
    };
    public BlendShape JawRight = new BlendShape(){
        num = -1,
        weight = 0
    };
    public BlendShape BrowDownLeft = new BlendShape(){
        num = -1,
        weight = 0
    };
    public BlendShape BrowOuterUpLeft = new BlendShape(){
        num = -1,
        weight = 0
    };
    public BlendShape BrowDownRight = new BlendShape(){
        num = -1,
        weight = 0
    };
    public BlendShape BrowOuterUpRight = new BlendShape(){
        num = -1,
        weight = 0
    };
    public BlendShape CheekSquintRight = new BlendShape(){
        num = -1,
        weight = 0
    };
    public BlendShape CheekSquintLeft = new BlendShape(){
        num = -1,
        weight = 0
    }    ;
    
    
    public void UpdateBlendShape()
    {
        // Apply deformation weights
        UpdateBlendShapeWeight(EyeBlinkLeft.num,EyeBlinkLeft.weight);
        UpdateBlendShapeWeight(EyeBlinkRight.num,EyeBlinkRight.weight);
        UpdateBlendShapeWeight(MouthSmileRight.num,MouthSmileRight.weight);
        UpdateBlendShapeWeight(MouthSmileLeft.num,MouthSmileLeft.weight);
        UpdateBlendShapeWeight(MouthFrownRight.num,MouthFrownRight.weight);
        UpdateBlendShapeWeight(MouthFrownLeft.num,MouthFrownLeft.weight);
        UpdateBlendShapeWeight(MouthLeft.num,MouthLeft.weight);
        UpdateBlendShapeWeight(MouthRight.num,MouthRight.weight);
        UpdateBlendShapeWeight(MouthLowerDownRight.num,MouthLowerDownRight.weight);
        UpdateBlendShapeWeight(MouthLowerDownLeft.num,MouthLowerDownLeft.weight);
        UpdateBlendShapeWeight(MouthPressLeft.num,MouthPressLeft.weight);
        UpdateBlendShapeWeight(MouthPressRight.num,MouthPressRight.weight);
        UpdateBlendShapeWeight(MouthClose.num,MouthClose.weight);
        UpdateBlendShapeWeight(MouthPucker.num,MouthPucker.weight);
        UpdateBlendShapeWeight(MouthShrugUpper.num,MouthShrugUpper.weight);
        UpdateBlendShapeWeight(JawOpen.num,JawOpen.weight);
        UpdateBlendShapeWeight(JawLeft.num,JawLeft.weight);
        UpdateBlendShapeWeight(JawRight.num,JawRight.weight);
        UpdateBlendShapeWeight(BrowDownLeft.num,BrowDownLeft.weight);
        UpdateBlendShapeWeight(BrowOuterUpLeft.num,BrowOuterUpLeft.weight);
        UpdateBlendShapeWeight(BrowDownRight.num,BrowDownRight.weight);
        UpdateBlendShapeWeight(BrowOuterUpRight.num,BrowOuterUpRight.weight);
        UpdateBlendShapeWeight(CheekSquintRight.num,CheekSquintRight.weight);
        UpdateBlendShapeWeight(CheekSquintLeft.num,CheekSquintLeft.weight);
    }

    private void UpdateBlendShapeWeight(int blendNum, float blendWeight)
    {
        if (blendNum != -1)
        {
            skinnedMeshRenderer.SetBlendShapeWeight(blendNum, blendWeight);
        }
    }
}