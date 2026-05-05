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
public class FullPoseJson
{
    public PoseJson bodyPose;
    public HandJson handsPose;
    public int frame;
}


[Serializable]
public class FaceJson
{
    public float[] blendShapes;
    public int frame;
    public float time;
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
    /// <summary>Playback time in seconds (from video start). Prefer FaceJson.time when present; used to match animation rate to source video.</summary>
    public float timeSeconds;
}

/// <summary>WebSocket /live_mocap error payload from Python server.</summary>
[Serializable]
public class LiveMocapWsError
{
    public string error;
}

/// <summary>One frame from ws://.../ws/live_mocap (matches Backend realtime_mocap JSON).</summary>
[Serializable]
public class LiveMocapFrameDto
{
    public int frame;
    public PoseJson bodyPose;
    public HandJson handsPose;
    public FaceJson faceData;
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
    [SerializeField] private bool enableVideoAspectRatioEffector;
    private float videoFractionX = 1;
    private float videoFractionY = 1;
    private float videoFractionZ = 1;
    
    [Header("Frame rate")]
    [SerializeField] private float nextFrameTime;

    [Tooltip("Used when JSON has no FaceJson.time (ms); also fallback span between pose/hand-only keys.")]
    [SerializeField] private float fallbackTimelineFps = 30f;

    /// <summary>Wall-clock playback position when not driving from VideoPlayer.time.</summary>
    private float playbackElapsedSeconds;

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
    [SerializeField] private bool enableFace;
    [SerializeField] private bool enableHands=true;

    [Header("PlayController")] 
    public bool pause = true;

    [SerializeField] private Slider slider;
    
    [SerializeField] private bool enableVideo;

    /// <summary>When true, FixedUpdate timeline playback is skipped; use ApplyLiveMocap* from WebSocket client.</summary>
    [HideInInspector] public bool liveStreamSuppressTimeline;

    [Header("Camera Zoom")] 
    [SerializeField] private Transform bodyZoomCameraPlace;
    [SerializeField] private Transform faceZoomCameraPlace;
    [SerializeField] private Camera camera;
    
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

    [Header("Recording")] 
    [SerializeField] private GameObject recorder;
    
