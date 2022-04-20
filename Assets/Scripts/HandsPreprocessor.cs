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
    protected override void InitializationHumanoidPose()
    {
        InitializeRightHand();
        InitializeLeftHand();
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

    public void Predict3DPose(HandJsonVector poseJsonVector)
    {
        
        //right hand
        BodyPartVector[] handR = poseJsonVector.handsR;
        for (int i = 0; i < handR.Length; i++)
        {
            jointsDebug[i].transform.position = handR[i].position;
        }
        for (int i = 0; i < handR.Length; i++)
        {
            rightHand[i].LandmarkPose = handR[i].position;
        }
        

        
        
        Vector3 finger1 = handR[(int) HandPoints.MiddleFingerFirst].position;
        Vector3 finger2 = handR[(int) HandPoints.IndexFingerFirst].position;
        Vector3 finger3 = handR[(int) HandPoints.RingFingerFirst].position;
        Vector3 finger4 = handR[(int) HandPoints.PinkyFirst].position;
        

        
        
        Vector3 a = handR[(int) HandPoints.ThumbSecond].position;
        Vector3 wrist = handR[(int) HandPoints.Wrist].position;
        Vector3 c = handR[(int) HandPoints.PinkyFirst].position;
        Vector3 forward = c.TriangleNormal(a,wrist);
        
        Vector3 upward = (finger1 + finger2 + finger3 + finger4) / 4.0f - wrist;
        upward.Normalize();
        
        ExtensionMethods.DrawNormal(rightHand[(int) HandPoints.Wrist].Transform.position,forward);
        
        Vector3 normalR = wrist.TriangleNormal(handR[(int) HandPoints.ThumbSecond].position,handR[(int) HandPoints.MiddleFingerFirst].position);
        rightHand[(int) HandPoints.Wrist].Transform.rotation = Quaternion.LookRotation(handR[(int) HandPoints.ThumbSecond].position - handR[(int) HandPoints.MiddleFingerFirst].position, normalR) * rightHand[(int) HandPoints.Wrist].InverseRotation;
        
        /*
        
        for (int i = 0; i < rootHandPoints.Length; i++)
        {

            JointPoint bone = rightHand[(int)rootHandPoints[i]];
            Debug.Log((int)rootHandPoints[i]);
            JointPoint parent = rightHand[(int)HandPoints.Wrist];
            float distance = bone.DistanceFromDad;
            Vector3 direction = (-parent.LandmarkPose + bone.LandmarkPose) / (bone.LandmarkPose - parent.LandmarkPose).magnitude;

            bone.Transform.position = parent.Transform.position + direction * distance;

        }
        
        //setting bone positions
        for (int i = 0; i < rightHand.Length; i++)
        {
            JointPoint bone = rightHand[i];
            if (bone.Child != null)
            {
                if (bone.Child.Transform != null)
                {
                    JointPoint child = bone.Child;
                    float distance = bone.DistanceFromChild;
                    Vector3 direction = (-bone.LandmarkPose + child.LandmarkPose) / (-bone.LandmarkPose + child.LandmarkPose).magnitude;
                    child.Transform.position = bone.Transform.position + direction * distance;
                    Debug.Log(distance + "  " + Vector3.Distance(child.Transform.position,bone.Transform.position));
                }
            }
        }
        
*/
        
        //rotation
        
        for (int i = 0; i < rightHand.Length; i++)
        {
            // if(i == (int) HandPoints.PinkyThird || i == (int) HandPoints.ThumbThird || 
            //    i == (int) HandPoints.IndexFingerThird || i == (int) HandPoints.RingFingerThird
            //    || i == (int) HandPoints.MiddleFingerThird)
            //     continue;
            JointPoint bone = rightHand[i];
            
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
            //     //forward = rightHand[(int) HandPoints.Wrist].Transform.position - bone.Transform.position;
            //     //bone.Transform.rotation = Quaternion.LookRotation(bone.LandmarkPose- bone.Child.LandmarkPose, (rightHand[(int) HandPoints.Wrist].LandmarkPose - bone.LandmarkPose)) * bone.InverseRotation;
            //     // bone.Transform.rotation = Quaternion.LookRotation(bone.LandmarkPose- bone.Child.LandmarkPose, (rightHand[(int) HandPoints.Wrist].LandmarkPose - bone.LandmarkPose)) * bone.InverseRotation;
            //     //bone.Transform.rotation = Quaternion.LookRotation(bone.LandmarkPose- bone.Child.LandmarkPose, upward) * bone.InverseRotation;
            // }
            
            if (bone.Parent != null)
            {
                print(bone.Parent.Transform.name);
                Vector3 fv = bone.Parent.LandmarkPose - bone.LandmarkPose;
                bone.Transform.rotation = Quaternion.LookRotation(bone.LandmarkPose- bone.Child.LandmarkPose, fv) * bone.InverseRotation;
            }
            else if (bone.Child != null)
            {
                //forward = rightHand[(int) HandPoints.Wrist].Transform.position - bone.Transform.position;
                bone.Transform.rotation = Quaternion.LookRotation(bone.LandmarkPose- bone.Child.LandmarkPose, (rightHand[(int) HandPoints.Wrist].LandmarkPose - bone.LandmarkPose)) * bone.InverseRotation;
                // bone.Transform.rotation = Quaternion.LookRotation(bone.LandmarkPose- bone.Child.LandmarkPose, (rightHand[(int) HandPoints.Wrist].LandmarkPose - bone.LandmarkPose)) * bone.InverseRotation;
                //bone.Transform.rotation = Quaternion.LookRotation(bone.LandmarkPose- bone.Child.LandmarkPose, upward) * bone.InverseRotation;
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
        Vector3 a = jointPoints[(int) HandPoints.ThumbSecond].Transform.position;
        Vector3 b = jointPoints[(int) HandPoints.Wrist].Transform.position;
        Vector3 c = jointPoints[(int) HandPoints.PinkyFirst].Transform.position;
        var forward = c.TriangleNormal(a,b);
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
                    jointPoint.Inverse = Quaternion.Inverse(Quaternion.LookRotation(jointPoint.Transform.position - jointPoint.Child.Transform.position, forward));
                    jointPoint.InverseRotation = jointPoint.Inverse * jointPoint.InitRotation;
                }
            }
        }
        
        //set inverse rotation wrist
        var wrist = jointPoints[(int)HandPoints.Wrist];
        wrist.InitRotation = wrist.Transform.rotation;
        var rf = wrist.Transform.position.TriangleNormal(jointPoints[(int) HandPoints.ThumbSecond].Transform.position,
            jointPoints[(int) HandPoints.MiddleFingerFirst].Transform.position);
        origin = wrist.Transform.position;
        Forward = rf;
        wrist.Inverse = Quaternion.Inverse(Quaternion.LookRotation(jointPoints[(int)HandPoints.ThumbSecond].Transform.position - jointPoints[(int)HandPoints.MiddleFingerFirst].Transform.position, rf));
        wrist.InverseRotation = wrist.Inverse * wrist.InitRotation;
        
    }

    private Vector3 origin = Vector3.forward;
    private Vector3 Forward = Vector3.forward;
    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(origin,Forward);
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
        for (var i = 0; i < rightHand.Length; i++) rightHand[i] = new JointPoint();
        
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

    }

    private void InitializeLeftHand()
    {
        // Right Hand
        leftHand = new JointPoint[21];
        for (var i = 0; i < leftHand.Length; i++) leftHand[i] = new JointPoint();
        
        //Wrist
        leftHand[(int) HandPoints.Wrist].Transform = anim.GetBoneTransform(HumanBodyBones.LeftHand);

        //thumb
        leftHand[(int) HandPoints.ThumbFirst].Transform = anim.GetBoneTransform(HumanBodyBones.LeftThumbProximal);
        leftHand[(int) HandPoints.ThumbSecond].Transform = anim.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
        leftHand[(int) HandPoints.ThumbThird].Transform = anim.GetBoneTransform(HumanBodyBones.LeftThumbDistal);
        //child and parent
        leftHand[(int) HandPoints.ThumbFirst].Child = leftHand[(int) HandPoints.ThumbSecond];
        leftHand[(int) HandPoints.ThumbSecond].Child = leftHand[(int) HandPoints.ThumbThird];
        leftHand[(int) HandPoints.ThumbSecond].Parent = leftHand[(int) HandPoints.ThumbFirst];
        
        //index
        leftHand[(int) HandPoints.IndexFingerFirst].Transform = anim.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
        leftHand[(int) HandPoints.IndexFingerSecond].Transform = anim.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate);
        leftHand[(int) HandPoints.IndexFingerThird].Transform = anim.GetBoneTransform(HumanBodyBones.LeftIndexDistal);
        //child and parent
        leftHand[(int) HandPoints.IndexFingerFirst].Child = leftHand[(int) HandPoints.IndexFingerSecond];
        leftHand[(int) HandPoints.IndexFingerSecond].Child = leftHand[(int) HandPoints.IndexFingerThird];
        leftHand[(int) HandPoints.IndexFingerSecond].Parent = leftHand[(int) HandPoints.IndexFingerFirst];
        
        //middle
        leftHand[(int) HandPoints.MiddleFingerFirst].Transform = anim.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
        leftHand[(int) HandPoints.MiddleFingerSecond].Transform = anim.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate);
        leftHand[(int) HandPoints.MiddleFingerThird].Transform = anim.GetBoneTransform(HumanBodyBones.LeftMiddleDistal);
        //child and parent
        leftHand[(int) HandPoints.MiddleFingerFirst].Child = leftHand[(int) HandPoints.MiddleFingerSecond];
        leftHand[(int) HandPoints.MiddleFingerSecond].Child = leftHand[(int) HandPoints.MiddleFingerThird];
        leftHand[(int) HandPoints.MiddleFingerSecond].Parent = leftHand[(int) HandPoints.MiddleFingerFirst];
        
        //ring
        leftHand[(int) HandPoints.RingFingerFirst].Transform = anim.GetBoneTransform(HumanBodyBones.LeftRingProximal);
        leftHand[(int) HandPoints.RingFingerSecond].Transform = anim.GetBoneTransform(HumanBodyBones.LeftRingIntermediate);
        leftHand[(int) HandPoints.RingFingerThird].Transform = anim.GetBoneTransform(HumanBodyBones.LeftRingDistal);
        //child and parent
        leftHand[(int) HandPoints.RingFingerFirst].Child = leftHand[(int) HandPoints.RingFingerSecond];
        leftHand[(int) HandPoints.RingFingerSecond].Child = leftHand[(int) HandPoints.RingFingerThird];
        leftHand[(int) HandPoints.RingFingerSecond].Parent = leftHand[(int) HandPoints.RingFingerFirst];
        
        //pinky
        leftHand[(int) HandPoints.PinkyFirst].Transform = anim.GetBoneTransform(HumanBodyBones.LeftLittleProximal);
        leftHand[(int) HandPoints.PinkySecond].Transform = anim.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate);
        leftHand[(int) HandPoints.PinkyThird].Transform = anim.GetBoneTransform(HumanBodyBones.LeftLittleDistal);
        //child and parent
        leftHand[(int) HandPoints.PinkyFirst].Child = leftHand[(int) HandPoints.PinkySecond];
        leftHand[(int) HandPoints.PinkySecond].Child = leftHand[(int) HandPoints.PinkyThird];
        leftHand[(int) HandPoints.PinkySecond].Parent = leftHand[(int) HandPoints.PinkyFirst];
    }
}
