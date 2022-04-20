using UnityEngine;

public class JointPoint
{
    //Landmark data
    public Vector3 LandmarkPose = new Vector3();
    
    // Bones
    public Transform Transform = null;
    public Quaternion InitRotation;
    public Quaternion Inverse;
    public Quaternion InverseRotation;
    public Vector3 InitialRotation;
    
    public JointPoint Child = null;
    public JointPoint Parent = null;

    public float DistanceFromChild;
    public float DistanceFromDad;
    
    
    // For Kalman filter
    public Vector3 P = new Vector3();
    public Vector3 X = new Vector3();
    public Vector3 K = new Vector3();
}

public abstract class CharacterMapper : MonoBehaviour
{
    [Header("Character")]
    [SerializeField] protected GameObject character;
    [Header("Debug")] 
    [SerializeField] protected bool debugMode;
    [SerializeField] protected GameObject debugGameObject;
    protected GameObject[] jointsDebug;
    
    
    protected Animator anim;
    protected abstract void InitializationHumanoidPose();
    public abstract void Predict3DPose(PoseJsonVector poseJsonVector);
    private void Awake()
    {
        anim = character.GetComponent<Animator>();
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
    
}
