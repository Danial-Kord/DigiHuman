using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public enum BodyPoints : int
{
Nose,
LeftEyeInner,
LeftEye,
LeftEyeOuter,
RightEyeInner,
RightEye,
RightEyeOuter,
LeftEar,
RightEar,
LeftMouth,
RightMouth,
LeftShoulder,
RightShoulder,
LeftElbow,
RightElbow,
LeftWrist,
RightWrist,
LeftPinky,
RightPinky,
LeftIndex,
RightIndex,
LeftThump,
RightThump,
LeftHip,
RightHip,
LeftKnee,
RightKnee,
LeftAnkle,
RightAnkle,
LeftHeel,
RightHeel,
LeftFootIndex,
RightFootIndex,
Hips,
Spine,
Neck,
Head
}

[Serializable]
public struct CharacterBody
{
    public GameObject nose;
    public GameObject leftEyeInner;
    public GameObject leftEye;
    public GameObject leftEyeOuter;
    public GameObject rightEyeInner;
    public GameObject rightEye;
    public GameObject rightEyeOuter;
    public GameObject leftEar;
    public GameObject rightEar;
    public GameObject leftMouth;
    public GameObject rightMouth;

    public GameObject hips;
    public GameObject leftShoulder;
    public GameObject rightShoulder;
    public GameObject leftElbow;
    public GameObject rightElbow;
    public GameObject leftWrist;
    public GameObject rightWrist;
    public GameObject leftHip;
    public GameObject rightHip;
    public GameObject leftKnee;
    public GameObject rightKnee;
    public GameObject leftAnkle;
    public GameObject rightAnkle;
    public GameObject leftHeel;
    public GameObject rightHeel;
}



public class Pose3DMapper : CharacterMapper
{

    
    [SerializeField] private FrameReader frameReader;
    
    

    [Tooltip("optional")] [SerializeField] private CharacterBody characterBodyIK;
    private Transform hips;
    [SerializeField] private bool IKEnable;
    [SerializeField] private bool normalMode;
    [SerializeField] private Transform characterPlacement;
    private Vector3 headUpVector;
    private Vector3 distanceOffset;
    private JointPoint[] jointPoints;
    private GameObject[] jointsDebug;


