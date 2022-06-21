using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

[Serializable]
public class FrameData
{
    public PoseJsonVector poseData;
    public FaceJson faceData;
    public HandJsonVector handData;
    public int frame;
}

public class FrameReader : MonoBehaviour
{
    [Header("Requirements")]
    public Pose3DMapper pose3DMapper;
    public HandsPreprocessor handPose;
    public FacialExpressionHandler facialExpressionHandler;
    public VideoPlayer videoPlayer;
    private List<FrameData> frameData;
    private FrameData currentFrameData;
    
    
    [Header("Fractions to multiply by pose estimates")]
    public float fraction = 1.2f;
    public float fractionX = 1.2f;
    public float fractionY = 1.2f;
    public float fractionZ = 1.2f;

    [Header("Frame rate")]
    [SerializeField] private float nextFrameTime = 0.1f;

    private int currentAnimationSlot = 0;
    
    
    //Body pose
    private List<PoseJsonVector> estimatedPoses;
    [HideInInspector] public PoseJson currentPoseJson;
    [HideInInspector] public PoseJsonVector currentPoseJsonVector;
    [HideInInspector] public PoseJsonVector currentPoseJsonVectorNew;
    [HideInInspector] public int poseIndex;

    
    //Hand pose
    private List<HandJsonVector> estimatedHandPose;
    [HideInInspector] public HandJsonVector currentHandJsonVector;
    [HideInInspector] public HandJsonVector currentHandJsonVectorNew;
    [HideInInspector] public int handIndex;
    
    //facial mocap
    private List<FaceJson> estimatedFacialMocap;
    [HideInInspector] public FaceJson currentFaceJson;
    [HideInInspector] public FaceJson currentFaceJsonNew;
    [HideInInspector] public int faceIndex;


    [Header("3D Character")] 
    [SerializeField] private GameObject character;

    [Header("PlayController")] 
    public bool pause = true;
    
    
    
    [Header("Debug")] 
    [SerializeField] private bool debug;

    [SerializeField] private bool readFromFileHand;
    [SerializeField] private TextAsset jsonTestHand;
    
    [SerializeField] private bool readFromFace;
    [SerializeField] private TextAsset jsonTestFace;
    
    [SerializeField] private bool readFromFilePose;
    [SerializeField] private TextAsset jsonTestPose;

    [SerializeField] private bool enableFileSeriesReader;
    [SerializeField] private string path = "C:\\Danial\\Projects\\Danial\\DigiHuman\\Backend\\hand_json\\";
    [SerializeField] private bool onlyCurrentIndex;
    
