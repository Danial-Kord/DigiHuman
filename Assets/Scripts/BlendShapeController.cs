using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendShapeController : MonoBehaviour
{
    public int blinkFunctionSelect;
    public int leftEyeNum;
    public int rightEyeNum;
    public int mouthWidNum;
    public int mouthLenNum;
    public int shockEyeNum;

    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
    public static Vector3 leftEyeShape;
    public static Vector3 rightEyeShape;
    public static Vector3 mouthShape;

    private float leftEyeWeight;
    private float rightEyeWeight;
    private float mouthWidWeight;
    private float mouthLenWeight;
    private float shockEyeWeight;
    
    void Start ()
    {
    }

    public void UpdateBlendShape()
    {
        // Blinking function
        switch(blinkFunctionSelect)
        {
            case 1:
            {
                // Blinking Function version 1 -- Piecewise Function
                // Set weight for left eye
                if(leftEyeShape[1]<0.05f)//闭眼 //参数匹配：ParameterServer -> 参数控制!闭眼
                {
                    leftEyeWeight = 100;
                }
                else if(leftEyeShape[1]<0.1f)
                {
                    leftEyeWeight = 5/leftEyeShape[1];  
                }
                else if(leftEyeShape[1]<0.2f)
                {
                    leftEyeWeight = -500*leftEyeShape[1]+100;
                }
                else
                {
                    leftEyeWeight = 0;
                }
                // Set weight for right eye
                if(rightEyeShape[1]<0.05f)
                {
                    rightEyeWeight = 100;
                }
                else if(rightEyeShape[1]<0.1f)
                {
                    rightEyeWeight = 5/rightEyeShape[1];  
                }
                else if(rightEyeShape[1]<0.2f)
                {
                    rightEyeWeight = -500*rightEyeShape[1]+100;
                }
                else
                {
                    rightEyeWeight = 0;
                }
                
                break;
            }
            case 2:
            {
                // Blinking Function version 2 -- Sigmoid Function
                leftEyeWeight = 100 - 100/(1+Mathf.Exp(-500*(leftEyeShape[1]-0.12f)));
                rightEyeWeight = 100 - 100/(1+Mathf.Exp(-500*(rightEyeShape[1]-0.12f)));
                break;
            }
        }
        
        // Shocked function
        if(rightEyeShape[1]>0.25f)//惊愕 0.25-0.35线性
        {
            shockEyeWeight = 500*rightEyeShape[1]-125;
        }
        else
        {
            shockEyeWeight = 0;
        }

        // Mouth deformation function
        if (500*mouthShape[1] < 100f)
        {
            mouthWidWeight = 500*mouthShape[1];
        }
        else
        {
            mouthWidWeight = 100f;
        }

        if(mouthShape[0] <0.1f)
        {
            mouthLenWeight = 50;
        }
        else if(mouthShape[0] < 0.4f)
        {
            mouthLenWeight = 120f-400*mouthShape[0];
        }
        else
        {
            // mouthLenWeight = 0;
        }

        // Apply deformation weights
        if(leftEyeNum != -1)
            skinnedMeshRenderer.SetBlendShapeWeight(leftEyeNum, leftEyeWeight);
        if(rightEyeNum != -1)
            skinnedMeshRenderer.SetBlendShapeWeight(rightEyeNum, rightEyeWeight);
        // skinnedMeshRenderer.SetBlendShapeWeight (shockEyeNum, shockEyeWeight);

        skinnedMeshRenderer.SetBlendShapeWeight (mouthWidNum, Mathf.Clamp(mouthWidWeight,0,100));
        skinnedMeshRenderer.SetBlendShapeWeight (mouthLenNum, Mathf.Clamp(mouthLenWeight,0,100));
    }
}