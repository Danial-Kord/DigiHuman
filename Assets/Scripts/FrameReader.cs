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
    public CharacterMapper characterMapper;
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
    [SerializeField] private TextAsset jsonTest;

    private void Awake()
    {
        estimatedPoses = new Queue<PoseJsonVector>();
        if (debug)
        {
            videoPlayer.Prepare();

            //currentPoseJson = GetBodyParts(jsonTest.text);
            //currentPoseJsonVector = GetBodyPartsVector(currentPoseJson);
        }
        
    }

    private float timer = 0;
    private void Update()
    {
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
            characterMapper.Predict3DPose(currentPoseJsonVector);
        }
        catch (Exception e)
        {

        }
        
    }

    
    
    private PoseJson GetBodyParts(string jsonText)
    {
        return JsonUtility.FromJson<PoseJson>(jsonText);
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

    public void SetPosesQueue(List<PoseJson> estimated)
    {
        currentPoseJsonVectorNew = GetBodyPartsVector(estimated[0]);
        foreach (PoseJson poseJson in estimated)
        {
            estimatedPoses.Enqueue(GetBodyPartsVector(poseJson));            
        }
        
    }
}
