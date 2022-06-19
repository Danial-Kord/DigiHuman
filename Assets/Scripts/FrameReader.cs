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
public class FaceJson
{
    public float leftEyeWid;
    public float rightEyeWid;
    public float mouthWid;
    public float mouthLen;
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

public class FrameData
{
    public PoseJsonVector poseData;
    public FaceJson faceData;
    public HandJson handData;
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

    //Body pose
    private List<PoseJsonVector> estimatedPoses;
    [HideInInspector] public PoseJson currentPoseJson;
    [HideInInspector] public PoseJsonVector currentPoseJsonVector;
    [HideInInspector] public PoseJsonVector currentPoseJsonVectorNew;
    [HideInInspector] public int poseIndex;

    //facial mocap
    private List<FaceJson> estimatedFacialMocap;
    [HideInInspector] public FaceJson currentFaceJson;
    [HideInInspector] public FaceJson currentFaceJsonNew;
    [HideInInspector] public int faceIndex;
    
    
    
    [Header("Debug")] 
    [SerializeField] private bool debug;

    [SerializeField] private bool readFromFileHand;
    [SerializeField] private TextAsset jsonTestHand;
    
    [SerializeField] private bool readFromFilePose;
    [SerializeField] private TextAsset jsonTestPose;

    [SerializeField] private bool enableFileSeriesReader;
    [SerializeField] private string path = "C:\\Danial\\Projects\\Danial\\DigiHuman\\Backend\\hand_json\\";
    [SerializeField] private bool onlyCurrentIndex;
    
    private void Start()
    {
        estimatedPoses = new List<PoseJsonVector>();
        if (debug)
        {
            videoPlayer.Prepare();
        }
    }

    private float timer = 0;
    private string jsonTest;
    [SerializeField] private int fileIndex = 1;

    private void TestFromFile()
    {
        if (enableFileSeriesReader)
        {
            StreamReader reader = new StreamReader(path + "" + fileIndex + ".json");
            jsonTest = reader.ReadToEnd();
        }
        else
        {
            if(readFromFilePose)
                jsonTest = jsonTestPose.text;
            if(readFromFileHand)
                jsonTest = jsonTestHand.text;
        }
        if (readFromFilePose)
        {
            currentPoseJson = GetBodyParts<PoseJson>(jsonTest);
            currentPoseJsonVector = GetBodyPartsVector(currentPoseJson);
            pose3DMapper.Predict3DPose(currentPoseJsonVector);
            videoPlayer.frame = currentPoseJson.frame;
            videoPlayer.Play();
            videoPlayer.Pause();
        }
        if (readFromFileHand)
        {
            HandJson handJson = GetBodyParts<HandJson>(jsonTest);
            HandJsonVector handsVector = GetHandsVector(handJson);
            handPose.Predict3DPose(handsVector);
            videoPlayer.frame = handJson.frame;
            videoPlayer.Play();
            videoPlayer.Pause();
        }
    }
    private void LateUpdate()
    {
        timer += Time.deltaTime;
        if (debug)
        {
            
            // videoPlayer.frame = fileIndex-1;
            // videoPlayer.Play();
            // videoPlayer.Pause();
            if (timer > nextFrameTime)
            {
                timer = 0;
                if(!onlyCurrentIndex)
                    fileIndex += 1;
            }
            try
            {
                TestFromFile();
            }
            catch (Exception e)
            {
                print("File problem or empty array!" + "\n" + e.StackTrace);
                throw;
                Console.Write(e);
            }
            
            

            return;
        }
        

        if (timer > nextFrameTime)
        {

            //body pose
            if (!estimatedPoses.Count.Equals(0))
            {
                currentPoseJsonVector = currentPoseJsonVectorNew;
                currentPoseJsonVectorNew = estimatedPoses[poseIndex];
            }

            //Face
            if (!estimatedFacialMocap.Count.Equals(0))
            {
                currentFaceJson = currentFaceJsonNew;
                currentFaceJsonNew = estimatedFacialMocap[faceIndex];
            }

            if (debug)
            {
                videoPlayer.frame = currentPoseJsonVectorNew.frame -1;
                videoPlayer.Play();
                videoPlayer.Pause();
            }
            timer = 0;
            
            //TODO Sync
            poseIndex++;
            faceIndex++;
        }

        try
        {
            //-------- Body Pose ------
            if (!estimatedPoses.Count.Equals(0))
            {
                //for each bone position in the current frame
                for (int i = 0; i < currentPoseJsonVector.predictions.Length; i++)
                {
                    currentPoseJsonVector.predictions[i].position = Vector3.Lerp(
                        currentPoseJsonVector.predictions[i].position, currentPoseJsonVectorNew.predictions[i].position,
                        timer / nextFrameTime);
                }

                pose3DMapper.Predict3DPose(currentPoseJsonVector);
            }

            //----- Hands -----
            //TODO lerp hand data
            
            //----- Facial Mocap -------
            
            
        }
        catch (Exception e)
        {
            Debug.LogError("Problem with pose estimation: " + e.Message);
        }
        
    }

