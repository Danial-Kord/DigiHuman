using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class NetworkManager : MonoSingleton<NetworkManager>
{



    [Header("Server")]
    [SerializeField] private string serverUploadURL;
    [SerializeField] private string serverFullPoseEstimatorURL;
    [SerializeField] private string serverFullPoseUploadURL;
    [SerializeField] private string serverPoseUploadURL;
    [SerializeField] private string serverPoseEstimatorURL;
    [SerializeField] private string serverHandUploadURL;
    [SerializeField] private string serverHandPoseEstimatorURL;

    [SerializeField] private string serverFaceUploadURL;
    [SerializeField] private string serverFaceMocapURL; 


    [Header("Dependencies")] 
    [SerializeField] private FrameReader frameReader;


    
    //for testing in engine only
#if UNITY_EDITOR
    [Header("Debug")] 
    [SerializeField] private bool enableDebug; //if this is true we are in debug mode!
    [SerializeField] private string filePath; //for testing system
#endif




    [Serializable] 
    public struct UploadResponse
    {
        public string file;
        public int totalFrames;
        public float aspectRatio;
    }
    
    
    [Serializable] 
    public struct PoseRequest
    {
        public string fileName;
        public int index;
    }

    private void Start()
    {
        if (enableDebug)
        {
            StartCoroutine(Upload(filePath, serverFullPoseUploadURL, (response, bytes) =>
            {
                StartCoroutine(GetFullBodyPoseEstimates(response,bytes));
            }));
        }
    }


    //starting coroutine for sending ASync to server
    public void UploadImageGauGan(string localFileName,Action<UploadResponse,byte[]> onFinished)
    {
        StartCoroutine(Upload(localFileName, serverUploadURL,onFinished)); //Get estimates }));
    }
    
    

    
    
    //starting coroutine for sending ASync to server

    public void UploadFaceMoacap(string localFileName, Action onSuccess=null)
    {
        StartCoroutine(Upload(localFileName, serverFaceUploadURL, (response, bytes) =>
        {
            StartCoroutine(GetFaceMocap(response,bytes));
            onSuccess?.Invoke();
        })); //Get estimates }));
    }
    
    
    //starting coroutine for sending ASync to server

    public void UploadAndEstimatePose(string localFileName, Action onSuccess=null)
    {

        StartCoroutine(Upload(localFileName, serverPoseUploadURL, (response, bytes) =>
        {
            StartCoroutine(GetPoseEstimates(response,bytes)); 
            onSuccess?.Invoke();
        })); //Get estimates }));
    }
    
    
    //starting coroutine for sending ASync to server
    public void UploadAndEstimateHandPose(string localFileName, Action onSuccess=null)
    {
        StartCoroutine(Upload(localFileName, serverHandUploadURL, (response, bytes) =>
        {
            StartCoroutine(GetHandPoseEstimates(response,bytes));
            onSuccess?.Invoke();
        })); //Get estimates }));
    }
    
    //starting coroutine for sending ASync to server
    public void UploadAndEstimateFullPose(string localFileName, Action onSuccess=null)
    {
        StartCoroutine(Upload(localFileName, serverFullPoseUploadURL, (response, bytes) =>
        {
            StartCoroutine(GetFullBodyPoseEstimates(response,bytes));
            onSuccess?.Invoke();
        })); //Get estimates }));
    }
    
    
    //Async file uploader
    IEnumerator UploadFileCo(string localFileName, string uploadURL)
    {
        WWW localFile = new WWW("file:///" + localFileName);
        yield return localFile;
        if (localFile.error == null)
            Debug.Log("Loaded file successfully");
        else
        {
            Debug.Log("Open file error: "+localFile.error);
            yield break; // stop the coroutine here
        }
        WWWForm postForm = new WWWForm();
        postForm.AddBinaryData("file",localFile.bytes,localFileName,"text/plain");
        WWW upload = new WWW(uploadURL,postForm);        
        yield return upload;
        if (upload.error == null)
        {
            while (upload.MoveNext())
            {
                Debug.Log("upload done :" + upload.text);
                yield return upload;
            }
            Debug.Log(upload.isDone);
                Debug.Log("upload done :" + upload.text);
                Debug.Log(upload.bytes.Length);

        }
        else
            Debug.Log("Error during upload: " + upload.error);
    }

    //Async file uploader method2
    IEnumerator Upload(string localFileName, string url, Action<UploadResponse,byte[]> onFinishedUpload) {

        WWW localFile = new WWW("file:///" + localFileName);
        yield return localFile;
        if (localFile.error == null)
            Debug.Log("Loaded file successfully");
        else
        {
            Debug.Log("Open file error: "+localFile.error);
            yield break; // stop the coroutine here
        }
        WWWForm postForm = new WWWForm();

        postForm.AddBinaryData("file",localFile.bytes,localFileName,"text/plain");

        UnityWebRequest www = UnityWebRequest.Post(url, postForm);
        
        UIManager.Instancce.CheckAndEnableWaitingModeUI(WaitingModeUI.Loading,true);
        yield return www.SendWebRequest();
        UIManager.Instancce.CheckAndEnableWaitingModeUI(WaitingModeUI.Loading,false);
        
        if (www.result != UnityWebRequest.Result.Success) {
            Debug.Log(www.error);
            UIManager.Instancce.ShowErrorMessage("Server Connection Failed!");
        }
        else {
            byte[] results = www.downloadHandler.data;
            using (var stream = new MemoryStream(results))
            using (var binaryStream = new BinaryReader(stream))
            {             
                Debug.Log(results.Length);
            }
            
            Debug.Log(www.downloadHandler.text);
            try
            {
                UploadResponse uploadResponse = JsonUtility.FromJson<UploadResponse>(www.downloadHandler.text);
                onFinishedUpload(uploadResponse,results);

            }
            catch (Exception e)
            {
                
                onFinishedUpload((new UploadResponse()),results);

                Console.WriteLine(e);
                throw;
            }
            //sending response to the action method
            Debug.Log("Upload complete!");
        }
    }



    //getting estimates for video facial mocap
    IEnumerator GetFaceMocap(UploadResponse reponse, byte[] bytes)
    {
        PoseRequest poseRequest = new PoseRequest();
        poseRequest.index = 0;
        poseRequest.fileName = reponse.file;

        float totalFrames = reponse.totalFrames;
        
        List<FaceJson> faceJsons = new List<FaceJson>();
        UIManager.Instancce.CheckAndEnableWaitingModeUI(WaitingModeUI.ProgressBar,true);
        UIManager.Instancce.UpdateProgressBar(0);

        while (true)
        {
            UnityWebRequest webRequest = new UnityWebRequest(serverFaceMocapURL, "POST");
            byte[] encodedPayload = new System.Text.UTF8Encoding().GetBytes(JsonUtility.ToJson(poseRequest));
            webRequest.uploadHandler = (UploadHandler) new UploadHandlerRaw(encodedPayload);
            webRequest.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("cache-control", "no-cache");

            yield return webRequest.SendWebRequest();
            try
            {
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(webRequest.error);
                }
                else
                {
                    if (webRequest.downloadHandler.text.Equals("Done"))
                        break;
                    // Debug.Log(webRequest.downloadHandler.text);
                    FaceJson receivedJson = JsonUtility.FromJson<FaceJson>(webRequest.downloadHandler.text);
                    faceJsons.Add(receivedJson);
                    // Debug.Log(JsonUtility.FromJson<PoseJson>(webRequest.downloadHandler.text).frame);
                    poseRequest.index += 1;
                    UIManager.Instancce.UpdateProgressBar(((float)receivedJson.frame)/totalFrames);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                UIManager.Instancce.CheckAndEnableWaitingModeUI(WaitingModeUI.ProgressBar,false);
                UIManager.Instancce.ShowErrorMessage("Error in downloading Facial Data!");
                throw;
            }

            yield return null;
        }
        UIManager.Instancce.UpdateProgressBar(1);
        yield return null;
        
        frameReader.SetFaceMocapList(faceJsons);
        UIManager.Instancce.OnFaceDataReceived();

        UIManager.Instancce.CheckAndEnableWaitingModeUI(WaitingModeUI.ProgressBar,false);

        yield break;
    }
    
    
 //getting estimates for video hand pose
    IEnumerator GetHandPoseEstimates(UploadResponse response, byte[] bytes)
    {
        PoseRequest poseRequest = new PoseRequest();
        poseRequest.index = 0;
        poseRequest.fileName = response.file;
        frameReader.SetVideoFractions(response.aspectRatio);

        float totalFrames = response.totalFrames;    
        
        List<HandJson> poseJsons = new List<HandJson>();
        UIManager.Instancce.CheckAndEnableWaitingModeUI(WaitingModeUI.ProgressBar,true);
        UIManager.Instancce.UpdateProgressBar(0);

        while (true)
        {
            UnityWebRequest webRequest = new UnityWebRequest(serverHandPoseEstimatorURL, "POST");
            byte[] encodedPayload = new System.Text.UTF8Encoding().GetBytes(JsonUtility.ToJson(poseRequest));
            webRequest.uploadHandler = (UploadHandler) new UploadHandlerRaw(encodedPayload);
            webRequest.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("cache-control", "no-cache");

            yield return webRequest.SendWebRequest();
            try
            {
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(webRequest.error);
                }
                else
                {
                    if (webRequest.downloadHandler.text.Equals("Done"))
                        break;
                    HandJson receivedJson = JsonUtility.FromJson<HandJson>(webRequest.downloadHandler.text);
                    poseJsons.Add(receivedJson);
                    Debug.Log(JsonUtility.FromJson<HandJson>(webRequest.downloadHandler.text).frame);
                    poseRequest.index += 1;
                    UIManager.Instancce.UpdateProgressBar(receivedJson.frame/totalFrames);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                UIManager.Instancce.CheckAndEnableWaitingModeUI(WaitingModeUI.ProgressBar,false);
                UIManager.Instancce.ShowErrorMessage("Error in downloading Pose Data!");
                throw;
            }

            yield return null;
        }
        UIManager.Instancce.UpdateProgressBar(1);
        yield return null;
        frameReader.SetHandPoseList(poseJsons);
        UIManager.Instancce.OnHandPoseDataReceived();

        UIManager.Instancce.CheckAndEnableWaitingModeUI(WaitingModeUI.ProgressBar,false);

        yield break;
    }
    
    
    
    
    //getting estimates for video pose
    IEnumerator GetPoseEstimates(UploadResponse response, byte[] bytes)
    {
        PoseRequest poseRequest = new PoseRequest();
        poseRequest.index = 0;
        poseRequest.fileName = response.file;
        frameReader.SetVideoFractions(response.aspectRatio);
        float totalFrames = response.totalFrames;    
        
        List<PoseJson> poseJsons = new List<PoseJson>();
        UIManager.Instancce.CheckAndEnableWaitingModeUI(WaitingModeUI.ProgressBar,true);
        UIManager.Instancce.UpdateProgressBar(0);

        while (true)
        {
            UnityWebRequest webRequest = new UnityWebRequest(serverPoseEstimatorURL, "POST");
            byte[] encodedPayload = new System.Text.UTF8Encoding().GetBytes(JsonUtility.ToJson(poseRequest));
            webRequest.uploadHandler = (UploadHandler) new UploadHandlerRaw(encodedPayload);
            webRequest.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("cache-control", "no-cache");

            yield return webRequest.SendWebRequest();
            try
            {
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(webRequest.error);
                }
                else
                {
                    if (webRequest.downloadHandler.text.Equals("Done"))
                        break;
                    PoseJson receivedJson = JsonUtility.FromJson<PoseJson>(webRequest.downloadHandler.text);
                    poseJsons.Add(receivedJson);
                    Debug.Log(JsonUtility.FromJson<PoseJson>(webRequest.downloadHandler.text).frame);
                    poseRequest.index += 1;
                    UIManager.Instancce.UpdateProgressBar(receivedJson.frame/totalFrames);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                UIManager.Instancce.CheckAndEnableWaitingModeUI(WaitingModeUI.ProgressBar,false);
                UIManager.Instancce.ShowErrorMessage("Error in downloading Pose Data!");
                throw;
            }

            yield return null;
        }
        UIManager.Instancce.UpdateProgressBar(1);
        yield return null;
        frameReader.SetPoseList(poseJsons);
        UIManager.Instancce.OnPoseDataReceived();

        UIManager.Instancce.CheckAndEnableWaitingModeUI(WaitingModeUI.ProgressBar,false);

        yield break;
    }
    
    
    //getting estimates for video body & hand poses
    IEnumerator GetFullBodyPoseEstimates(UploadResponse response, byte[] bytes)
    {
        PoseRequest poseRequest = new PoseRequest();
        poseRequest.index = 0;
        poseRequest.fileName = response.file;
        frameReader.SetVideoFractions(response.aspectRatio);

        float totalFrames = response.totalFrames;    
        
        List<HandJson> handJsons = new List<HandJson>();
        List<PoseJson> bodyJsons = new List<PoseJson>();
        UIManager.Instancce.CheckAndEnableWaitingModeUI(WaitingModeUI.ProgressBar,true);
        UIManager.Instancce.UpdateProgressBar(0);

        while (true)
        {
            UnityWebRequest webRequest = new UnityWebRequest(serverFullPoseEstimatorURL, "POST");
            byte[] encodedPayload = new System.Text.UTF8Encoding().GetBytes(JsonUtility.ToJson(poseRequest));
            webRequest.uploadHandler = (UploadHandler) new UploadHandlerRaw(encodedPayload);
            webRequest.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("cache-control", "no-cache");

            yield return webRequest.SendWebRequest();
            try
            {
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(webRequest.error);
                }
                else
                {
                    if (webRequest.downloadHandler.text.Equals("Done"))
                        break;
                    FullPoseJson receivedJson = JsonUtility.FromJson<FullPoseJson>(webRequest.downloadHandler.text);
                    bodyJsons.Add(receivedJson.bodyPose);
                    handJsons.Add(receivedJson.handsPose);
                    Debug.Log(JsonUtility.FromJson<HandJson>(webRequest.downloadHandler.text).frame);
                    poseRequest.index += 1;
                    UIManager.Instancce.UpdateProgressBar(receivedJson.frame/totalFrames);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                UIManager.Instancce.CheckAndEnableWaitingModeUI(WaitingModeUI.ProgressBar,false);
                UIManager.Instancce.ShowErrorMessage("Error in downloading Full Body Pose Data!");
                throw;
            }

            yield return null;
        }
        UIManager.Instancce.UpdateProgressBar(1);
        yield return null;
        frameReader.SetHandPoseList(handJsons);
        frameReader.SetPoseList(bodyJsons);
        UIManager.Instancce.OnFullPoseDataReceived();

        UIManager.Instancce.CheckAndEnableWaitingModeUI(WaitingModeUI.ProgressBar,false);

        yield break;
    }
    
    
    //getting GauGan image from the server
    IEnumerator GetGauGanImage(string serverResponse)
    {
        Debug.Log(serverResponse);
        yield break;
    }
}
