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
Spine
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

public class JointPoint
{
    public Vector2 Pos2D = new Vector2();
    public float score2D;

    public Vector3 Pos3D = new Vector3();
    public Vector3 Now3D = new Vector3();
    public Vector3[] PrevPos3D = new Vector3[6];
    public float score3D;

    // Bones
    public Transform Transform = null;
    public Quaternion InitRotation;
    public Quaternion Inverse;
    public Quaternion InverseRotation;

    public JointPoint Child = null;
    public JointPoint Parent = null;

    // For Kalman filter
    public Vector3 P = new Vector3();
    public Vector3 X = new Vector3();
    public Vector3 K = new Vector3();
}

public static partial class EnumExtend
{
    public static int Int(this BodyPoints i)
    {
        return (int)i;
    }
}
public class CharacterMapper : MonoBehaviour
{

    
    [SerializeField] private CharacterBody characterBody;
    public FrameReader FrameReader;
    [SerializeField]private JointPoint[] jointPoints;
    [SerializeField] private bool IKEnable;
    [SerializeField] private bool normalMode;
    [Header("Character")]
    [SerializeField] private GameObject character;
    [SerializeField] private GameObject nose;
    [SerializeField] private GameObject hips;
    [Header("Debug")] 
    [SerializeField] private bool debugMode;
    
    [SerializeField] private GameObject debugGameObject;
    private GameObject[] jointsDebug;
    private Animator anim;

    private void InitializationHumanoidPose()
    {
       jointPoints = new JointPoint[35];
        for (var i = 0; i < jointPoints.Length; i++) jointPoints[i] = new JointPoint();

        anim = character.GetComponent<Animator>();

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
        //jointPoints[PositionIndex.head.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Head);
        //jointPoints[PositionIndex.neck.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Neck);
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
      jointPoints[(int) BodyPoints.RightAnkle].Parent = jointPoints[(int) BodyPoints.RightHip];

        // Left Leg
        jointPoints[(int) BodyPoints.LeftHip].Child = jointPoints[(int) BodyPoints.LeftKnee];
        jointPoints[(int) BodyPoints.LeftKnee].Child = jointPoints[(int) BodyPoints.LeftAnkle];
        jointPoints[(int) BodyPoints.LeftAnkle].Child = jointPoints[(int) BodyPoints.LeftFootIndex];
        jointPoints[(int) BodyPoints.LeftAnkle].Parent = jointPoints[(int) BodyPoints.LeftHip];

        // etc
     //   jointPoints[PositionIndex.spine.Int()].Child = jointPoints[PositionIndex.neck.Int()];
      //  jointPoints[PositionIndex.neck.Int()].Child = jointPoints[PositionIndex.head.Int()];
        //jointPoints[PositionIndex.head.Int()].Child = jointPoints[PositionIndex.Nose.Int()];


        // Set Inverse
        Vector3 a = jointPoints[(int) BodyPoints.LeftHip].Transform.position;
        Vector3 b = jointPoints[(int) BodyPoints.Hips].Transform.position;
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
        var hip = jointPoints[(int) BodyPoints.Hips];
        hip.Inverse = Quaternion.Inverse(Quaternion.LookRotation(forward));
        hip.InverseRotation = hip.Inverse * hip.InitRotation;

        // For Head Rotation
        // var head = jointPoints[PositionIndex.head.Int()];
        // head.InitRotation = jointPoints[PositionIndex.head.Int()].Transform.rotation;
        // var gaze = jointPoints[PositionIndex.Nose.Int()].Transform.position - jointPoints[PositionIndex.head.Int()].Transform.position;
        // head.Inverse = Quaternion.Inverse(Quaternion.LookRotation(gaze));
        // head.InverseRotation = head.Inverse * head.InitRotation;
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
    }
    
    
    private void Start()
    {
        if (debugMode)
        {
            jointsDebug = new GameObject[33];
            for (int i = 0; i < jointsDebug.Length; i++)
            {
                jointsDebug[i] = Instantiate(debugGameObject);
            }
        }
        InitializationHumanoidPose();
        
    }

