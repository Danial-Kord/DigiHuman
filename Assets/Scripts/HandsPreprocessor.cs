using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum HandPoints : int
{
    Wrist,
    ThumbFirst, //first thumb bone (nearest to the wrist)
    ThumbSecond, //second
    ThumbThird, //third
    ThumbFourth, //fourth
    IndexFingerFirst,
    IndexFingerSecond, 
    IndexFingerThird, 
    IndexFingerFourth, 
    MiddleFingerFirst,
    MiddleFingerSecond, 
    MiddleFingerThird, 
    MiddleFingerFourth, 
    RingFingerFirst,
    RingFingerSecond, 
    RingFingerThird, 
    RingFingerFourth, 
    PinkyFirst, //little finger = pinky
    PinkySecond, 
    PinkyThird, 
    PinkyFourth, 
}

public class HandsPreprocessor : CharacterMapper
{
    private JointPoint[] leftHand;
    private JointPoint[] rightHand;
    private JointPoint[] rightRootFingers;
    private JointPoint[] leftRootFingers;
    private HandPoints[] rootHandPoints;
    private GameObject[] RHandJointsDebug;
    private GameObject[] LHandJointsDebug;



    [Header("Anomaly Detection")] 
    [SerializeField] private bool enableAnomalyDetector;
    [SerializeField] private HandAnomalyDetector handAnomalyDetector;
    //anomaly denier
    private JointPoint rightElbow;
    private JointPoint leftElbow;
    private Vector3 lastFrameRHandDirection = Vector3.zero;
    private Vector3 lastFrameLHandDirection = Vector3.zero;

    public bool flipHands;
    
    protected override void InitializationHumanoidPose()
    {
        InitializeRightHand();
        InitializeLeftHand();
        
        if (debugMode)
        {
            RHandJointsDebug = new GameObject[21];
            LHandJointsDebug = new GameObject[21];
            for (int i = 0; i < RHandJointsDebug.Length; i++)
            {
                RHandJointsDebug[i] = Instantiate(debugGameObject);
            }
            for (int i = 0; i < LHandJointsDebug.Length; i++)
            {
                LHandJointsDebug[i] = Instantiate(debugGameObject);
            }
        }
        
        rightRootFingers = new JointPoint[5];
        leftRootFingers = new JointPoint[5];
        rootHandPoints = new[]
        {
            HandPoints.ThumbFirst,
            HandPoints.PinkyFirst,
            HandPoints.IndexFingerFirst,
            HandPoints.MiddleFingerFirst,
            HandPoints.RingFingerFirst,
        };
        InitializeRootFingers(rightHand,rightRootFingers);
        InitializeRootFingers(leftHand,leftRootFingers);
        SetupInverseAndDistance(rightHand);
        SetupInverseAndDistance(leftHand);
        
    }

    public override void Predict3DPose(PoseJsonVector poseJsonVector)
    {
        throw new NotImplementedException();
    }


    public void DataCleaner(FrameData[] frameDatas)
    {
        if(enableAnomalyDetector)
            handAnomalyDetector.WrongLandmarkDetector(frameDatas);
    }
        


