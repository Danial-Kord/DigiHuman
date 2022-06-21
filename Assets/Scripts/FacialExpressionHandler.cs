using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacialExpressionHandler : MonoBehaviour
{
    // Control objects
    ControlObject LeftEyeControl = new ControlObject();
    ControlObject RightEyeControl = new ControlObject();
    ControlObject MouthWidControl = new ControlObject();
    ControlObject MouthLenControl = new ControlObject();

    

    public Vector3 getleftEyeShape;
    public Vector3 getrightEyeShape;
    public Vector3 getmouthShape;

    [SerializeField] private GameObject character;
    private BlendShapeController characterBlendShapeController;
    // Start is called before the first frame update
    void Start()
    {

        Initialization();

    }

    private void Initialization()
    {
        characterBlendShapeController = character.GetComponentInChildren<BlendShapeController>();
        // Initialize head parameters

        getleftEyeShape = new Vector3(0.2f, 0.2f, 0.2f);
        getrightEyeShape = new Vector3(0.2f, 0.2f, 0.2f);
        getmouthShape = new Vector3(0.45f, 0.03f, 0.2f);

        // Initialize control objects
        RightEyeControl.M = 2;
        RightEyeControl.ALPHA = 0.8f;
        RightEyeControl.KP = 0.04f;
        RightEyeControl.KD = 1;   

        LeftEyeControl.M = 2;
        LeftEyeControl.ALPHA = 0.8f;
        LeftEyeControl.KP = 0.04f;
        LeftEyeControl.KD = 1;

        // Select Control Mode
        RightEyeControl.mode = 1;
        LeftEyeControl.mode = 1;
        MouthLenControl.mode = 1;
        MouthWidControl.mode = 1;
    }
    
    public void SetCharacter(GameObject character)
    {
        this.character = character;
        Initialization();
    }
    
    public void UpdateData(float leftEyeWid, float rightEyeWid, float mouthWid, float mouthLen)
    {
        
        getleftEyeShape = new Vector3(0.2f, leftEyeWid, 0.2f);
        getrightEyeShape = new Vector3(0.2f, rightEyeWid, 0.2f);
        getmouthShape = new Vector3(mouthLen, mouthWid, 0.2f);
        FaceUpdate();
    }
    
    void FaceUpdate()
    {
        
        // Facial expression control!
        // 1. Right Eye
        if(getrightEyeShape[1]<0.05f)// Right eye closed 
        {
            getrightEyeShape[1] = 0.01f;
        }
        else// Right eye opened
        {
            getrightEyeShape[1] = RightEyeControl.control(BlendShapeController.rightEyeShape[1], getrightEyeShape[1]);
        }
        // 2. Left Eye
        if(getleftEyeShape[1]<0.05f)// Left eye closed
        {
            getleftEyeShape[1] = 0.01f;
        }
        else// Left eye opened
        {
            getleftEyeShape[1] = LeftEyeControl.control(BlendShapeController.leftEyeShape[1], getleftEyeShape[1]);
        }
        // 3. Mouth Length
        getmouthShape[0] = MouthLenControl.control(BlendShapeController.mouthShape[0], getmouthShape[0]);
        // 4. Mouth Width
        getmouthShape[1] = MouthWidControl.control(BlendShapeController.mouthShape[1], getmouthShape[1]);
        

        // Update global variables 

        // 2. Update facial expression shapes  
        BlendShapeController.leftEyeShape = getleftEyeShape;
        BlendShapeController.rightEyeShape = getrightEyeShape;
        BlendShapeController.mouthShape = getmouthShape;

    }
}

   


class ControlObject
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