    private void UpdateNormalMode(BodyPartVector[] bodyPartVectors)
    {


        // jointPoints[(int) BodyPoints.Hips].Transform.eulerAngles = new Vector3(0,-Vector3.Angle(Vector3.back,a.TriangleNormal(b,c)),0);
        jointPoints[(int) BodyPoints.Hips].Transform.position = (bodyPartVectors[(int) BodyPoints.RightHip].position +
                                                                 bodyPartVectors[(int) BodyPoints.LeftHip].position) / 2;
        Vector3 a = bodyPartVectors[(int) BodyPoints.LeftShoulder].position;
        Vector3 b = jointPoints[(int) BodyPoints.Hips].Transform.position;
        Vector3 c = bodyPartVectors[(int) BodyPoints.RightShoulder].position;
        jointPoints[(int) BodyPoints.Hips].Transform.rotation = Quaternion.LookRotation(a.TriangleNormal(b, c)) *
                                                                jointPoints[(int) BodyPoints.Hips].InverseRotation;

        jointPoints[(int) BodyPoints.Spine].Transform.position = (bodyPartVectors[(int) BodyPoints.RightHip].position +
                                                                 bodyPartVectors[(int) BodyPoints.LeftHip].position +
                                                                 bodyPartVectors[(int) BodyPoints.LeftShoulder].position +
                                                                 bodyPartVectors[(int) BodyPoints.RightShoulder].position) / 4;

        var forward = jointPoints[(int) BodyPoints.Hips].Transform.forward;
        for (int i = 0; i < jointPoints.Length && i < bodyPartVectors.Length; i++)
        {
            if(i == (int) BodyPoints.RightShoulder || i == (int) BodyPoints.LeftShoulder || 
               i == (int) BodyPoints.LeftHip || i== (int) BodyPoints.RightHip)
                continue;
            if(jointPoints[i].Transform != null)
                jointPoints[i].Transform.position = bodyPartVectors[i].position;
        }
        // rotate each of bones
        foreach (var jointPoint in jointPoints)
        {
            if(jointPoint == null)
                continue;
            if (jointPoint.Parent != null)
            {
                Debug.Log("fuk :/");
                var fv = jointPoint.Parent.Transform.position - jointPoint.Transform.position;
                jointPoint.Transform.rotation = Quaternion.LookRotation(jointPoint.Transform.position- jointPoint.Child.Transform.position, fv) * jointPoint.InverseRotation;
            }
            else if (jointPoint.Child != null)
            {
                Debug.Log("fuk :/");
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
    }
    private void Update()
    {
        PoseJsonVector poseJsonVector = FrameReader.poseJsonVector;
        BodyPartVector[] bodyPartVectors = poseJsonVector.predictions_world;
        if (debugMode)
        {
            for (int i = 0; i < bodyPartVectors.Length; i++)
            {
                jointsDebug[i].transform.position = bodyPartVectors[i].position;
            }
        }
        try
        {
            if (normalMode)
            {
                UpdateNormalMode(bodyPartVectors);
            }
            else if (IKEnable)
            {

                characterBody.hips.transform.position = (bodyPartVectors[(int) BodyPoints.RightHip].position +
                                                         bodyPartVectors[(int) BodyPoints.LeftHip].position +
                                                         bodyPartVectors[(int) BodyPoints.LeftShoulder].position +
                                                         bodyPartVectors[(int) BodyPoints.RightShoulder].position) / 4;
                Vector3 a = bodyPartVectors[(int) BodyPoints.LeftShoulder].position -
                            bodyPartVectors[(int) BodyPoints.RightShoulder].position;
                Vector3 b = bodyPartVectors[(int) BodyPoints.LeftShoulder].position -
                            bodyPartVectors[(int) BodyPoints.LeftHip].position;
                Debug.Log(Vector3.Cross(a, b));
                characterBody.hips.transform.eulerAngles =
                    new Vector3(0, -Vector3.Angle(Vector3.back, Vector3.Cross(a, b)), 0);

                characterBody.leftElbow.transform.position = bodyPartVectors[(int) BodyPoints.LeftElbow].position;
                characterBody.rightElbow.transform.position = bodyPartVectors[(int) BodyPoints.RightElbow].position;
                characterBody.leftAnkle.transform.position = bodyPartVectors[(int) BodyPoints.LeftAnkle].position;
                characterBody.rightAnkle.transform.position = bodyPartVectors[(int) BodyPoints.RightAnkle].position;
                characterBody.leftWrist.transform.position = bodyPartVectors[(int) BodyPoints.LeftWrist].position;
                characterBody.rightWrist.transform.position = bodyPartVectors[(int) BodyPoints.RightWrist].position;
            }
        }
        catch (Exception e)
        {
            Debug.Log("problem");
        }
        return;

        characterBody.leftShoulder.transform.position = bodyPartVectors[(int) BodyPoints.LeftShoulder].position;
        characterBody.rightShoulder.transform.position = bodyPartVectors[(int) BodyPoints.RightShoulder].position;

        characterBody.leftHip.transform.position = bodyPartVectors[(int) BodyPoints.LeftHip].position;
        characterBody.rightHip.transform.position = bodyPartVectors[(int) BodyPoints.RightHip].position;
        characterBody.leftKnee.transform.position = bodyPartVectors[(int) BodyPoints.LeftKnee].position;
        characterBody.rightKnee.transform.position = bodyPartVectors[(int) BodyPoints.RightKnee].position;
        characterBody.leftHeel.transform.position = bodyPartVectors[(int) BodyPoints.LeftHeel].position;
        characterBody.rightHeel.transform.position = bodyPartVectors[(int) BodyPoints.RightHeel].position;

        PoseJson poseJson = FrameReader.poseJson;
        BodyPart[] bodyParts = poseJson.predictions_world;
        characterBody.leftShoulder.transform.position = new Vector3(-bodyParts[11].x,
            -bodyParts[11].y,bodyParts[11].z);
        
        characterBody.rightShoulder.transform.position = new Vector3(-bodyParts[12].x,
            -bodyParts[12].y,bodyParts[12].z);
        
        characterBody.leftElbow.transform.position = new Vector3(-bodyParts[13].x,
            -bodyParts[13].y,bodyParts[13].z);
        
        characterBody.rightElbow.transform.position = new Vector3(-bodyParts[14].x,
            -bodyParts[14].y,bodyParts[14].z);
        
        characterBody.leftWrist.transform.position = new Vector3(-bodyParts[15].x,
            -bodyParts[15].y,bodyParts[15].z);
        
        characterBody.rightWrist.transform.position = new Vector3(-bodyParts[16].x,
            -bodyParts[16].y,bodyParts[16].z);
        
        characterBody.leftHip.transform.position = new Vector3(-bodyParts[23].x,
            -bodyParts[23].y,bodyParts[23].z);
        
        characterBody.rightHip.transform.position = new Vector3(-bodyParts[24].x,
            -bodyParts[24].y,bodyParts[24].z);

        characterBody.hips.transform.position = (characterBody.rightHip.transform.position +
                                                 characterBody.leftHip.transform.position +
                                                 characterBody.leftShoulder.transform.position +
                                                 characterBody.rightShoulder.transform.position) / 4;
        
        
        characterBody.leftKnee.transform.position = new Vector3(-bodyParts[25].x,
            -bodyParts[25].y,bodyParts[25].z);
        
        characterBody.rightKnee.transform.position = new Vector3(-bodyParts[26].x,
            -bodyParts[26].y,bodyParts[26].z);
        
        characterBody.leftHeel.transform.position = new Vector3(-bodyParts[27].x,
            -bodyParts[27].y,bodyParts[27].z);
        
        characterBody.rightHeel.transform.position = new Vector3(-bodyParts[28].x,
            -bodyParts[28].y,bodyParts[28].z);
        
        characterBody.leftAnkle.transform.position = new Vector3(-bodyParts[27].x,
            -bodyParts[27].y,bodyParts[27].z);
        
        characterBody.rightAnkle.transform.position = new Vector3(-bodyParts[28].x,
            -bodyParts[28].y,bodyParts[28].z);
        
    }
}
