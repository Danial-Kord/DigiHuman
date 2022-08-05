using UnityEngine;

public class JointPoint
{
    //Landmark data
    public Vector3 LandmarkPose = new Vector3();
    public Vector3 WorldPos  = new Vector3();
    
    // Bones
    public Transform Transform = null;
    public Vector3 FilteredPos  = new Vector3();
    public Vector3[] LastPoses = new Vector3[6];
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


    [Header("Kalman Filter")] 
    [SerializeField] protected bool enableKalmanFilter;
    [SerializeField] protected float KalmanParamQ;
    [SerializeField] protected float KalmanParamR;


    [Header("Low Pass Filter")] 
    [SerializeField] protected bool useLowPassFilter;
    [SerializeField] protected float lowPassParam = 0.1f;
    [SerializeField] protected int lowPassFilterChannels = 24; 
    
    protected Animator anim;
    protected abstract void InitializationHumanoidPose();
    public abstract void Predict3DPose(PoseJsonVector poseJsonVector);
    private void Awake()
    {
        SetCharacter(character);
    }

    public void SetCharacter(GameObject newCharacter)
    {
        character = newCharacter;
        anim = character.GetComponentInChildren<Animator>();

        InitializationHumanoidPose();
    }

    
    
 protected void KalmanUpdate(JointPoint measurement)
    {
        //measurement.Pos3D = measurement.Now3D;
        //return;
        measurementUpdate(measurement);
        Vector3 newPos = new Vector3();
        newPos.x = measurement.X.x + (measurement.WorldPos.x - measurement.X.x) * measurement.K.x;
        newPos.y = measurement.X.y + (measurement.WorldPos.y - measurement.X.y) * measurement.K.y;
        newPos.z = measurement.X.z + (measurement.WorldPos.z - measurement.X.z) * measurement.K.z;
        measurement.FilteredPos = newPos;
        measurement.X = newPos;
    }

	protected void measurementUpdate(JointPoint measurement)
    {
        measurement.K.x = (measurement.P.x + KalmanParamQ) / (measurement.P.x + KalmanParamQ + KalmanParamR);
        measurement.K.y = (measurement.P.y + KalmanParamQ) / (measurement.P.y + KalmanParamQ + KalmanParamR);
        measurement.K.z = (measurement.P.z + KalmanParamQ) / (measurement.P.z + KalmanParamQ + KalmanParamR);
        measurement.P.x = KalmanParamR * (measurement.P.x + KalmanParamQ) / (KalmanParamR + measurement.P.x + KalmanParamQ);
        measurement.P.y = KalmanParamR * (measurement.P.y + KalmanParamQ) / (KalmanParamR + measurement.P.y + KalmanParamQ);
        measurement.P.z = KalmanParamR * (measurement.P.z + KalmanParamQ) / (KalmanParamR + measurement.P.z + KalmanParamQ);
    }

}