    // private void Update()
    // {
    //     timer += Time.deltaTime;
    //     if (debug)
    //     {
    //         if (readFromFilePose)
    //         {
    //             currentPoseJson = GetBodyParts<PoseJson>(jsonTestPose.text);
    //             currentPoseJsonVector = GetBodyPartsVector(currentPoseJson);
    //             pose3DMapper.Predict3DPose(currentPoseJsonVector);
    //         }
    //         if (readFromFileHand)
    //         {
    //             HandJson handJson = GetBodyParts<HandJson>(jsonTestHand.text);
    //             HandJsonVector handsVector = GetHandsVector(handJson);
    //             handPose.Predict3DPose(handsVector);
    //         }
    //         return;
    //     }
    //     if(estimatedPoses.Count.Equals(0))
    //         return;
    //     if (timer > nextFrameTime)
    //     {
    //
    //         currentPoseJsonVector = currentPoseJsonVectorNew;
    //         currentPoseJsonVectorNew = estimatedPoses.Dequeue();
    //         timer = 0;
    //         if (debug)
    //         {
    //             videoPlayer.frame = currentPoseJsonVectorNew.frame;
    //             videoPlayer.Play();
    //             videoPlayer.Pause();
    //         }
    //         
    //     }
    //
    //     try
    //     {
    //         for (int i = 0; i < currentPoseJsonVector.predictions.Length; i++)
    //         {
    //             currentPoseJsonVector.predictions[i].position = Vector3.Lerp(
    //                 currentPoseJsonVector.predictions[i].position, currentPoseJsonVectorNew.predictions[i].position,
    //                 timer / nextFrameTime);
    //         }
    //         pose3DMapper.Predict3DPose(currentPoseJsonVector);
    //     }
    //     catch (Exception e)
    //     {
    //
    //     }
    //
    // }
    //

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
            handJsonVector.handsR[i].position = new Vector3(-data.x * fractionX,-data.y * fractionY,-data.z * fractionZ);
            handJsonVector.handsR[i].visibility = data.visibility;
        }

        for (int i = 0; i < len2; i++)
        {
            BodyPart data = handJson.handsL[i];
            handJsonVector.handsL[i].position = new Vector3(data.x * fractionX,data.y * fractionY,data.z * fractionZ);
            handJsonVector.handsL[i].visibility = data.visibility;
        }
        return handJsonVector;
    }
    

    public void SetPoseList(List<PoseJson> estimated)
    {
        currentPoseJsonVectorNew = GetBodyPartsVector(estimated[0]);
        foreach (PoseJson poseJson in estimated)
        {
            estimatedPoses.Add(GetBodyPartsVector(poseJson));            
        }
    }
    
    public void SetFaceMocapList(List<FaceJson> estimated)
    {
        currentFaceJsonNew = estimated[0];
        estimatedFacialMocap = estimated;
    }

    public void ArrangeDataFrames()
    {
        
    }
}
