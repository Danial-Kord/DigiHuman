using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandAnomalyDetector : MonoBehaviour
{

    [SerializeField] 
    [Tooltip("how much rotation difference should be considered as negative data in 1 frame difference")] 
    private float rotationOffset;

    [SerializeField] 
    [Tooltip("maximum wrong consecutive frames")] 
    private int maxConsecutiveWrongFrames;

    [SerializeField] 
    [Tooltip("maximum consecutive null frames")] 
    private int maxConsecutiveNullFrames;
    
    
    public void WrongLandmarkDetector(FrameData[] frameDatas)
    {
        Vector3 indexFingerFirst = Vector3.zero;
        Vector3 wrist = Vector3.zero;
        Vector3 pinkyFirstLandmark = Vector3.zero;
        

        Vector3 rightHandDirectionBase = Vector3.zero;
        int rightHandCounter = 0;
        int consecutiveNullFramesR = 0;
        int consecutiveNullFramesL = 0;
        
        Vector3 leftHandDirectionBase = Vector3.zero;
        int leftHandCounter = 0;
        
        
        Vector3 currentDir = Vector3.zero;

        bool lastDataWasWrongR = false;
        bool lastDataWasWrongL = false;
        
        for (int i = 0; i < frameDatas.Length; i++)
        {
            //Detect right hand anomalies
            BodyPartVector[] currentR = frameDatas[i].handData.handsR;
            if (currentR != null)
            {
                if (currentR.Length != 0)
                {
                    wrist = currentR[(int) HandPoints.Wrist].position;
                    indexFingerFirst = currentR[(int) HandPoints.IndexFingerFirst].position;
                    pinkyFirstLandmark = currentR[(int) HandPoints.PinkyFirst].position;
                    currentDir = wrist.TriangleNormal(indexFingerFirst,pinkyFirstLandmark);
                    if (rightHandDirectionBase.Equals(Vector3.zero) || rightHandCounter>maxConsecutiveWrongFrames)
                    {
                        rightHandDirectionBase = currentDir;
                        rightHandCounter = 0;
                    }
                    else if (Vector3.Angle(currentDir, rightHandDirectionBase) > rotationOffset ||
                            Vector3.Angle(currentDir, rightHandDirectionBase) < -rotationOffset)
                    {
                        //this frame hand data is not valid!
                        frameDatas[i].handData.handsR = new BodyPartVector[0];
                        Debug.Log($"Frame data for right hand deleted, frame:{frameDatas[i].frame}");
                    }
                    else
                    {
                        rightHandDirectionBase = currentDir;
                        rightHandCounter = 0;
                    }
                    rightHandCounter++;
                }
                else
                {
                    rightHandCounter+=maxConsecutiveWrongFrames/maxConsecutiveNullFrames;
                }
            }
            else
            {
                rightHandCounter+=maxConsecutiveWrongFrames/maxConsecutiveNullFrames;
            }
            
            
            //Detect left hand anomalies
            BodyPartVector[] currentL = frameDatas[i].handData.handsL;
            if (currentL != null)
            {
                if (currentL.Length != 0)
                {
                    wrist = currentL[(int) HandPoints.Wrist].position;
                    indexFingerFirst = currentL[(int) HandPoints.IndexFingerFirst].position;
                    pinkyFirstLandmark = currentL[(int) HandPoints.PinkyFirst].position;
                    currentDir = wrist.TriangleNormal(indexFingerFirst,pinkyFirstLandmark);
                    if (leftHandDirectionBase.Equals(Vector3.zero) || leftHandCounter>maxConsecutiveWrongFrames)
                    {
                        leftHandDirectionBase = currentDir;
                        leftHandCounter = 0;
                    }
                    else if (Vector3.Angle(currentDir, leftHandDirectionBase) > rotationOffset ||
                             Vector3.Angle(currentDir, leftHandDirectionBase) < -rotationOffset)
                    {
                        //this frame hand data is not valid!
                        frameDatas[i].handData.handsL = new BodyPartVector[0];
                        Debug.Log($"Frame data for left hand deleted, frame:{frameDatas[i].frame}");
                    }
                    else
                    {
                        leftHandDirectionBase = currentDir;
                        leftHandCounter = 0;
                    }
                    leftHandCounter++;
                }
                else
                {
                    leftHandCounter+=maxConsecutiveWrongFrames/maxConsecutiveNullFrames;

                }
            }
            else
            {
                leftHandCounter+=maxConsecutiveWrongFrames/maxConsecutiveNullFrames;

            }

            
        }
    }
        


    
    
}