    protected override void InitializationHumanoidPose()
    {
        character.transform.rotation = Quaternion.identity;
        jointPoints = new JointPoint[37];
        for (var i = 0; i < jointPoints.Length; i++)
        {
            jointPoints[i] = new JointPoint();
            jointPoints[i].LastPoses = new Vector3[lowPassFilterChannels];

        }
        
        if (debugMode)
        {
            jointsDebug = new GameObject[33];
            for (int i = 0; i < jointsDebug.Length; i++)
            {
                jointsDebug[i] = Instantiate(debugGameObject);
            }
        }
        

        // Right Arm
        jointPoints[(int) BodyPoints.RightShoulder].Transform = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
        jointPoints[(int) BodyPoints.RightElbow].Transform = anim.GetBoneTransform(HumanBodyBones.RightLowerArm);
        jointPoints[(int) BodyPoints.RightWrist].Transform = anim.GetBoneTransform(HumanBodyBones.RightHand);
        // Left Arm
        jointPoints[(int) BodyPoints.LeftShoulder].Transform = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        jointPoints[(int) BodyPoints.LeftElbow].Transform = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        jointPoints[(int) BodyPoints.LeftWrist].Transform = anim.GetBoneTransform(HumanBodyBones.LeftHand);
        
        // Right Leg
        jointPoints[(int) BodyPoints.RightHip].Transform = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        jointPoints[(int) BodyPoints.RightKnee].Transform = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        jointPoints[(int) BodyPoints.RightAnkle].Transform = anim.GetBoneTransform(HumanBodyBones.RightFoot);
        jointPoints[(int) BodyPoints.RightFootIndex].Transform = anim.GetBoneTransform(HumanBodyBones.RightToes);

        // Left Leg
        jointPoints[(int) BodyPoints.LeftHip].Transform = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        jointPoints[(int) BodyPoints.LeftKnee].Transform = anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        jointPoints[(int) BodyPoints.LeftAnkle].Transform = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        jointPoints[(int) BodyPoints.LeftFootIndex].Transform = anim.GetBoneTransform(HumanBodyBones.LeftToes);

        // etc
        //jointPoints[PositionIndex.abdomenUpper.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Spine);
        // jointPoints[(int) BodyPoints.Hips].Transform = hips.transform;
        jointPoints[(int) BodyPoints.Hips].Transform = anim.GetBoneTransform(HumanBodyBones.Hips);
        jointPoints[(int) BodyPoints.Head].Transform = anim.GetBoneTransform(HumanBodyBones.Head);
        jointPoints[(int) BodyPoints.Neck].Transform = anim.GetBoneTransform(HumanBodyBones.Neck);
        jointPoints[(int) BodyPoints.Spine].Transform = anim.GetBoneTransform(HumanBodyBones.Spine);
        

        // Child Settings
        // Right Arm
        jointPoints[(int) BodyPoints.RightShoulder].Child = jointPoints[(int) BodyPoints.RightElbow];
        jointPoints[(int) BodyPoints.RightElbow].Child = jointPoints[(int) BodyPoints.RightWrist];
        jointPoints[(int) BodyPoints.RightElbow].Parent = jointPoints[(int) BodyPoints.RightShoulder];

        // Left Arm
        jointPoints[(int) BodyPoints.LeftShoulder].Child = jointPoints[(int) BodyPoints.LeftElbow];
        jointPoints[(int) BodyPoints.LeftElbow].Child = jointPoints[(int) BodyPoints.LeftWrist];
        jointPoints[(int) BodyPoints.LeftElbow].Parent = jointPoints[(int) BodyPoints.LeftShoulder];

        // Fase

        
        
        // Right Leg
        jointPoints[(int) BodyPoints.RightHip].Child = jointPoints[(int) BodyPoints.RightKnee];
        jointPoints[(int) BodyPoints.RightKnee].Child = jointPoints[(int) BodyPoints.RightAnkle];
        jointPoints[(int) BodyPoints.RightAnkle].Child = jointPoints[(int) BodyPoints.RightFootIndex];
        // jointPoints[(int) BodyPoints.RightKnee].Parent = jointPoints[(int) BodyPoints.RightHip];
        jointPoints[(int) BodyPoints.RightAnkle].Parent = jointPoints[(int) BodyPoints.RightKnee];

        // Left Leg
        jointPoints[(int) BodyPoints.LeftHip].Child = jointPoints[(int) BodyPoints.LeftKnee];
        jointPoints[(int) BodyPoints.LeftKnee].Child = jointPoints[(int) BodyPoints.LeftAnkle];
        jointPoints[(int) BodyPoints.LeftAnkle].Child = jointPoints[(int) BodyPoints.LeftFootIndex];
        // jointPoints[(int) BodyPoints.LeftKnee].Parent = jointPoints[(int) BodyPoints.LeftHip];
        jointPoints[(int) BodyPoints.LeftAnkle].Parent = jointPoints[(int) BodyPoints.LeftKnee];

        // etc
        // jointPoints[(int) BodyPoints.Spine].Child = jointPoints[(int) BodyPoints.Neck];
        // jointPoints[(int) BodyPoints.Neck].Child = jointPoints[(int) BodyPoints.Head];
        // jointPoints[(int) BodyPoints.Head].Child = jointPoints[(int) BodyPoints.Nose];

        
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
        }
        

        // Set Inverse
        Vector3 a = jointPoints[(int) BodyPoints.LeftHip].Transform.position;
        Vector3 b = jointPoints[(int) BodyPoints.Spine].Transform.position;
        hips = jointPoints[(int) BodyPoints.Hips].Transform;
        Vector3 c = jointPoints[(int) BodyPoints.RightHip].Transform.position;
        var forward = b.TriangleNormal(a,c);
        
        
        