    private void PredictHandPose(BodyPartVector[] handLandmarks, JointPoint[] hand)
    {

        for (int i = 0; i < handLandmarks.Length; i++)
        {
            hand[i].LandmarkPose = handLandmarks[i].position;
        }
          

        // Vector3 forwardFinger = wrist.Transform.position
        //     .TriangleNormal(pinkyFirstLandmark.Transform.position,indexFingerFirst.Transform.position);
        
        // upward.Normalize();
        


        
        /*
        for (int i = 0; i < rootHandPoints.Length; i++)
        {

            JointPoint bone = hand[(int)rootHandPoints[i]];
            float distance = bone.DistanceFromDad;
            Vector3 direction = (-wrist.LandmarkPose + bone.LandmarkPose) / (bone.LandmarkPose - wrist.LandmarkPose).magnitude;

            bone.Transform.position = wrist.Transform.position + direction * distance;

        }
       */

        //setting bone positions
        for (int i = 0; i < hand.Length; i++)
        {
            
            JointPoint bone = hand[i];

            if (bone.Transform != null) //fourth finger node is not real!
            {
                bone.WorldPos = bone.Transform.position;

            }
        
        }
        //setting bone positions
        for (int i = 0; i < hand.Length; i++)
        {
            JointPoint bone = hand[i];
            if (bone.Child != null)
            {
                if (bone.Child.Transform != null) //fourth finger node is not real!
                {
                    JointPoint child = bone.Child;
                    float distance = bone.DistanceFromChild;
                    Vector3 direction = (-bone.LandmarkPose + child.LandmarkPose) 
                                        / (-bone.LandmarkPose + child.LandmarkPose).magnitude;
                    child.WorldPos = bone.WorldPos + direction * distance;
                    // child.Transform.position = child.WorldPos;

                    continue;
                }
            }
        }
        

        if(enableKalmanFilter)
            for (int i = 0; i < hand.Length; i++)
            {
                if(hand[i].Transform != null)
                    KalmanUpdate(hand[i]);
            }
        else
        {
            for (int i = 0; i < hand.Length; i++)
            {
                if (hand[i].Transform != null)
                    hand[i].FilteredPos = hand[i].WorldPos;
            }
        }

            // Low pass filter
            if (useLowPassFilter)
            {
                foreach (var jp in hand)
                {
                    jp.LastPoses[0] = jp.FilteredPos;
                    for (var i = 1; i < jp.LastPoses.Length; i++)
                    {
                        jp.LastPoses[i] = jp.LastPoses[i] * lowPassParam + jp.LastPoses[i - 1] * (1f - lowPassParam);
                    }
                    jp.FilteredPos = jp.LastPoses[jp.LastPoses.Length - 1];
                }
            }
            
            JointPoint indexFingerFirst = hand[(int) HandPoints.IndexFingerFirst];
            JointPoint wrist = hand[(int) HandPoints.Wrist];
            JointPoint pinkyFirstLandmark = hand[(int) HandPoints.PinkyFirst];
            Vector3 forwardFinger = wrist.FilteredPos
                .TriangleNormal(indexFingerFirst.FilteredPos,pinkyFirstLandmark.FilteredPos);


            
            //Rotation of the whole hand at First!
            Vector3 normal = wrist.LandmarkPose.TriangleNormal(indexFingerFirst.LandmarkPose,pinkyFirstLandmark.LandmarkPose);
            hand[(int) HandPoints.Wrist].Transform.rotation = Quaternion.LookRotation(-wrist.LandmarkPose + (indexFingerFirst.LandmarkPose + pinkyFirstLandmark.LandmarkPose)/2.0f, normal) * wrist.InverseRotation;

            //Method2
            // Vector3 normal = wrist.WorldPos.TriangleNormal(indexFingerFirst.WorldPos,pinkyFirstLandmark.WorldPos);
            // hand[(int) HandPoints.Wrist].Transform.rotation = Quaternion.LookRotation(-wrist.WorldPos + (indexFingerFirst.WorldPos + pinkyFirstLandmark.WorldPos)/2.0f, normal) * wrist.InverseRotation;
            //

            //Method3
            // Vector3 normal = wrist.FilteredPos.TriangleNormal(indexFingerFirst.FilteredPos,pinkyFirstLandmark.FilteredPos);
            // hand[(int) HandPoints.Wrist].Transform.rotation = Quaternion.LookRotation(wrist.FilteredPos - (indexFingerFirst.FilteredPos + pinkyFirstLandmark.FilteredPos)/2.0f, normal) * wrist.InverseRotation;
            //

            
        //rotation
        
        for (int i = 0; i < hand.Length; i++)
        {
            
            // if(i == (int) HandPoints.PinkyThird || i == (int) HandPoints.ThumbThird || 
            //    i == (int) HandPoints.IndexFingerThird || i == (int) HandPoints.RingFingerThird
            //    || i == (int) HandPoints.MiddleFingerThird)
            //     continue;
            JointPoint bone = hand[i];
            
            // if (bone.Parent != null)
            // {
            //     print(bone.Parent.Transform.name);
            //     Vector3 fv = bone.Parent.LandmarkPose - bone.LandmarkPose;
            //     Vector3 forward1 = -bone.LandmarkPose + bone.Child.LandmarkPose;
            //     Vector3 eulur = bone.InitialRotation;
            //     eulur.x += Vector3.Angle(fv, forward1);
            //     bone.Transform.eulerAngles = eulur;
            // }
            // else if (bone.Child != null)
            // {
            //     //forward = hand[(int) HandPoints.Wrist].Transform.position - bone.Transform.position;
            //     //bone.Transform.rotation = Quaternion.LookRotation(bone.LandmarkPose- bone.Child.LandmarkPose, (hand[(int) HandPoints.Wrist].LandmarkPose - bone.LandmarkPose)) * bone.InverseRotation;
            //     // bone.Transform.rotation = Quaternion.LookRotation(bone.LandmarkPose- bone.Child.LandmarkPose, (hand[(int) HandPoints.Wrist].LandmarkPose - bone.LandmarkPose)) * bone.InverseRotation;
            //     //bone.Transform.rotation = Quaternion.LookRotation(bone.LandmarkPose- bone.Child.LandmarkPose, upward) * bone.InverseRotation;
            // }
            
            if (bone.Parent != null)
            {
               //print(bone.Parent.Transform.name);
                
               //normalR or forwardFinger this is the problem!
               
                //Method1
                try
                {
                    //1
                    // Vector3 fv = bone.Parent.FilteredPos - bone.FilteredPos;
                    // bone.Transform.rotation = Quaternion.LookRotation(bone.FilteredPos- bone.Child.FilteredPos, fv) * bone.InverseRotation;
                    //
                    // 1'
                    // bone.Transform.rotation = Quaternion.LookRotation(bone.Transform.position- bone.Child.Transform.position, (forwardFinger + bone.Parent.Transform.right)/2.0f) * bone.InverseRotation;
                    
                    bone.Transform.rotation = Quaternion.LookRotation(bone.FilteredPos- bone.Child.FilteredPos, forwardFinger) * bone.InverseRotation;
                    
                    //2
                    // bone.Transform.rotation = Quaternion.LookRotation(bone.Transform.position- bone.Child.Transform.position, forwardFinger) 
                    //                           * Quaternion.LookRotation(bone.Transform.position- bone.Child.Transform.position, fv)
                    //                           * bone.InverseRotation;

                }
                catch (Exception e)
                {
                    //Method1
                    // Vector3 fv = bone.Parent.LandmarkPose - bone.LandmarkPose;
                    // bone.Transform.rotation = Quaternion.LookRotation(bone.LandmarkPose- bone.Child.LandmarkPose, fv) * bone.InverseRotation;
                    //
                    //Method2
                    //1
                    bone.Transform.rotation = Quaternion.LookRotation(bone.LandmarkPose- bone.Child.LandmarkPose, forwardFinger) * bone.InverseRotation;
                    //2
                    // bone.Transform.rotation = Quaternion.LookRotation(bone.LandmarkPose- bone.Child.LandmarkPose, forwardFinger) 
                    //                           *Quaternion.LookRotation(bone.LandmarkPose- bone.Child.LandmarkPose, fv) 
                    //                           * bone.InverseRotation;

                    // Debug.Log("problem with bone: "+bone.Transform.name);
                }
                
                //Method2
                // Vector3 fv = bone.Parent.LandmarkPose - bone.LandmarkPose;
                // bone.Transform.rotation = Quaternion.LookRotation(bone.LandmarkPose- bone.Child.LandmarkPose, fv) * bone.InverseRotation;

            }
            else if (bone.Child != null)
            {
                // print(bone.Transform.name);
                //forward = hand[(int) HandPoints.Wrist].Transform.position - bone.Transform.position;
                //Method1
                //bone.Transform.rotation = Quaternion.LookRotation(bone.Transform.position- bone.Child.Transform.position, (bone.Transform.parent.position - bone.Transform.position)) * bone.InverseRotation;
                //Method2
                // bone.Transform.rotation = Quaternion.LookRotation(bone.LandmarkPose- bone.Child.LandmarkPose, (hand[(int) HandPoints.Wrist].LandmarkPose - bone.LandmarkPose)) * bone.InverseRotation;
                //bone.Transform.rotation = Quaternion.LookRotation(bone.LandmarkPose- bone.Child.LandmarkPose, upward) * bone.InverseRotation;
                //Method3
                // bone.Transform.rotation = Quaternion.LookRotation(bone.Transform.position- bone.Child.Transform.position, (wrist.Transform.position - bone.Transform.position)) * bone.InverseRotation;
                //Method4
                bone.Transform.rotation = Quaternion.LookRotation(bone.FilteredPos- bone.Child.FilteredPos, forwardFinger) * bone.InverseRotation;

            }
            /*
            if (bone.Parent != null)
            {
                Vector3 fv = bone.Parent.Transform.position - bone.Transform.position;
                bone.Transform.rotation = Quaternion.LookRotation(bone.Transform.position- bone.Child.Transform.position, fv) * bone.InverseRotation;
            }
            
            else if (bone.Child != null)
            {
                bone.Transform.rotation = Quaternion.LookRotation(bone.Transform.position- bone.Child.Transform.position, forward) * bone.InverseRotation;
            }
            */
        }
        
        //Rotation of the whole hand at the end!
        // Vector3 normal = wrist.LandmarkPose.TriangleNormal(indexFingerFirst.LandmarkPose,pinkyFirstLandmark.LandmarkPose);
        // hand[(int) HandPoints.Wrist].Transform.rotation = Quaternion.LookRotation(-wrist.LandmarkPose + (indexFingerFirst.LandmarkPose + pinkyFirstLandmark.LandmarkPose)/2.0f, normal) * wrist.InverseRotation;

        //Method2
        // Vector3 normal = wrist.Transform.position.TriangleNormal(indexFingerFirst.Transform.position,pinkyFirstLandmark.Transform.position);
        // hand[(int) HandPoints.Wrist].Transform.rotation = Quaternion.LookRotation(-wrist.Transform.position + (indexFingerFirst.Transform.position + pinkyFirstLandmark.Transform.position)/2.0f, normal) * wrist.InverseRotation;
        //     

        //Method3
        // Vector3 normal = wrist.FilteredPos.TriangleNormal(indexFingerFirst.FilteredPos,pinkyFirstLandmark.FilteredPos);
        // hand[(int) HandPoints.Wrist].Transform.rotation = Quaternion.LookRotation(-wrist.FilteredPos + (indexFingerFirst.FilteredPos + pinkyFirstLandmark.FilteredPos)/2.0f, normal) * wrist.InverseRotation;
        //
        
    }
    