    private void Start()
    {
        estimatedPoses = new List<PoseJsonVector>();
        estimatedFacialMocap = new List<FaceJson>();
        estimatedHandPose = new List<HandJsonVector>();
        frameData = new List<FrameData>();
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
            if (readFromFace)
                jsonTest = jsonTestFace.text;
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

        if (readFromFace)
        {
            FaceJson faceJson = GetBodyParts<FaceJson>(jsonTest);
            facialExpressionHandler.UpdateData(faceJson.leftEyeWid,faceJson.rightEyeWid
                ,faceJson.mouthWid,faceJson.mouthLen);
            videoPlayer.frame = faceJson.frame;
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
        
        if(pause || currentAnimationSlot >= frameData.Count)
            return;
        if (timer > nextFrameTime)
        {

            // //body pose
            // if (poseIndex < estimatedPoses.Count)
            // {
            //     currentPoseJsonVector = currentPoseJsonVectorNew;
            //     currentPoseJsonVectorNew = estimatedPoses[poseIndex];
            // }
            //
            // //Hand
            // if (handIndex < estimatedHandPose.Count)
            // {
            //     currentHandJsonVector = currentHandJsonVectorNew;
            //     currentHandJsonVectorNew = estimatedHandPose[handIndex];
            // }
            //
            // //Face
            // if (faceIndex < estimatedFacialMocap.Count)
            // {
            //     currentFaceJson = currentFaceJsonNew;
            //     currentFaceJsonNew = estimatedFacialMocap[faceIndex];
            // }

            //Current Frame data
            
            currentFrameData = frameData[currentAnimationSlot];
            //Body
            currentPoseJsonVector = currentPoseJsonVectorNew;
            currentPoseJsonVectorNew = currentFrameData.poseData;
            //Hand
            currentHandJsonVector = currentHandJsonVectorNew;
            currentHandJsonVectorNew = currentFrameData.handData;
            //Face
            currentFaceJson = currentFaceJsonNew;
            currentFaceJsonNew = currentFrameData.faceData;
            
            if (debug)
            {
                videoPlayer.frame = frameData[currentAnimationSlot].frame;
                videoPlayer.Play();
                videoPlayer.Pause();
            }
            timer = 0;
            currentAnimationSlot++;
        }

        try
        {
            //-------- Body Pose ------
            if (currentPoseJsonVector != null)
            {
                //TODO change maybe looking for 5 frames later!
                if (currentPoseJsonVectorNew != null)
                {
                    //for each bone position in the current frame
                    for (int i = 0; i < currentPoseJsonVector.predictions.Length; i++)
                    {
                        currentPoseJsonVector.predictions[i].position = Vector3.Lerp(
                            currentPoseJsonVector.predictions[i].position,
                            currentPoseJsonVectorNew.predictions[i].position,
                            timer / nextFrameTime);
                    }
                }

                pose3DMapper.Predict3DPose(currentPoseJsonVector);
            }

            //----- Hands -----
            //TODO lerp hand data
            if (currentHandJsonVector != null)
            {
                handPose.Predict3DPose(currentHandJsonVector);
            }

            //----- Facial Mocap -------
            if (currentFaceJson != null)
            {
                facialExpressionHandler.UpdateData(currentFaceJson.leftEyeWid, currentFaceJson.rightEyeWid
                    , currentFaceJson.mouthWid, currentFaceJson.mouthLen);
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
            Debug.LogError("Problem occured: " + e.Message);
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
            handJsonVector.handsR[i].position = new Vector3(data.x * fractionX,-data.y * fractionY,-data.z * fractionZ);
            handJsonVector.handsR[i].visibility = data.visibility;
        }

        for (int i = 0; i < len2; i++)
        {
            BodyPart data = handJson.handsL[i];
            handJsonVector.handsL[i].position = new Vector3(data.x * fractionX,-data.y * fractionY,-data.z * fractionZ);
            handJsonVector.handsL[i].visibility = data.visibility;
        }
        return handJsonVector;
    }
    
    
    public void SetHandPoseList(List<HandJson> estimated)
    {
        framesLoaded = false;
        currentHandJsonVector = GetHandsVector(estimated[0]);
        foreach (HandJson poseJson in estimated)
        {
            estimatedHandPose.Add(GetHandsVector(poseJson));            
        }
        Debug.Log(estimatedHandPose.Count);
        Debug.Log(estimatedHandPose[estimatedHandPose.Count/2].handsL.Length);
        Debug.Log(estimatedHandPose[estimatedHandPose.Count/2].handsR.Length);
    }
    

    public void SetPoseList(List<PoseJson> estimated)
    {
        framesLoaded = false;
        currentPoseJsonVectorNew = GetBodyPartsVector(estimated[0]);
        foreach (PoseJson poseJson in estimated)
        {
            estimatedPoses.Add(GetBodyPartsVector(poseJson));            
        }
        Debug.Log(estimated.Count);
    }
    
    public void SetFaceMocapList(List<FaceJson> estimated)
    {
        framesLoaded = false;
        print(estimated.Count);
        currentFaceJsonNew = estimated[0];
        estimatedFacialMocap = estimated;
    }

    private bool framesLoaded = false;
    public void LoadFrames(FrameData[] frameData)
    {
        framesLoaded = true;
        this.frameData = frameData.ToList<FrameData>();
    }
    public void ArrangeDataFrames()
    {
        if(framesLoaded)
            return;
        int handFrame = 0;
        int faceFrame = 0;
        int bodyFrame = 0;
        int minFrame = 0;

        int bodyIndex = 0;
        int faceIndex = 0;
        int handIndex = 0;

        int index = 0;
        while (true)
        {
            if (bodyIndex < estimatedPoses.Count)
            {
                bodyFrame = estimatedPoses[bodyIndex].frame;
            }
            else
            {
                bodyFrame = int.MaxValue; //no more frames!
            }
            
            if (handIndex < estimatedHandPose.Count)
            {
                handFrame = estimatedHandPose[handIndex].frame;
            }
            else
            {
                handFrame = int.MaxValue; //no more frames!
            }
            
            if (faceIndex < estimatedFacialMocap.Count)
            {
                faceFrame = estimatedFacialMocap[handIndex].frame;
            }
            else
            {
                faceFrame = int.MaxValue; //no more frames!
            }

            minFrame = Mathf.Min(bodyFrame, handFrame, faceFrame);
            
            if(minFrame == Int32.MaxValue)
                break;
            
            FrameData currentFrameData = new FrameData();
            if (bodyFrame == minFrame)
            {
                currentFrameData.poseData = estimatedPoses[bodyIndex];
                bodyIndex++;
            }
            if (handFrame == minFrame)
            {
                currentFrameData.handData = estimatedHandPose[handIndex];
                handIndex++;
            }

            if (faceIndex == minFrame)
            {
                currentFrameData.faceData = estimatedFacialMocap[faceIndex];
                faceIndex++;
            }


            currentFrameData.frame = minFrame;
            frameData.Add(currentFrameData);
            index++;
        }

        currentPoseJsonVectorNew = frameData[0].poseData;
        currentHandJsonVectorNew = frameData[0].handData;
        currentFaceJsonNew = frameData[0].faceData;
        currentAnimationSlot = frameData[0].frame;
    }

    public FrameData[] GetFrameData()
    {
        
        Debug.Log(frameData.ToArray().Length);
        return frameData.ToArray();
    }

    public void SetNewCharacter(GameObject newCharacter)
    {
        character.SetActive(false);
        character = newCharacter;
        pose3DMapper.SetCharacter(character);
        handPose.SetCharacter(character);
        facialExpressionHandler.SetCharacter(newCharacter);

    }
}
