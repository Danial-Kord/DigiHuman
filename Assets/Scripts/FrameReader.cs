using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[Serializable] 
public struct BodyPart
{
    public float x;
    public float y;
    public float z;
    public float visibility;
}


[Serializable]
public class PoseJson
{
    public BodyPart[] predictions;
    public float width;
    public float height;
    public int frame;
    
}



[Serializable]
public class HandJson
{
    public BodyPart[] handsR;
    public BodyPart[] handsL;
    public int frame;
}

[Serializable]
public class HandJsonVector
{
    public BodyPartVector[] handsR;
    public BodyPartVector[] handsL;
    public int frame;
}

[Serializable] 
public struct BodyPartVector
{
    public Vector3 position;
    public float visibility;
}

[Serializable]
public class PoseJsonVector
{
    public BodyPartVector[] predictions;
    public float width;
    public float height;
    public int frame;

}

public class FrameReader : MonoBehaviour
{
    public Pose3DMapper pose3DMapper;
    public HandsPreprocessor handPose;
    public VideoPlayer videoPlayer;

    [Header("Fractions to multiply by pose estimates")]
    public float fraction = 1.2f;
    public float fractionX = 1.2f;
    public float fractionY = 1.2f;
    public float fractionZ = 1.2f;

    [Header("Frame rate")]
    [SerializeField]private float nextFrameTime = 0.1f;

    private Queue<PoseJsonVector> estimatedPoses;
    [HideInInspector] public PoseJson currentPoseJson;
    [HideInInspector] public PoseJsonVector currentPoseJsonVector;
    [HideInInspector] public PoseJsonVector currentPoseJsonVectorNew;

    [Header("Debug")] 
    [SerializeField] private bool debug;

    [SerializeField] private bool readFromFileHand;
    [SerializeField] private TextAsset jsonTestHand;
    
    [SerializeField] private bool readFromFilePose;
    [SerializeField] private TextAsset jsonTestPose;


    private void Start()
    {
        estimatedPoses = new Queue<PoseJsonVector>();
        if (debug)
        {
            videoPlayer.Prepare();
        }
    }

    private float timer = 0;
    private void LateUpdate()
    {
        if (debug)
        {
            if (readFromFilePose)
            {
                currentPoseJson = GetBodyParts<PoseJson>(jsonTestPose.text);
                currentPoseJsonVector = GetBodyPartsVector(currentPoseJson);
                pose3DMapper.Predict3DPose(currentPoseJsonVector);
            }
            if (readFromFileHand)
            {
                HandJson handJson = GetBodyParts<HandJson>(jsonTestHand.text);
                HandJsonVector handsVector = GetHandsVector(handJson);
                handPose.Predict3DPose(handsVector);
            }
        }
        timer += Time.deltaTime;
        if(estimatedPoses.Count.Equals(0))
            return;
        if (timer > nextFrameTime)
        {

            currentPoseJsonVector = currentPoseJsonVectorNew;
            currentPoseJsonVectorNew = estimatedPoses.Dequeue();
            timer = 0;
            if (debug)
            {
                videoPlayer.frame = currentPoseJsonVectorNew.frame;
                videoPlayer.Play();
                videoPlayer.Pause();
            }
            
        }

        try
        {
            for (int i = 0; i < currentPoseJsonVector.predictions.Length; i++)
            {
                currentPoseJsonVector.predictions[i].position = Vector3.Lerp(
                    currentPoseJsonVector.predictions[i].position, currentPoseJsonVectorNew.predictions[i].position,
                    timer / nextFrameTime);
            }
            pose3DMapper.Predict3DPose(currentPoseJsonVector);
        }
        catch (Exception e)
        {

        }
        
    }

    private void Update()
    {
        if (debug)
        {
            if (readFromFilePose)
            {
                currentPoseJson = GetBodyParts<PoseJson>(jsonTestPose.text);
                currentPoseJsonVector = GetBodyPartsVector(currentPoseJson);
                pose3DMapper.Predict3DPose(currentPoseJsonVector);
            }
            if (readFromFileHand)
            {
                HandJson handJson = GetBodyParts<HandJson>(jsonTestHand.text);
                HandJsonVector handsVector = GetHandsVector(handJson);
                handPose.Predict3DPose(handsVector);
            }
        }
        timer += Time.deltaTime;
        if(estimatedPoses.Count.Equals(0))
            return;
        if (timer > nextFrameTime)
        {

            currentPoseJsonVector = currentPoseJsonVectorNew;
            currentPoseJsonVectorNew = estimatedPoses.Dequeue();
            timer = 0;
            if (debug)
            {
                videoPlayer.frame = currentPoseJsonVectorNew.frame;
                videoPlayer.Play();
                videoPlayer.Pause();
            }
            
        }

        try
        {
            for (int i = 0; i < currentPoseJsonVector.predictions.Length; i++)
            {
                currentPoseJsonVector.predictions[i].position = Vector3.Lerp(
                    currentPoseJsonVector.predictions[i].position, currentPoseJsonVectorNew.predictions[i].position,
                    timer / nextFrameTime);
            }
            pose3DMapper.Predict3DPose(currentPoseJsonVector);
        }
        catch (Exception e)
        {

        }

    }


    private T GetBodyParts<T>(string jsonText)
    {
        return JsonUtility.FromJson<T>(jsonText);
    }
    private PoseJsonVector GetBodyPartsVector(PoseJson poseJson)
    {
        int len = poseJson.predictions.Length;
        PoseJsonVector poseJsonVector = new PoseJsonVector();
        poseJsonVector.predictions = new BodyPartVector[len];
        poseJsonVector.frame = poseJson.frame;
        poseJsonVector.width = poseJson.width;
        poseJsonVector.height = poseJson.height;
        for (int i = 0; i < len; i++)
        {
            poseJsonVector.predictions[i].position = fraction * new Vector3(-poseJson.predictions[i].x*fractionX,
                -poseJson.predictions[i].y*fractionY,poseJson.predictions[i].z*fractionZ);
            poseJsonVector.predictions[i].visibility = poseJson.predictions[i].visibility;
            
        }

        return poseJsonVector;
    }
    
    
    private HandJsonVector GetHandsVector(HandJson handJson)
    {
        int len = handJson.handsR.Length;
        int len2 = handJson.handsL.Length;
        HandJsonVector handJsonVector = new HandJsonVector();
        handJsonVector.handsR = new BodyPartVector[len];
        handJsonVector.handsL = new BodyPartVector[len2];
        handJsonVector.frame = handJson.frame;
        for (int i = 0; i < len; i++)
        {
            BodyPart data = handJson.handsR[i];
            handJsonVector.handsR[i].position = new Vector3(-data.x,-data.y,-data.z);
            handJsonVector.handsR[i].visibility = data.visibility;
        }

        for (int i = 0; i < len2; i++)
        {
            BodyPart data = handJson.handsL[i];
            handJsonVector.handsL[i].position = new Vector3(data.x,data.y,data.z);
            handJsonVector.handsL[i].visibility = data.visibility;
        }
        return handJsonVector;
    }
    

    public void SetPosesQueue(List<PoseJson> estimated)
    {
        currentPoseJsonVectorNew = GetBodyPartsVector(estimated[0]);
        foreach (PoseJson poseJson in estimated)
        {
            estimatedPoses.Enqueue(GetBodyPartsVector(poseJson));            
        }
        
    }
}