    public void Predict3DPose(HandJsonVector poseJsonVector)
    {
        try
        {
            //right hand
            BodyPartVector[] handR = poseJsonVector.handsR;
            BodyPartVector[] handL = poseJsonVector.handsL;
            if (flipHands)
            {
                handR = poseJsonVector.handsL;
                handL = poseJsonVector.handsR;
            }
            if(handR != null)
                if (handR.Length != 0)
                {
                    int rHandStat = RightHandAnomalyDetector(handR);
                    if (rHandStat == 0)
                    {
                        PredictHandPose(handR, rightHand);
                    }
                    else if(rHandStat != 2)
                    {
                        if(handL != null)
                            if(handL.Length != 0)
                                PredictHandPose(handL, rightHand);
                        Debug.Log("Anomaly detected! R");
                    }
                    if(debugMode)
                        for (int i = 0; i < handR.Length; i++)
                        {
                            RHandJointsDebug[i].transform.position = handR[i].position;
                        }
                }

            if(handL != null)
                if (handL.Length != 0)
                {
                    int lHandStat = LeftHandAnomalyDetector(handL);
                    if (lHandStat == 0)
                    {
                        PredictHandPose(handL, leftHand);
                    }
                    else if(lHandStat != 2)
                    {
                        if(handR != null)
                            if(handR.Length != 0)
                                PredictHandPose(handR, leftHand);
                        Debug.Log("Anomaly detected! L");
                    }
                    if(debugMode)
                        for (int i = 0; i < handL.Length; i++)
                        {
                            LHandJointsDebug[i].transform.position = handL[i].position;
                        }
                }
            
        }
        catch (Exception e)
        {
            Debug.LogError("Hand Problem!");

           Console.WriteLine(e);
            throw;
        }

    }

    
    //Anomaly detectors
    // 0 means its all good
    // 1 means should flip hands
    // 2 means should not calculate current frame
    private int RightHandAnomalyDetector(BodyPartVector[] handLandmarks)
    {
        return 0;
        Vector3 rightArmVector = rightHand[(int) HandPoints.Wrist].Transform.position - rightElbow.Transform.position;
        Vector3 indexFingerFirst = handLandmarks[(int) HandPoints.IndexFingerFirst].position;
        Vector3 wrist = handLandmarks[(int) HandPoints.Wrist].position;
        Vector3 pinkyFirstLandmark = handLandmarks[(int) HandPoints.PinkyFirst].position;
        Vector3 handVector =-wrist + (indexFingerFirst + pinkyFirstLandmark)/2.0f;

        Vector3 newHandDirection = wrist
            .TriangleNormal(indexFingerFirst,pinkyFirstLandmark);
        if (!lastFrameRHandDirection.Equals(Vector3.zero))
        {
            if (Vector3.Angle(newHandDirection, lastFrameRHandDirection) > 100 ||
                Vector3.Angle(newHandDirection, lastFrameRHandDirection) < -100)
            {
                Debug.Log("1 frame changed and more than 100 degree rotation!");
                // lastFrameRHandDirection = newHandDirection;
                return 2;
            }
        }
        lastFrameRHandDirection = newHandDirection;
        if (Vector3.Angle(rightArmVector, handVector) > 100 || Vector3.Angle(rightArmVector, handVector) < -100)
        {
            Debug.Log(Vector3.Angle(rightArmVector, handVector));

            return 1;
        }

        return 0;
    }
    private int LeftHandAnomalyDetector(BodyPartVector[] handLandmarks)
    {
        return 0;
        Vector3 leftArmVector = leftHand[(int) HandPoints.Wrist].Transform.position - leftElbow.Transform.position;
        Vector3 indexFingerFirst = handLandmarks[(int) HandPoints.IndexFingerFirst].position;
        Vector3 wrist = handLandmarks[(int) HandPoints.Wrist].position;
        Vector3 pinkyFirstLandmark = handLandmarks[(int) HandPoints.PinkyFirst].position;
        Vector3 handVector =-wrist + (indexFingerFirst + pinkyFirstLandmark)/2.0f;

        Vector3 newHandDirection = wrist
            .TriangleNormal(indexFingerFirst,pinkyFirstLandmark);
        
        
        if (!lastFrameLHandDirection.Equals(Vector3.zero))
        {
            if (Vector3.Angle(newHandDirection, lastFrameLHandDirection) > 100 ||
                Vector3.Angle(newHandDirection, lastFrameLHandDirection) < -100)
            {
                Debug.Log("1 frame changed and more than 100 degree rotation!");
                // lastFrameRHandDirection = newHandDirection;
                return 2;
            }
        }
        lastFrameLHandDirection = newHandDirection;

        if (Vector3.Angle(leftArmVector, handVector) > 100 || Vector3.Angle(leftArmVector, handVector) < -100)
        {
            Debug.Log(Vector3.Angle(leftArmVector, handVector));
            return 1;
        }
        return 0;
    }