        foreach (var jointPoint in jointPoints)
        {
            if (jointPoint.Transform != null)
            {
                jointPoint.InitRotation = jointPoint.Transform.rotation;
            }

            if (jointPoint.Child != null)
            {
                jointPoint.Inverse = Quaternion.Inverse(Quaternion.LookRotation(jointPoint.Transform.position - jointPoint.Child.Transform.position, forward));
                jointPoint.InverseRotation = jointPoint.Inverse * jointPoint.InitRotation;
            }
        }
        //Hip and Spine
        var hip = jointPoints[(int) BodyPoints.Hips];
        var spine = jointPoints[(int) BodyPoints.Spine];
        hip.Inverse = Quaternion.Inverse(Quaternion.LookRotation(forward,spine.Transform.position-hip.Transform.position));
        hip.InverseRotation = hip.Inverse * hip.InitRotation;

        if (spine.Transform != null)
        {
            spine.Inverse = Quaternion.Inverse(Quaternion.LookRotation(
                spine.Transform.position.TriangleNormal(jointPoints[(int) BodyPoints.RightShoulder].Transform.position,
                    jointPoints[(int) BodyPoints.LeftShoulder].Transform.position),
                jointPoints[(int) BodyPoints.Neck].Transform.position - spine.Transform.position));
            spine.InverseRotation = spine.Inverse * spine.InitRotation;
        }

        // For Head Rotation
        var head = jointPoints[(int) BodyPoints.Head];
        head.InitRotation = jointPoints[(int) BodyPoints.Head].Transform.rotation;
        var gaze = head.Transform.up;
        Debug.Log(gaze);
        head.Inverse = Quaternion.Inverse(Quaternion.LookRotation(gaze));
       // head.InverseRotation = head.Inverse * head.InitRotation; //TODO check why?
       
        head.InverseRotation = head.InitRotation;
        headUpVector = head.Transform.up;
        
        
        //feet setup
        var r_feet = jointPoints[(int) BodyPoints.RightAnkle];
        
        r_feet.Inverse = Quaternion.Inverse(Quaternion.LookRotation(r_feet.Transform.position - jointPoints[(int) BodyPoints.RightFootIndex].Transform.position, jointPoints[(int) BodyPoints.RightKnee].Transform.position - r_feet.Transform.position));
        r_feet.InverseRotation = r_feet.Inverse * r_feet.InitRotation;
        
        var l_feet = jointPoints[(int) BodyPoints.LeftAnkle];
        l_feet.Inverse = Quaternion.Inverse(Quaternion.LookRotation(l_feet.Transform.position - jointPoints[(int) BodyPoints.LeftFootIndex].Transform.position, jointPoints[(int) BodyPoints.LeftKnee].Transform.position - l_feet.Transform.position));
        l_feet.InverseRotation = l_feet.Inverse * l_feet.InitRotation;
        
        //
        // var lHand = jointPoints[PositionIndex.lHand.Int()];
        // var lf = TriangleNormal(lHand.Pos3D, jointPoints[PositionIndex.lMid1.Int()].Pos3D, jointPoints[PositionIndex.lThumb2.Int()].Pos3D);
        // lHand.InitRotation = lHand.Transform.rotation;
        // lHand.Inverse = Quaternion.Inverse(Quaternion.LookRotation(jointPoints[PositionIndex.lThumb2.Int()].Transform.position - jointPoints[PositionIndex.lMid1.Int()].Transform.position, lf));
        // lHand.InverseRotation = lHand.Inverse * lHand.InitRotation;
        //
        // var rHand = jointPoints[PositionIndex.rHand.Int()];
        // var rf = TriangleNormal(rHand.Pos3D, jointPoints[PositionIndex.rThumb2.Int()].Pos3D, jointPoints[PositionIndex.rMid1.Int()].Pos3D);
        // rHand.InitRotation = jointPoints[PositionIndex.rHand.Int()].Transform.rotation;
        // rHand.Inverse = Quaternion.Inverse(Quaternion.LookRotation(jointPoints[PositionIndex.rThumb2.Int()].Transform.position - jointPoints[PositionIndex.rMid1.Int()].Transform.position, rf));
        // rHand.InverseRotation = rHand.Inverse * rHand.InitRotation;
        
        // distanceOffset = character.transform.position

        for (int i = 0; i < jointPoints.Length; i++)
        {
            if(jointPoints[i].Transform != null)
                jointPoints[i].LandmarkPose = jointPoints[i].Transform.position;
        }
        
