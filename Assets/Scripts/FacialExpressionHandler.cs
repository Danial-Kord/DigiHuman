using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum BlendShapes : int
{
   EyeBlinkLeft,
   EyeBlinkRight,
   EyeSquintLeft,
   EyeSquintRight,
   EyeWideLeft,
   EyeWideRight,
   MouthSmileRight,
   MouthSmileLeft,
   MouthDimpleLeft,
   MouthDimpleRight,
   MouthFrownRight,
   MouthFrownLeft,
   LipLowerDownLeft,
   LipLowerDownRight,
   LipUpperUpLeft,
   LipUpperUpRight,
   MouthLeft,
   MouthRight,
   MouthStretchLeft,
   MouthStretchRight,
   MouthLowerDownRight,
   MouthLowerDownLeft,
   MouthPressLeft,
   MouthPressRight,
   MouthOpen,
   MouthPucker,
   MouthShrugUpper,
   JawOpen,
   JawLeft,
   JawRight,
   BrowDownLeft,
   BrowOuterUpLeft,
   BrowDownRight,
   BrowOuterUpRight,
   CheekSquintRight,
   CheekSquintLeft,
   MouthRollLower,
   MouthRollUpper,
   NoseSneerLeft,
   NoseSneerRight
}//23 nodes for now

[Serializable]
public class FaceBlendData
{
    //BlendWeight
    public float newWeight;
    public float weight;
    public ControlObject controlObject;
}


public class FacialExpressionHandler : MonoBehaviour
{
    // Control objects
    ControlObject LeftEyeControl = new ControlObject();
    ControlObject RightEyeControl = new ControlObject();
    ControlObject MouthWidControl = new ControlObject();
    ControlObject MouthLenControl = new ControlObject();

    //BlendNodes
    private FaceBlendData[] faceBlendNodes;

    [SerializeField] private GameObject character;
    private BlendShapeController BlendShapeController;
    // Start is called before the first frame update
    void Start()
    {

        Initialization();

    }

    private void Initialization()
    {
        BlendShapeController = character.GetComponentInChildren<BlendShapeController>();
        
        faceBlendNodes = new FaceBlendData[40];//number of current processable nodes
        for (int i = 0; i < faceBlendNodes.Length; i++)
        {
            faceBlendNodes[i] = new FaceBlendData();
            faceBlendNodes[i].weight = 0;
            faceBlendNodes[i].newWeight = 0;
            faceBlendNodes[i].controlObject = new ControlObject
            {
                M = 2,
                ALPHA = 0.8f,
                KP = 0.04f,
                KD = 1,
                mode = 1
            };
        }
    }
    
    public void SetCharacter(GameObject character)
    {
        this.character = character;
        Initialization();
    }
    
    public void UpdateData(FaceJson faceJson)
    {
        for (int i = 0; i < faceJson.blendShapes.Length; i++)
        {
            faceBlendNodes[i].newWeight = faceJson.blendShapes[i];
        }
        // FaceUpdate();
        UpdateFace();
    }
    