    public void ClearFramesCache()
    {
        lastFrameRHandDirection = Vector3.zero;
        lastFrameLHandDirection = Vector3.zero;
    }

    private void SetupInverseAndDistance(JointPoint[] jointPoints)
    {
        for (int i = 0; i < jointPoints.Length; i++)
        {
            if (jointPoints[i].Child != null)
            {
                if(jointPoints[i].Child.Transform != null)            
                {
                    jointPoints[i].DistanceFromChild = Vector3.Distance(jointPoints[i].Child.Transform.position,
                        jointPoints[i].Transform.position);
                }
            }

            if (jointPoints[i].Transform != null)
            {
                jointPoints[i].InitialRotation = jointPoints[i].Transform.eulerAngles;
            }
        }
        
        
        
        
        for (int i = 0; i < rootHandPoints.Length; i++)
        {
            jointPoints[(int)rootHandPoints[i]].DistanceFromDad = Vector3.Distance(jointPoints[(int)HandPoints.Wrist].Transform.position,
                    jointPoints[(int)rootHandPoints[i]].Transform.position);
        }
        
        // Set Inverse
        Vector3 indexFingerFirst = jointPoints[(int) HandPoints.IndexFingerFirst].Transform.position;
        Vector3 b = jointPoints[(int) HandPoints.Wrist].Transform.position;
        Vector3 pinkyFirst = jointPoints[(int) HandPoints.PinkyFirst].Transform.position;
        // var forward = b.TriangleNormal(pinkyFirst,indexFingerFirst);
        var forward = b.TriangleNormal(indexFingerFirst,pinkyFirst);
        
        
        
        // Vector3 a = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position;
        // Vector3 b = anim.GetBoneTransform(HumanBodyBones.Hips).position;
        // Vector3 c = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg).position;
        // var forward = b.TriangleNormal(a,c);
        foreach (var jointPoint in jointPoints)
        {
            if (jointPoint.Transform != null)
            {
                jointPoint.InitRotation = jointPoint.Transform.rotation;
            }

            if (jointPoint.Child != null)
            {
                if (jointPoint.Child.Transform != null)
                {
                    //Method1
                    // jointPoint.Inverse = Quaternion.Inverse(Quaternion.LookRotation(jointPoint.Transform.position - jointPoint.Child.Transform.position, forward));
                    
                    //Method2
                    // jointPoint.Inverse = Quaternion.Inverse(Quaternion.LookRotation(jointPoint.Transform.position - jointPoint.Child.Transform.position, b-jointPoint.Transform.position));
                    
                    //Method3

                    Vector3 f = forward;
                    // if (jointPoint.Parent.Transform != null)
                    //     f = jointPoint.Child.Transform.forward;
                    
                    jointPoint.Inverse = Quaternion.Inverse(Quaternion.LookRotation(jointPoint.Transform.position - jointPoint.Child.Transform.position, f));

                    jointPoint.InverseRotation = jointPoint.Inverse * jointPoint.InitRotation;
                    
                    //Method4
                    // if (jointPoint.Parent != null)
                    // {
                    //     jointPoint.Inverse = Quaternion.Inverse(Quaternion.LookRotation(jointPoint.Transform.position - jointPoint.Child.Transform.position, jointPoint.Parent.Transform.position-jointPoint.Transform.position));
                    //     jointPoint.InverseRotation = jointPoint.Inverse * jointPoint.InitRotation;
                    // }
                    // else
                    // {
                    //     jointPoint.Inverse = Quaternion.Inverse(Quaternion.LookRotation(jointPoint.Transform.position - jointPoint.Child.Transform.position, b-jointPoint.Transform.position));
                    //     jointPoint.InverseRotation = jointPoint.Inverse * jointPoint.InitRotation;
                    // }

                }
            }
            else if(jointPoint.Parent != null)
            {
                jointPoint.Child.Inverse = jointPoint.Parent.Inverse;
                jointPoint.Child.InverseRotation = jointPoint.Parent.InverseRotation;
            }
        }
        
        //set inverse rotation wrist
        var wrist = jointPoints[(int)HandPoints.Wrist];
        wrist.InitRotation = wrist.Transform.rotation;
        var rf = wrist.Transform.position.TriangleNormal(indexFingerFirst,
            pinkyFirst);
        wrist.Inverse = Quaternion.Inverse(Quaternion.LookRotation(-wrist.Transform.position +(indexFingerFirst + pinkyFirst)/2.0f, rf));
        wrist.InverseRotation = wrist.Inverse * wrist.InitRotation;
        
    }