        Debug.Log("wtf");
        character.transform.rotation = characterPlacement.rotation;
        hips.position = characterPlacement.position;

    }
    
    


    private void UpdateNormalMode(BodyPartVector[] bodyPartVectors)
    {

        for (int i = 0; i < bodyPartVectors.Length; i++)
        {
            if(bodyPartVectors[i].visibility > 0.2f)
                jointPoints[i].LandmarkPose = bodyPartVectors[i].position;
        }
        
        //setting position of each bone
        // jointPoints[(int) BodyPoints.Spine].Transform.position = bodyPartVectors[(int) BodyPoints.Spine].position;
        jointPoints[(int) BodyPoints.Hips].Transform.position = bodyPartVectors[(int) BodyPoints.Hips].position;

        for (int i = 0; i < jointPoints.Length && i < bodyPartVectors.Length; i++)
        {
            JointPoint bone = jointPoints[i];

            if (bone.Transform != null)
                bone.WorldPos = bone.Transform.position;
        }



        for (int i = 0; i < jointPoints.Length && i < bodyPartVectors.Length; i++)
        {
            JointPoint bone = jointPoints[i];
            
            if (bone.Child != null)
            {
                if (bone.Child.Transform != null) 
                {
                    JointPoint child = bone.Child;
                    float distance = bone.DistanceFromChild;
                    Vector3 direction = (-bone.LandmarkPose + child.LandmarkPose) / (-bone.LandmarkPose + child.LandmarkPose).magnitude;
                    child.WorldPos = bone.Transform.position + direction * distance;
                    // child.Transform.position = child.WorldPos;
//                    Debug.Log(distance + "  " + Vector3.Distance(child.Transform.position,bone.Transform.position));
                }
            }
            else
            {
                // if(i == (int) BodyPoints.RightShoulder || i == (int) BodyPoints.LeftShoulder || 
                //    i == (int) BodyPoints.LeftHip || i== (int) BodyPoints.RightHip || i == (int) BodyPoints.Head || i== (int) BodyPoints.Neck)
                //     continue;
                // if (jointPoints[i].Transform != null)
                // {
                //     if(bodyPartVectors[i].visibility > 0.75f)
                //         jointPoints[i].Transform.position = bodyPartVectors[i].position;
                // }
            }
        }


        if(enableKalmanFilter)
            for (int i = 0; i < jointPoints.Length; i++)
            {
                if(jointPoints[i].Transform != null)
                    KalmanUpdate(jointPoints[i]);
            }
        else
        {
            for (int i = 0; i < jointPoints.Length; i++)
            {
                    jointPoints[i].FilteredPos = jointPoints[i].WorldPos;
            }
        }

        if (useLowPassFilter)
        {
            foreach (var jp in jointPoints)
            {
                jp.LastPoses[0] = jp.FilteredPos;
                for (var i = 1; i < jp.LastPoses.Length; i++)
                {
                    jp.LastPoses[i] = jp.LastPoses[i] * lowPassParam + jp.LastPoses[i - 1] * (1f - lowPassParam);
                }
                jp.FilteredPos = jp.LastPoses[jp.LastPoses.Length - 1];
            }
        }


        //setting hip & spine rotation
        Vector3 a = bodyPartVectors[(int) BodyPoints.RightHip].position;
        Vector3 spine = bodyPartVectors[(int) BodyPoints.Spine].position;
        Vector3 hip = bodyPartVectors[(int) BodyPoints.Hips].position;
        Vector3 c = bodyPartVectors[(int) BodyPoints.LeftHip].position;
        Vector3 d = bodyPartVectors[(int) BodyPoints.RightShoulder].position;
        Vector3 e = bodyPartVectors[(int) BodyPoints.LeftShoulder].position;
        Vector3 hipsUpward = spine - hip;
        Vector3 spineUpward = bodyPartVectors[(int) BodyPoints.Neck].position - spine;
        jointPoints[(int) BodyPoints.Hips].Transform.rotation = Quaternion.LookRotation(spine.TriangleNormal(c, a),
                                                                   hipsUpward ) *
                                                                jointPoints[(int) BodyPoints.Hips].InverseRotation;

        jointPoints[(int) BodyPoints.Spine].Transform.rotation = Quaternion.LookRotation(spine.TriangleNormal(d, e),
                                                                     spineUpward ) *
                                                                jointPoints[(int) BodyPoints.Spine].InverseRotation;

        
        // Head Rotation
        Vector3 mouth = (bodyPartVectors[(int) BodyPoints.LeftMouth].position +
                         bodyPartVectors[(int) BodyPoints.RightMouth].position)/2.0f;
        Vector3 lEye = bodyPartVectors[(int) BodyPoints.LeftEye].position;
        Vector3 rEye = bodyPartVectors[(int) BodyPoints.RightEye].position;
                
        var gaze = lEye.TriangleNormal(mouth, rEye);
        
        Vector3 nose = bodyPartVectors[(int) BodyPoints.Nose].position;
        Vector3 rEar = bodyPartVectors[(int) BodyPoints.RightEar].position;
        Vector3 lEar = bodyPartVectors[(int) BodyPoints.LeftEar].position;
        var head = jointPoints[(int) BodyPoints.Head];
        Vector3 normal = nose.TriangleNormal(rEar, lEar);
        head.Transform.rotation = Quaternion.LookRotation(gaze, normal) * head.InverseRotation;
        
        
        // rotate each of bones
        Vector3 forward = jointPoints[(int) BodyPoints.Hips].Transform.forward;
        
        Vector3 leftHip = jointPoints[(int) BodyPoints.LeftHip].FilteredPos;
        Vector3 rightHip = jointPoints[(int) BodyPoints.RightHip].FilteredPos;
        forward = jointPoints[(int) BodyPoints.Spine].FilteredPos.TriangleNormal(leftHip,rightHip);

        
        foreach (var jointPoint in jointPoints)
        {
            if(jointPoint == null)
                continue;
            
            if (jointPoint.Parent != null)
            {
                Vector3 fv = jointPoint.Parent.FilteredPos - jointPoint.FilteredPos;
                jointPoint.Transform.rotation = 
                    Quaternion.LookRotation(jointPoint.FilteredPos- jointPoint.Child.FilteredPos, fv) 
                    * jointPoint.InverseRotation;
            }
            else if (jointPoint.Child != null)
            {
                jointPoint.Transform.rotation = 
                    Quaternion.LookRotation((jointPoint.FilteredPos- jointPoint.Child.FilteredPos).normalized, forward)
                    * jointPoint.InverseRotation;
            }
            continue;
            
            if (jointPoint.Parent != null)
            {
                Vector3 fv = jointPoint.Parent.Transform.position - jointPoint.Transform.position;
                jointPoint.Transform.rotation = Quaternion.LookRotation(jointPoint.Transform.position- jointPoint.Child.Transform.position, fv) * jointPoint.InverseRotation;
            }
            else if (jointPoint.Child != null)
            {
                jointPoint.Transform.rotation = Quaternion.LookRotation(jointPoint.Transform.position- jointPoint.Child.Transform.position, forward) * jointPoint.InverseRotation;
            }
            continue;
            if (jointPoint.Parent != null)
            {
                var fv = jointPoint.Parent.Transform.position - jointPoint.Transform.position;
                jointPoint.Transform.rotation = Quaternion.LookRotation(jointPoint.Transform.position- jointPoint.Child.Transform.position, fv);
            }
            else if (jointPoint.Child != null)
            {
                jointPoint.Transform.rotation = Quaternion.LookRotation(jointPoint.Transform.position- jointPoint.Child.Transform.position, forward);
            }
        }

        
        //Calculate feet rotation
        Vector3 r_ankle = bodyPartVectors[(int) BodyPoints.RightAnkle].position;
        Vector3 r_toe = bodyPartVectors[(int) BodyPoints.RightFootIndex].position;
        Vector3 r_knee = bodyPartVectors[(int) BodyPoints.RightKnee].position;
        
        JointPoint r_ankleT = jointPoints[(int) BodyPoints.RightAnkle];
        r_ankleT.Transform.rotation = 
            Quaternion.LookRotation(r_ankle - r_toe, r_knee - r_ankle) 
            * r_ankleT.InverseRotation;
        
        Vector3 l_ankle = bodyPartVectors[(int) BodyPoints.LeftAnkle].position;
        Vector3 l_toe = bodyPartVectors[(int) BodyPoints.LeftFootIndex].position;
        Vector3 l_knee = bodyPartVectors[(int) BodyPoints.LeftKnee].position;
        
        JointPoint l_ankleT = jointPoints[(int) BodyPoints.LeftAnkle];
        l_ankleT.Transform.rotation = 
            Quaternion.LookRotation(l_ankle - l_toe, l_knee - l_ankle) 
            * l_ankleT.InverseRotation;


        // for (int i = 0; i < jointPoints.Length && i < bodyPartVectors.Length; i++)
        // {
        //     JointPoint bone = jointPoints[i];
        //
        //     if (bone.Child != null)
        //     {
        //         if (bone.Child.Transform != null)
        //         {
        //             JointPoint child = bone.Child;
        //             child.Transform.position = child.WorldPos;
        //         }
        //     }
        // }
        // Vector3 a1 = bodyPartVectors[(int) BodyPoints.Nose].position;
        // Vector3 b1 = bodyPartVectors[(int) BodyPoints.RightEar].position;
        // Vector3 c1 = bodyPartVectors[(int) BodyPoints.LeftHip].position;
        // float yDegree = Vector3.Angle(b1.TriangleNormal(a1, c1), Vector3.up);
        // var head = jointPoints[(int) BodyPoints.Head];
        // Vector3 headAngle = head.Transform.eulerAngles;
        // headAngle.y = yDegree;
        // head.Transform.eulerAngles = headAngle;
        
        // Vector3 a1 = bodyPartVectors[(int) BodyPoints.RightEye].position;
        // Vector3 b1 = bodyPartVectors[(int) BodyPoints.LeftEye].position;
        // Vector3 c1 = (bodyPartVectors[(int) BodyPoints.LeftMouth].position + bodyPartVectors[(int) BodyPoints.RightMouth].position);
        // float yDegree = Vector3.Angle(c1.TriangleNormal(a1, b1), Vector3.forward);
        // var head = jointPoints[(int) BodyPoints.Head];
        // Vector3 headAngle = head.Transform.eulerAngles;
        // headAngle.y = -yDegree;
        // head.Transform.eulerAngles = headAngle;
        //jointPoints[(int) BodyPoints.Hips].Transform.position = characterPlacement;
    }

    //placing and rotating bones with the help of IK algorithm
    private void UpdateModeIK(BodyPartVector[] bodyPartVectors)
    {
        
        //setting hips position
        characterBodyIK.hips.transform.position = bodyPartVectors[(int) BodyPoints.Hips].position;
        
        //setting hip rotation
        Vector3 a = bodyPartVectors[(int) BodyPoints.LeftShoulder].position;
        Vector3 b = jointPoints[(int) BodyPoints.Hips].Transform.position;
        Vector3 c = bodyPartVectors[(int) BodyPoints.RightShoulder].position;
        jointPoints[(int) BodyPoints.Hips].Transform.rotation = Quaternion.LookRotation(a.TriangleNormal(b, c)) *
                                                                jointPoints[(int) BodyPoints.Hips].InverseRotation;

        //IK
        characterBodyIK.leftElbow.transform.position = bodyPartVectors[(int) BodyPoints.LeftElbow].position;
        characterBodyIK.rightElbow.transform.position = bodyPartVectors[(int) BodyPoints.RightElbow].position;
        characterBodyIK.leftAnkle.transform.position = bodyPartVectors[(int) BodyPoints.LeftAnkle].position;
        characterBodyIK.rightAnkle.transform.position = bodyPartVectors[(int) BodyPoints.RightAnkle].position;
        characterBodyIK.leftWrist.transform.position = bodyPartVectors[(int) BodyPoints.LeftWrist].position;
        characterBodyIK.rightWrist.transform.position = bodyPartVectors[(int) BodyPoints.RightWrist].position;
        return;

        characterBodyIK.leftShoulder.transform.position = bodyPartVectors[(int) BodyPoints.LeftShoulder].position;
        characterBodyIK.rightShoulder.transform.position = bodyPartVectors[(int) BodyPoints.RightShoulder].position;

        characterBodyIK.leftHip.transform.position = bodyPartVectors[(int) BodyPoints.LeftHip].position;
        characterBodyIK.rightHip.transform.position = bodyPartVectors[(int) BodyPoints.RightHip].position;
        characterBodyIK.leftKnee.transform.position = bodyPartVectors[(int) BodyPoints.LeftKnee].position;
        characterBodyIK.rightKnee.transform.position = bodyPartVectors[(int) BodyPoints.RightKnee].position;
        characterBodyIK.leftHeel.transform.position = bodyPartVectors[(int) BodyPoints.LeftHeel].position;
        characterBodyIK.rightHeel.transform.position = bodyPartVectors[(int) BodyPoints.RightHeel].position;

        PoseJson poseJson = frameReader.currentPoseJson;
        BodyPart[] bodyParts = poseJson.predictions;
        characterBodyIK.leftShoulder.transform.position = new Vector3(-bodyParts[11].x,
            -bodyParts[11].y,bodyParts[11].z);
        
        characterBodyIK.rightShoulder.transform.position = new Vector3(-bodyParts[12].x,
            -bodyParts[12].y,bodyParts[12].z);
        
        characterBodyIK.leftElbow.transform.position = new Vector3(-bodyParts[13].x,
            -bodyParts[13].y,bodyParts[13].z);
        
        characterBodyIK.rightElbow.transform.position = new Vector3(-bodyParts[14].x,
            -bodyParts[14].y,bodyParts[14].z);
        
        characterBodyIK.leftWrist.transform.position = new Vector3(-bodyParts[15].x,
            -bodyParts[15].y,bodyParts[15].z);
        
        characterBodyIK.rightWrist.transform.position = new Vector3(-bodyParts[16].x,
            -bodyParts[16].y,bodyParts[16].z);
        
        characterBodyIK.leftHip.transform.position = new Vector3(-bodyParts[23].x,
            -bodyParts[23].y,bodyParts[23].z);
        
        characterBodyIK.rightHip.transform.position = new Vector3(-bodyParts[24].x,
            -bodyParts[24].y,bodyParts[24].z);

        characterBodyIK.hips.transform.position = (characterBodyIK.rightHip.transform.position +
                                                 characterBodyIK.leftHip.transform.position +
                                                 characterBodyIK.leftShoulder.transform.position +
                                                 characterBodyIK.rightShoulder.transform.position) / 4;
        
        
        characterBodyIK.leftKnee.transform.position = new Vector3(-bodyParts[25].x,
            -bodyParts[25].y,bodyParts[25].z);
        
        characterBodyIK.rightKnee.transform.position = new Vector3(-bodyParts[26].x,
            -bodyParts[26].y,bodyParts[26].z);
        
        characterBodyIK.leftHeel.transform.position = new Vector3(-bodyParts[27].x,
            -bodyParts[27].y,bodyParts[27].z);
        
        characterBodyIK.rightHeel.transform.position = new Vector3(-bodyParts[28].x,
            -bodyParts[28].y,bodyParts[28].z);
        
        characterBodyIK.leftAnkle.transform.position = new Vector3(-bodyParts[27].x,
            -bodyParts[27].y,bodyParts[27].z);
        
        characterBodyIK.rightAnkle.transform.position = new Vector3(-bodyParts[28].x,
            -bodyParts[28].y,bodyParts[28].z);
        
    }

    public override void Predict3DPose(PoseJsonVector poseJsonVector)
    {
        BodyPartVector[] bodyPartVectors = poseJsonVector.predictions;
        if(bodyPartVectors == null)
            return;
        if(bodyPartVectors.Length == 0)
            return;
        if (debugMode)
        {
            for (int i = 0; i < bodyPartVectors.Length; i++)
            {
                jointsDebug[i].transform.position = bodyPartVectors[i].position;
            }
        }
        try
        {
            character.transform.rotation = Quaternion.identity;
            if (normalMode)
            {
                UpdateNormalMode(bodyPartVectors);
            }
            else if (IKEnable)
            {
                UpdateModeIK(bodyPartVectors);
            }
            character.transform.rotation = characterPlacement.rotation;
            hips.position = characterPlacement.position;
        }
        catch (Exception e)
        {
            Debug.LogError("Pose problem");
            throw e;
        }
    }
}