    private Quaternion characterRotation;
    private void Start()
    {
        estimatedPoses = new List<PoseJsonVector>();
        estimatedFacialMocap = new List<FaceJson>();
        estimatedHandPose = new List<HandJsonVector>();
        frameData = new List<FrameData>();
        characterRotation = character.transform.rotation;
        SetBodyZoomCamera();
        videoPlayer.Prepare();
        videoPlayer.Play();
        videoPlayer.frame = 0;
        videoPlayer.Pause();
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
            facialExpressionHandler.UpdateData(faceJson);
            videoPlayer.frame = faceJson.frame;
            videoPlayer.Play();
            videoPlayer.Pause();
        }
    }


    private IEnumerator TestCo()
    {
        while (true)
        {
            yield return new WaitForSeconds(nextFrameTime);
            currentAnimationSlot = (int) slider.value;
            if (debug)
            {
                // videoPlayer.frame = fileIndex-1;
                // videoPlayer.Play();
                // videoPlayer.Pause();
                if (timer > nextFrameTime)
                {
                    timer = 0;
                    if (!onlyCurrentIndex)
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
            }

            if (currentAnimationSlot >= frameData.Count)
                yield break;


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
            // currentFaceJson = currentFaceJsonNew;
            // currentFaceJsonNew = currentFrameData.faceData;
            currentFaceJson = currentFrameData.faceData;


            if (debug)
            {
                videoPlayer.frame = frameData[currentAnimationSlot].frame;
                videoPlayer.Play();
                videoPlayer.Pause();
            }

            timer = 0;
            currentAnimationSlot++;
            // currentFaceJson = estimatedFacialMocap[faceIndex];
            // faceIndex++;


            try
            {
                character.transform.rotation = Quaternion.identity;
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
                    facialExpressionHandler.UpdateData(currentFaceJson);
                }

                character.transform.rotation = characterRotation;
                slider.value = currentAnimationSlot;
            }
            catch (Exception e)
            {
                slider.value = currentAnimationSlot;
                character.transform.rotation = characterRotation;
                Console.WriteLine(e);
                throw;
                Debug.LogError("Problem occured: " + e.Message);
            }

            try
            {
                Debug.Log(videoPlayer.frame + "--" + currentFaceJson.frame);
                // if (videoPlayer.frame > currentFaceJson.frame || pause)
                //     videoPlayer.frame = currentFaceJson.frame;


            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
    }

    private bool framesLoaded = false;

    /// <summary>Fill <see cref="FrameData.timeSeconds"/> from FaceJson.time (ms) when available, else frame index / FPS.</summary>
    private void AssignTimelineSecondsToFrameData()
    {
        if (frameData == null || frameData.Count == 0)
            return;
        float fps = Mathf.Max(fallbackTimelineFps, 1f);
        if (enableVideo && videoPlayer != null && videoPlayer.frameRate > 1e-3f)
            fps = (float)videoPlayer.frameRate;
        float prev = 0f;
        for (int i = 0; i < frameData.Count; i++)
        {
            FrameData fd = frameData[i];
            float ts = fd.faceData != null && fd.faceData.time > 0.001f
                ? fd.faceData.time / 1000f
                : fd.frame / fps;
            if (ts < prev)
                ts = prev + 1e-5f;
            fd.timeSeconds = ts;
            prev = ts;
        }
    }

    private bool UsesVideoTimelineClock()
    {
        return enableVideo && videoPlayer != null && !string.IsNullOrEmpty(videoPlayer.url);
    }

    private float GetPlaybackTimelineSeconds()
    {
        if (UsesVideoTimelineClock())
            return Mathf.Max(0f, (float)videoPlayer.time);
        return Mathf.Max(0f, playbackElapsedSeconds);
    }

    private float ComputeTimelineLerpAlpha()
    {
        if (frameData == null || frameData.Count == 0)
            return 0f;
        int idx = Mathf.Clamp(currentAnimationSlot, 0, frameData.Count - 1);
        float playT = GetPlaybackTimelineSeconds();
        float t0 = frameData[idx].timeSeconds;
        float defaultSpan = 1f / Mathf.Max(fallbackTimelineFps, 1f);
        float fallbackSpan = nextFrameTime > 1e-5f ? nextFrameTime : defaultSpan;
        float t1 = idx + 1 < frameData.Count
            ? frameData[idx + 1].timeSeconds
            : t0 + Mathf.Max(fallbackSpan, defaultSpan);
        float span = Mathf.Max(t1 - t0, 1e-5f);
        return Mathf.Clamp01((playT - t0) / span);
    }

    private float GetTotalTimelineDurationSeconds()
    {
        if (frameData == null || frameData.Count == 0)
            return 0f;
        if (enableVideo && videoPlayer != null && videoPlayer.length > 0.01)
            return (float)videoPlayer.length;
        float last = frameData[frameData.Count - 1].timeSeconds;
        float spacing = frameData.Count >= 2
            ? Mathf.Max(
                frameData[frameData.Count - 1].timeSeconds - frameData[frameData.Count - 2].timeSeconds,
                1f / Mathf.Max(fallbackTimelineFps, 1f))
            : 1f / Mathf.Max(fallbackTimelineFps, 1f);
        return last + spacing;
    }

    private void FixedUpdate()
    {
        if (liveStreamSuppressTimeline)
            return;

        if (!pause)
            timer += Time.fixedDeltaTime;

        if (debug)
        {
            if (timer > nextFrameTime)
            {
                timer = 0;
                if (!onlyCurrentIndex)
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

        if (frameData == null || frameData.Count == 0)
            return;

        currentAnimationSlot = Mathf.Clamp((int)slider.value, 0, frameData.Count - 1);

        if (pause)
        {
            if (UsesVideoTimelineClock() && videoPlayer != null && videoPlayer.canSetTime)
                videoPlayer.time = frameData[currentAnimationSlot].timeSeconds;
            else
                playbackElapsedSeconds = frameData[currentAnimationSlot].timeSeconds;
        }
        else
        {
            if (!UsesVideoTimelineClock())
                playbackElapsedSeconds += Time.fixedDeltaTime;

            float playT = GetPlaybackTimelineSeconds();
            while (currentAnimationSlot + 1 < frameData.Count &&
                   frameData[currentAnimationSlot + 1].timeSeconds <= playT)
                currentAnimationSlot++;

            if (playT >= GetTotalTimelineDurationSeconds() - 1e-4f)
            {
                OnAnimationPlayFinish();
                return;
            }
        }

        currentAnimationSlot = Mathf.Clamp(currentAnimationSlot, 0, frameData.Count - 1);

        currentFrameData = frameData[currentAnimationSlot];
        currentPoseJsonVector = currentPoseJsonVectorNew;
        currentPoseJsonVectorNew = currentFrameData.poseData;
        currentHandJsonVector = currentHandJsonVectorNew;
        currentHandJsonVectorNew = currentFrameData.handData;
        currentFaceJson = currentFrameData.faceData;

        float lerpAlpha = ComputeTimelineLerpAlpha();

        try
        {
            character.transform.rotation = Quaternion.identity;
            if (currentPoseJsonVector != null)
            {
                if (currentPoseJsonVectorNew != null)
                {
                    for (int i = 0; i < currentPoseJsonVector.predictions.Length; i++)
                    {
                        currentPoseJsonVector.predictions[i].position = Vector3.Lerp(
                            currentPoseJsonVector.predictions[i].position,
                            currentPoseJsonVectorNew.predictions[i].position,
                            lerpAlpha);
                    }
                }

                pose3DMapper.Predict3DPose(currentPoseJsonVector);
            }

            if (currentHandJsonVector != null && enableHands)
            {
                if (currentHandJsonVectorNew != null)
                {
                    if (currentHandJsonVector.handsR.Length == currentHandJsonVectorNew.handsR.Length)
                        for (int i = 0; i < currentHandJsonVector.handsR.Length; i++)
                        {
                            currentHandJsonVector.handsR[i].position = Vector3.Lerp(
                                currentHandJsonVector.handsR[i].position,
                                currentHandJsonVectorNew.handsR[i].position,
                                lerpAlpha);
                        }

                    if (currentHandJsonVector.handsL.Length == currentHandJsonVectorNew.handsL.Length)
                        for (int i = 0; i < currentHandJsonVector.handsL.Length; i++)
                        {
                            currentHandJsonVector.handsL[i].position = Vector3.Lerp(
                                currentHandJsonVector.handsL[i].position,
                                currentHandJsonVectorNew.handsL[i].position,
                                lerpAlpha);
                        }
                }

                handPose.Predict3DPose(currentHandJsonVector);
            }

            if (currentFaceJson != null && enableFace)
                facialExpressionHandler.UpdateData(currentFaceJson);

            character.transform.rotation = characterRotation;
            slider.value = currentAnimationSlot;
        }
        catch (Exception e)
        {
            slider.value = currentAnimationSlot;
            character.transform.rotation = characterRotation;
            Debug.LogError("Problem occured: " + e.Message);

            Console.WriteLine(e);
            throw;
        }
    }


    private void OnAnimationPlayFinish()
    {
        pause = true;
        if (videoPlayer != null)
            videoPlayer.Pause();
        if (recording)
            StopRecording();
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
        int len = poseJson.predictions != null ? poseJson.predictions.Length : 0;
        PoseJsonVector poseJsonVector = new PoseJsonVector();
        poseJsonVector.predictions = new BodyPartVector[len];
        poseJsonVector.frame = poseJson.frame;
        poseJsonVector.width = poseJson.width;
        poseJsonVector.height = poseJson.height;
        for (int i = 0; i < len; i++)
        {
            poseJsonVector.predictions[i].position = fraction * new Vector3(-poseJson.predictions[i].x*fractionX * videoFractionX,
                -poseJson.predictions[i].y*fractionY*videoFractionY,poseJson.predictions[i].z*fractionZ*videoFractionZ);
            poseJsonVector.predictions[i].visibility = poseJson.predictions[i].visibility;
            
        }

        return poseJsonVector;
    }

    public void SetLiveStreamMode(bool enabled)
    {
        liveStreamSuppressTimeline = enabled;
        if (enabled)
            pause = true;
    }

    /// <summary>Apply one server live-mocap JSON text frame (main thread).</summary>
    public void ApplyLiveMocapFrameJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            return;
        if (json.IndexOf("\"error\"", StringComparison.Ordinal) >= 0)
        {
            var err = JsonUtility.FromJson<LiveMocapWsError>(json);
            if (!string.IsNullOrEmpty(err.error))
                Debug.LogWarning("Live mocap: " + err.error);
            return;
        }

        LiveMocapFrameDto dto = JsonUtility.FromJson<LiveMocapFrameDto>(json);
        if (dto == null)
            return;

        PoseJsonVector poseVec = null;
        if (dto.bodyPose != null && dto.bodyPose.predictions != null && dto.bodyPose.predictions.Length > 0)
            poseVec = GetBodyPartsVector(dto.bodyPose);

        HandJsonVector handVec = null;
        if (dto.handsPose != null)
            handVec = GetHandsVector(dto.handsPose);

        ApplyLiveMocapSnapshot(poseVec, handVec, dto.faceData);
    }

    public void ApplyLiveMocapSnapshot(PoseJsonVector poseVec, HandJsonVector handVec, FaceJson face)
    {
        try
        {
            character.transform.rotation = Quaternion.identity;
            if (poseVec != null)
                pose3DMapper.Predict3DPose(poseVec);
            if (handVec != null && enableHands)
                handPose.Predict3DPose(handVec);
            if (face != null && face.blendShapes != null && enableFace)
                facialExpressionHandler.UpdateData(face);
            character.transform.rotation = characterRotation;
        }
        catch (Exception e)
        {
            Debug.LogWarning("Live mocap apply failed: " + e.Message);
        }
    }
    
    
    private HandJsonVector GetHandsVector(HandJson handJson)
    {
        int len = handJson.handsR != null ? handJson.handsR.Length : 0;
        int len2 = handJson.handsL != null ? handJson.handsL.Length : 0;
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
        estimatedHandPose.Clear();
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
        estimatedPoses.Clear();
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

    public void LoadFrames(FrameData[] frameData)
    {
        this.frameData = frameData.ToList<FrameData>();
        handPose.DataCleaner(frameData);
        // for (int i = 0; i < frameData.Length; i++)
        // {
        //     PoseJsonVector p = frameData[i].poseData;
        //     if(p!=null)
        //     for (int j = 0; j < p.predictions.Length; j++)
        //     {
        //         p.predictions[j].position.y *= 1.7f;
        //     }
        // }
        Debug.Log(frameData.Length);
        AssignTimelineSecondsToFrameData();
        MakeSceneReady();
    }
    public void ArrangeDataFrames()
    {
        frameData.Clear();
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
                faceFrame = estimatedFacialMocap[faceIndex].frame;
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

            if (faceFrame == minFrame)
            {
                currentFrameData.faceData = estimatedFacialMocap[faceIndex];
                faceIndex++;
            }

            Debug.Log(minFrame);
            currentFrameData.frame = minFrame;
            frameData.Add(currentFrameData);
            index++;
        }

        AssignTimelineSecondsToFrameData();

        MakeSceneReady();
    }

    public FrameData[] GetFrameData()
    {
        
        Debug.Log(frameData.ToArray().Length);
        return frameData.ToArray();
    }

    //set fractions based on video aspect ratio
    public void SetVideoFractions(float aspectRatio)
    {
        if (enableVideoAspectRatioEffector)
        {

            videoFractionX = 1;
            videoFractionY = aspectRatio;
            videoFractionZ = 1;
        }
    }

    public void SetNewCharacter(GameObject newCharacter)
    {
      
        character.SetActive(false);
        character = newCharacter;
        pose3DMapper.SetCharacter(character);
        try
        {
            enableHands = true;
            handPose.SetCharacter(character);
        }
        catch (Exception e)
        {
            enableHands = false;
            Console.WriteLine(e);
        }
        if (character.GetComponentInChildren<BlendShapeController>() != null)
        {
            facialExpressionHandler.SetCharacter(newCharacter);
            enableFace = true;
        }
        else
        {
            enableFace = false;
        }

        characterRotation = character.transform.rotation;
        HideCharacter();
    }

    private void MakeSceneReady()
    {
        framesLoaded = true;
        slider.maxValue = frameData.Count;
        slider.interactable = true;
        UIManager.Instancce.ActiveAnimationControlPanel();
        currentPoseJsonVectorNew = frameData[0].poseData;
        currentHandJsonVectorNew = frameData[0].handData;
        currentFaceJsonNew = frameData[0].faceData;
        currentAnimationSlot = 0;
        // currentAnimationSlot = frameData[0].frame;
    }

    public void HideCharacter()
    {
        
        character.SetActive(false);
    }
    
    public void ShowCharacter()
    {
        character.SetActive(true);
    }

    public void OnTogglePlay()
    {
        if (videoPlayer != null && enableVideo && !string.IsNullOrEmpty(videoPlayer.url))
        {
            float fr = (float)videoPlayer.frameRate;
            nextFrameTime = fr > 1e-3f ? 1f / fr : 1f / Mathf.Max(fallbackTimelineFps, 1f);
            videoPlayer.frame = 0;
        }
        else
            nextFrameTime = 1f / Mathf.Max(fallbackTimelineFps, 1f);

        timer = 0;
        pause = !pause;

        if (!pause)
            playbackElapsedSeconds = 0f;

        if (videoPlayer != null && enableVideo && !string.IsNullOrEmpty(videoPlayer.url))
            test();
    }




    private bool recording = false;
    public void StartRecording()
    {
        if(!pause)
            return;
        UIManager.Instancce.DeActiveAnimationControlPanel();
        recorder.SetActive(true);
        recording = true;
        OnTogglePlay();
    }

    private void StopRecording()
    {
        recording = false;
        recorder.SetActive(false);
        UIManager.Instancce.ActiveAnimationControlPanel();
        UIManager.Instancce.ShowSuccessMessage("Animation Recorded successfully!");
    }
    
    private void test()
    {
        

        if(pause)
            videoPlayer.Pause();
        else
            videoPlayer.Play();
    }

    public void SetFaceOriginalVideo(string path)
    {
        if (!enableVideo || videoPlayer == null)
            return;
        videoPlayer.url = path;
        videoPlayer.Prepare();
        videoPlayer.Play();
        videoPlayer.frame = 0;
        videoPlayer.Pause();
        if (frameData != null && frameData.Count > 0)
            AssignTimelineSecondsToFrameData();
    }
    
    //Set Camera Zoom
    public void SetFaceZoomCamera()
    {
        camera.transform.position = faceZoomCameraPlace.position;
        camera.transform.rotation = faceZoomCameraPlace.rotation;
        recorder.transform.position = faceZoomCameraPlace.position;
        recorder.transform.rotation = faceZoomCameraPlace.rotation;
    }
    
    public void SetBodyZoomCamera()
    {
        camera.transform.position = bodyZoomCameraPlace.position;
        camera.transform.rotation = bodyZoomCameraPlace.rotation;
        recorder.transform.position = bodyZoomCameraPlace.position;
        recorder.transform.rotation = bodyZoomCameraPlace.rotation;
    }
}