    private void InitializeRootFingers(JointPoint[] bones,JointPoint[] rootFingers)
    {
        rootFingers[0] = new JointPoint(){
        Transform = bones[(int) HandPoints.ThumbFirst].Transform
        }; 
        rootFingers[1] = new JointPoint(){
        Transform = bones[(int) HandPoints.PinkyFirst].Transform
        }; 
        rootFingers[2] = new JointPoint(){
        Transform = bones[(int) HandPoints.MiddleFingerFirst].Transform
        }; 
        rootFingers[3] = new JointPoint(){
        Transform = bones[(int) HandPoints.RingFingerFirst].Transform
        }; 
        rootFingers[4] = new JointPoint(){
        Transform = bones[(int) HandPoints.IndexFingerFirst].Transform
        }; 
        for (int i = 0; i < rootFingers.Length; i++)
        {
            rootFingers[i].Parent = bones[(int) HandPoints.Wrist];
        }
    }

    private void InitializeRightHand()
    {
        // Right Hand
        rightHand = new JointPoint[21];

        for (var i = 0; i < rightHand.Length; i++)
        {
            rightHand[i] = new JointPoint();
            rightHand[i].LastPoses = new Vector3[lowPassFilterChannels];

        }
        
        //Wrist
        rightHand[(int) HandPoints.Wrist].Transform = anim.GetBoneTransform(HumanBodyBones.RightHand);

        //thumb
        rightHand[(int) HandPoints.ThumbFirst].Transform = anim.GetBoneTransform(HumanBodyBones.RightThumbProximal);
        rightHand[(int) HandPoints.ThumbSecond].Transform = anim.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
        rightHand[(int) HandPoints.ThumbThird].Transform = anim.GetBoneTransform(HumanBodyBones.RightThumbDistal);
        //child and parent
        rightHand[(int) HandPoints.ThumbFirst].Child = rightHand[(int) HandPoints.ThumbSecond];
        rightHand[(int) HandPoints.ThumbSecond].Child = rightHand[(int) HandPoints.ThumbThird];
        rightHand[(int) HandPoints.ThumbThird].Child = rightHand[(int) HandPoints.ThumbFourth];
        rightHand[(int) HandPoints.ThumbSecond].Parent = rightHand[(int) HandPoints.ThumbFirst];
        rightHand[(int) HandPoints.ThumbThird].Parent = rightHand[(int) HandPoints.ThumbSecond];
        
        //index
        rightHand[(int) HandPoints.IndexFingerFirst].Transform = anim.GetBoneTransform(HumanBodyBones.RightIndexProximal);
        rightHand[(int) HandPoints.IndexFingerSecond].Transform = anim.GetBoneTransform(HumanBodyBones.RightIndexIntermediate);
        rightHand[(int) HandPoints.IndexFingerThird].Transform = anim.GetBoneTransform(HumanBodyBones.RightIndexDistal);
        //child and parent
        rightHand[(int) HandPoints.IndexFingerFirst].Child = rightHand[(int) HandPoints.IndexFingerSecond];
        rightHand[(int) HandPoints.IndexFingerSecond].Child = rightHand[(int) HandPoints.IndexFingerThird];
        rightHand[(int) HandPoints.IndexFingerThird].Child = rightHand[(int) HandPoints.IndexFingerFourth];
        rightHand[(int) HandPoints.IndexFingerSecond].Parent = rightHand[(int) HandPoints.IndexFingerFirst];
        rightHand[(int) HandPoints.IndexFingerThird].Parent = rightHand[(int) HandPoints.IndexFingerSecond];
        
        //middle
        rightHand[(int) HandPoints.MiddleFingerFirst].Transform = anim.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
        rightHand[(int) HandPoints.MiddleFingerSecond].Transform = anim.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate);
        rightHand[(int) HandPoints.MiddleFingerThird].Transform = anim.GetBoneTransform(HumanBodyBones.RightMiddleDistal);
        //child and parent
        rightHand[(int) HandPoints.MiddleFingerFirst].Child = rightHand[(int) HandPoints.MiddleFingerSecond];
        rightHand[(int) HandPoints.MiddleFingerSecond].Child = rightHand[(int) HandPoints.MiddleFingerThird];
        rightHand[(int) HandPoints.MiddleFingerThird].Child = rightHand[(int) HandPoints.MiddleFingerFourth];
        rightHand[(int) HandPoints.MiddleFingerSecond].Parent = rightHand[(int) HandPoints.MiddleFingerFirst];
        rightHand[(int) HandPoints.MiddleFingerThird].Parent = rightHand[(int) HandPoints.MiddleFingerSecond];
        