    void UpdateFace()
    {
        
        for (int i = 0; i < faceBlendNodes.Length; i++)
        {
            faceBlendNodes[i].weight = faceBlendNodes[i].newWeight;
        }
        
        // Update facial expression
        BlendShapeController.EyeBlinkLeft.weight = faceBlendNodes[(int) BlendShapes.EyeBlinkLeft].weight;
        BlendShapeController.EyeBlinkRight.weight = faceBlendNodes[(int) BlendShapes.EyeBlinkRight].weight;
        BlendShapeController.EyeSquintLeft.weight = faceBlendNodes[(int) BlendShapes.EyeSquintLeft].weight;
        BlendShapeController.EyeSquintRight.weight = faceBlendNodes[(int) BlendShapes.EyeSquintRight].weight;
        BlendShapeController.EyeWideLeft.weight = faceBlendNodes[(int) BlendShapes.EyeWideLeft].weight;
        BlendShapeController.EyeWideRight.weight = faceBlendNodes[(int) BlendShapes.EyeWideRight].weight;
        BlendShapeController.MouthSmileRight.weight = faceBlendNodes[(int) BlendShapes.MouthSmileRight].weight;
        BlendShapeController.MouthSmileLeft.weight = faceBlendNodes[(int) BlendShapes.MouthSmileLeft].weight;
        BlendShapeController.MouthFrownRight.weight = faceBlendNodes[(int) BlendShapes.MouthFrownRight].weight;
        BlendShapeController.MouthFrownLeft.weight = faceBlendNodes[(int) BlendShapes.MouthFrownLeft].weight;
        BlendShapeController.LipLowerDownLeft.weight = faceBlendNodes[(int) BlendShapes.LipLowerDownLeft].weight;
        BlendShapeController.LipLowerDownRight.weight = faceBlendNodes[(int) BlendShapes.LipLowerDownRight].weight;
        BlendShapeController.LipUpperUpLeft.weight = faceBlendNodes[(int) BlendShapes.LipUpperUpLeft].weight;
        BlendShapeController.LipUpperUpRight.weight = faceBlendNodes[(int) BlendShapes.LipUpperUpRight].weight;
        BlendShapeController.MouthLeft.weight = faceBlendNodes[(int) BlendShapes.MouthLeft].weight;
        BlendShapeController.MouthRight.weight = faceBlendNodes[(int) BlendShapes.MouthRight].weight;
        BlendShapeController.MouthStretchLeft.weight = faceBlendNodes[(int) BlendShapes.MouthStretchLeft].weight;
        BlendShapeController.MouthStretchRight.weight = faceBlendNodes[(int) BlendShapes.MouthStretchRight].weight;
        BlendShapeController.MouthLowerDownRight.weight = faceBlendNodes[(int) BlendShapes.MouthLowerDownRight].weight;
        BlendShapeController.MouthLowerDownLeft.weight = faceBlendNodes[(int) BlendShapes.MouthLowerDownLeft].weight;
        BlendShapeController.MouthPressLeft.weight = faceBlendNodes[(int) BlendShapes.MouthPressLeft].weight;
        BlendShapeController.MouthPressRight.weight = faceBlendNodes[(int) BlendShapes.MouthPressRight].weight;
        BlendShapeController.MouthOpen.weight = faceBlendNodes[(int) BlendShapes.MouthOpen].weight;
        BlendShapeController.MouthPucker.weight = faceBlendNodes[(int) BlendShapes.MouthPucker].weight;
        BlendShapeController.MouthShrugUpper.weight = faceBlendNodes[(int) BlendShapes.MouthShrugUpper].weight;
        BlendShapeController.JawOpen.weight = faceBlendNodes[(int) BlendShapes.JawOpen].weight;
        BlendShapeController.JawLeft.weight = faceBlendNodes[(int) BlendShapes.JawLeft].weight;
        BlendShapeController.JawRight.weight = faceBlendNodes[(int) BlendShapes.JawRight].weight;
        BlendShapeController.BrowDownLeft.weight = faceBlendNodes[(int) BlendShapes.BrowDownLeft].weight;
        BlendShapeController.BrowOuterUpLeft.weight = faceBlendNodes[(int) BlendShapes.BrowOuterUpLeft].weight;
        BlendShapeController.BrowDownRight.weight = faceBlendNodes[(int) BlendShapes.BrowDownRight].weight;
        BlendShapeController.BrowOuterUpRight.weight = faceBlendNodes[(int) BlendShapes.BrowOuterUpRight].weight;
        BlendShapeController.CheekSquintRight.weight = faceBlendNodes[(int) BlendShapes.CheekSquintRight].weight;
        BlendShapeController.CheekSquintLeft.weight = faceBlendNodes[(int) BlendShapes.CheekSquintLeft].weight;
        BlendShapeController.MouthDimpleLeft.weight = faceBlendNodes[(int) BlendShapes.MouthDimpleLeft].weight;
        BlendShapeController.MouthDimpleRight.weight = faceBlendNodes[(int) BlendShapes.MouthDimpleRight].weight;
        BlendShapeController.MouthRollLower.weight = faceBlendNodes[(int) BlendShapes.MouthRollLower].weight;
        BlendShapeController.MouthRollUpper.weight = faceBlendNodes[(int) BlendShapes.MouthRollUpper].weight;
        BlendShapeController.NoseSneerLeft.weight = faceBlendNodes[(int) BlendShapes.NoseSneerLeft].weight;
        BlendShapeController.NoseSneerRight.weight = faceBlendNodes[(int) BlendShapes.NoseSneerRight].weight;
        
        BlendShapeController.UpdateBlendShape();
    }
}


//inspired from OpenVHead repository
public class ControlObject
{
    // member variables
    public float T = 0.1f; // time interval
    public float ALPHA = 0.7f; // incomplete derivative coefficient
    public float KP = 0.04f;
    public float KD = 1;
    public float M = 1; // mass
    public float a = 0; // acceleration
    public float v = 0; // velocity
    public float x = 0; // position
    public float x_d = 0; // desired position
    public float e = 0; // error
    public float e_1 = 0; // last error
    public float de = 0; // derivative of error
    public float p_out = 0; // proportional termd_outd_out_1
    public float d_out = 0; // derivative term
    public float d_out_1 = 0; // last derivative term 
    public float F = 0; // control force

    public float THRESH = 0.05f; // control law changing threshold
    public bool isBlinking = false;
    public int mode;// control mode

    // member methods
    public float control(float X, float X_D)
    {
        x = X;
        x_d = X_D;

        // Incomplete derivative PD control
        // Control Law ==================================================
        e = x_d - x; // Update error
        de = (e - e_1)/T; // Compute the derivative of error
        p_out = KP*e;
        d_out = (1-ALPHA)*KD*de + ALPHA*d_out_1;
        
        switch(mode)
        {
            case 1:
                F = p_out + d_out; // Update control force
                break;

            case 2: // Many bugs !!!!!!!!
                if(X_D < THRESH && x > THRESH)
                {
                    isBlinking = true;
                    F = -1;
                }
                else if(isBlinking == true)
                {
                    if(x>0.001f)
                    {
                        isBlinking = true;
                        F = -1;
                    }
                    else
                    {
                        isBlinking = false;
                        F = p_out + d_out;
                    }
                }
                else
                {
                    F = p_out + d_out; // Update control force
                }
                break;
            default:
                break;

        }
        

        e_1 = e;// Update last error
        d_out_1 = d_out; // Update last derivative term

        // System Law ==================================================
        a = F/M; // Update acceleration
        v = v + a*T; // Update velocity
        x = x + v*T; // Update position
        if(x<0)
        {
            x = 0;
        }
        
        return x;
    }
}

class KalmanObject
{
    public float K;
    public float X = 0;
    public float P = 0.1f;

    public float kalman_filter(float input,float Q,float R)
    {
        K = P / (P + R);
        X = X + K * (input - X);
        P = P - K * P + Q;
        return X;
    }
    
}
