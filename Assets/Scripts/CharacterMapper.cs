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
Hips
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
    public GameObject model;
    private JointPoint[] jointPoints;
    private void Start()
    {
     //   PoseJsonVector poseJsonVector = FrameReader.GetBodyPartsVector(FrameReader.jsonTest.text);
     //   BodyPartVector[] bodyPartVectors = poseJsonVector.predictions_world;
     //   Animator anim = model.GetComponent<Animator>();
    }

    private void Update()
    {
        PoseJsonVector poseJsonVector = FrameReader.GetBodyPartsVector(FrameReader.jsonTest.text);
        BodyPartVector[] bodyPartVectors = poseJsonVector.predictions_world;
        characterBody.hips.transform.position = (bodyPartVectors[(int) BodyPoints.RightHip].position +
                                                 bodyPartVectors[(int) BodyPoints.LeftHip].position +
                                                 bodyPartVectors[(int) BodyPoints.LeftShoulder].position +
                                                 bodyPartVectors[(int) BodyPoints.RightShoulder].position) / 4;
        characterBody.leftShoulder.transform.position = bodyPartVectors[(int) BodyPoints.LeftShoulder].position;
        characterBody.rightShoulder.transform.position = bodyPartVectors[(int) BodyPoints.RightShoulder].position;
        characterBody.leftElbow.transform.position = bodyPartVectors[(int) BodyPoints.LeftElbow].position;
        characterBody.rightElbow.transform.position = bodyPartVectors[(int) BodyPoints.RightElbow].position;
        characterBody.leftWrist.transform.position = bodyPartVectors[(int) BodyPoints.LeftWrist].position;
        characterBody.rightWrist.transform.position = bodyPartVectors[(int) BodyPoints.RightWrist].position;
        characterBody.leftHip.transform.position = bodyPartVectors[(int) BodyPoints.LeftHip].position;
        characterBody.rightHip.transform.position = bodyPartVectors[(int) BodyPoints.RightHip].position;
        characterBody.leftKnee.transform.position = bodyPartVectors[(int) BodyPoints.LeftKnee].position;
        characterBody.rightKnee.transform.position = bodyPartVectors[(int) BodyPoints.RightKnee].position;
        characterBody.leftHeel.transform.position = bodyPartVectors[(int) BodyPoints.LeftHeel].position;
        characterBody.rightHeel.transform.position = bodyPartVectors[(int) BodyPoints.RightHeel].position;
        characterBody.leftAnkle.transform.position = bodyPartVectors[(int) BodyPoints.LeftAnkle].position;
        characterBody.rightAnkle.transform.position = bodyPartVectors[(int) BodyPoints.RightAnkle].position;

        return;
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