        //ring
        rightHand[(int) HandPoints.RingFingerFirst].Transform = anim.GetBoneTransform(HumanBodyBones.RightRingProximal);
        rightHand[(int) HandPoints.RingFingerSecond].Transform = anim.GetBoneTransform(HumanBodyBones.RightRingIntermediate);
        rightHand[(int) HandPoints.RingFingerThird].Transform = anim.GetBoneTransform(HumanBodyBones.RightRingDistal);
        //child and parent
        rightHand[(int) HandPoints.RingFingerFirst].Child = rightHand[(int) HandPoints.RingFingerSecond];
        rightHand[(int) HandPoints.RingFingerSecond].Child = rightHand[(int) HandPoints.RingFingerThird];
        rightHand[(int) HandPoints.RingFingerThird].Child = rightHand[(int) HandPoints.RingFingerFourth];
        rightHand[(int) HandPoints.RingFingerSecond].Parent = rightHand[(int) HandPoints.RingFingerFirst];
        rightHand[(int) HandPoints.RingFingerThird].Parent = rightHand[(int) HandPoints.RingFingerSecond];
        
        //pinky
        rightHand[(int) HandPoints.PinkyFirst].Transform = anim.GetBoneTransform(HumanBodyBones.RightLittleProximal);
        rightHand[(int) HandPoints.PinkySecond].Transform = anim.GetBoneTransform(HumanBodyBones.RightLittleIntermediate);
        rightHand[(int) HandPoints.PinkyThird].Transform = anim.GetBoneTransform(HumanBodyBones.RightLittleDistal);
        //child and parent
        rightHand[(int) HandPoints.PinkyFirst].Child = rightHand[(int) HandPoints.PinkySecond];
        rightHand[(int) HandPoints.PinkySecond].Child = rightHand[(int) HandPoints.PinkyThird];
        rightHand[(int) HandPoints.PinkyThird].Child = rightHand[(int) HandPoints.PinkyFourth];
        rightHand[(int) HandPoints.PinkySecond].Parent = rightHand[(int) HandPoints.PinkyFirst];
        rightHand[(int) HandPoints.PinkyThird].Parent = rightHand[(int) HandPoints.PinkySecond];

