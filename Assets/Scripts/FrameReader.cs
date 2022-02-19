using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public BodyPart[] predictions_world;
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
    public BodyPartVector[] predictions_world;
}

public class FrameReader : MonoBehaviour
{
    public TextAsset jsonTest;

    public float fraction = 1.2f;


    public PoseJson poseJson;

    private void Awake()
    {
        poseJson = GetBodyParts(jsonTest.text);
        Debug.Log(poseJson.predictions_world[1].x);
    }

    public PoseJson GetBodyParts(string text)
    {
        
        return JsonUtility.FromJson<PoseJson>(text);
    }
    public PoseJsonVector GetBodyPartsVector(string text)
    {
        PoseJson poseJson = JsonUtility.FromJson<PoseJson>(text);
        int len = poseJson.predictions_world.Length;
        PoseJsonVector poseJsonVector = new PoseJsonVector();
        poseJsonVector.predictions = new BodyPartVector[len];
        poseJsonVector.predictions_world = new BodyPartVector[len];
        
        for (int i = 0; i < len; i++)
        {
            poseJsonVector.predictions[i].position = new Vector3(poseJson.predictions[i].x,
                poseJson.predictions[i].y,poseJson.predictions[i].z);
            poseJsonVector.predictions[i].visibility = poseJson.predictions[i].visibility;
            
            poseJsonVector.predictions_world[i].position = fraction * new Vector3(-poseJson.predictions_world[i].x,
                -poseJson.predictions_world[i].y,poseJson.predictions_world[i].z);
            poseJsonVector.predictions_world[i].visibility = poseJson.predictions_world[i].visibility;
        }

        return poseJsonVector;
    }
}