        //elbow
        rightElbow = new JointPoint();
        rightElbow.Transform = anim.GetBoneTransform(HumanBodyBones.RightLowerArm);
    }

    private void InitializeLeftHand()
    {
        // Right Hand
        leftHand = new JointPoint[21];

        for (var i = 0; i < leftHand.Length; i++)
        {
            leftHand[i] = new JointPoint();
            leftHand[i].LastPoses = new Vector3[lowPassFilterChannels];

        }
        
        //Wrist
        leftHand[(int) HandPoints.Wrist].Transform = anim.GetBoneTransform(HumanBodyBones.LeftHand);

        //thumb
        leftHand[(int) HandPoints.ThumbFirst].Transform = anim.GetBoneTransform(HumanBodyBones.LeftThumbProximal);
        leftHand[(int) HandPoints.ThumbSecond].Transform = anim.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
        leftHand[(int) HandPoints.ThumbThird].Transform = anim.GetBoneTransform(HumanBodyBones.LeftThumbDistal);
        //child and parent
        leftHand[(int) HandPoints.ThumbFirst].Child = leftHand[(int) HandPoints.ThumbSecond];
        leftHand[(int) HandPoints.ThumbSecond].Child = leftHand[(int) HandPoints.ThumbThird];
        leftHand[(int) HandPoints.ThumbThird].Child = leftHand[(int) HandPoints.ThumbFourth];
        leftHand[(int) HandPoints.ThumbSecond].Parent = leftHand[(int) HandPoints.ThumbFirst];
        leftHand[(int) HandPoints.ThumbThird].Parent = leftHand[(int) HandPoints.ThumbSecond];
        
        //index
        leftHand[(int) HandPoints.IndexFingerFirst].Transform = anim.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
        leftHand[(int) HandPoints.IndexFingerSecond].Transform = anim.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate);
        leftHand[(int) HandPoints.IndexFingerThird].Transform = anim.GetBoneTransform(HumanBodyBones.LeftIndexDistal);
        //child and parent
        leftHand[(int) HandPoints.IndexFingerFirst].Child = leftHand[(int) HandPoints.IndexFingerSecond];
        leftHand[(int) HandPoints.IndexFingerSecond].Child = leftHand[(int) HandPoints.IndexFingerThird];
        leftHand[(int) HandPoints.IndexFingerThird].Child = leftHand[(int) HandPoints.IndexFingerFourth];
        leftHand[(int) HandPoints.IndexFingerSecond].Parent = leftHand[(int) HandPoints.IndexFingerFirst];
        leftHand[(int) HandPoints.IndexFingerThird].Parent = leftHand[(int) HandPoints.IndexFingerSecond];
        
        //middle
        leftHand[(int) HandPoints.MiddleFingerFirst].Transform = anim.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
        leftHand[(int) HandPoints.MiddleFingerSecond].Transform = anim.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate);
        leftHand[(int) HandPoints.MiddleFingerThird].Transform = anim.GetBoneTransform(HumanBodyBones.LeftMiddleDistal);
        //child and parent
        leftHand[(int) HandPoints.MiddleFingerFirst].Child = leftHand[(int) HandPoints.MiddleFingerSecond];
        leftHand[(int) HandPoints.MiddleFingerSecond].Child = leftHand[(int) HandPoints.MiddleFingerThird];
        leftHand[(int) HandPoints.MiddleFingerThird].Child = leftHand[(int) HandPoints.MiddleFingerFourth];
        leftHand[(int) HandPoints.MiddleFingerSecond].Parent = leftHand[(int) HandPoints.MiddleFingerFirst];
        leftHand[(int) HandPoints.MiddleFingerThird].Parent = leftHand[(int) HandPoints.MiddleFingerSecond];
        
        //ring
        leftHand[(int) HandPoints.RingFingerFirst].Transform = anim.GetBoneTransform(HumanBodyBones.LeftRingProximal);
        leftHand[(int) HandPoints.RingFingerSecond].Transform = anim.GetBoneTransform(HumanBodyBones.LeftRingIntermediate);
        leftHand[(int) HandPoints.RingFingerThird].Transform = anim.GetBoneTransform(HumanBodyBones.LeftRingDistal);
        //child and parent
        leftHand[(int) HandPoints.RingFingerFirst].Child = leftHand[(int) HandPoints.RingFingerSecond];
        leftHand[(int) HandPoints.RingFingerSecond].Child = leftHand[(int) HandPoints.RingFingerThird];
        leftHand[(int) HandPoints.RingFingerThird].Child = leftHand[(int) HandPoints.RingFingerFourth];
        leftHand[(int) HandPoints.RingFingerSecond].Parent = leftHand[(int) HandPoints.RingFingerFirst];
        leftHand[(int) HandPoints.RingFingerThird].Parent = leftHand[(int) HandPoints.RingFingerSecond];
        
        //pinky
        leftHand[(int) HandPoints.PinkyFirst].Transform = anim.GetBoneTransform(HumanBodyBones.LeftLittleProximal);
        leftHand[(int) HandPoints.PinkySecond].Transform = anim.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate);
        leftHand[(int) HandPoints.PinkyThird].Transform = anim.GetBoneTransform(HumanBodyBones.LeftLittleDistal);
        //child and parent
        leftHand[(int) HandPoints.PinkyFirst].Child = leftHand[(int) HandPoints.PinkySecond];
        leftHand[(int) HandPoints.PinkySecond].Child = leftHand[(int) HandPoints.PinkyThird];
        leftHand[(int) HandPoints.PinkyThird].Child = leftHand[(int) HandPoints.PinkyFourth];
        leftHand[(int) HandPoints.PinkySecond].Parent = leftHand[(int) HandPoints.PinkyFirst];
        leftHand[(int) HandPoints.PinkyThird].Parent = leftHand[(int) HandPoints.PinkySecond];
        
        //elbow
        leftElbow = new JointPoint();
        leftElbow.Transform = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
    }
}
