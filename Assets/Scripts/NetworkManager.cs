using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour
{

    public static NetworkManager instance;
    [Header("Server")]
    [SerializeField] private string serverUploadURL; //server URL
    [SerializeField] private string serverPoseEstimatorURL; //server URL

    [Header("Dependencies")] 
    [SerializeField] private FrameReader frameReader;
    //for testing in engine only
#if UNITY_EDITOR
    [Header("Debug")] 
    [SerializeField] private bool enableDebug; //if this is true we are in debug mode!
    [SerializeField] private string filePath; //for testing system
#endif

    private void Start()
    {
        instance = this;
        //for testing in engine only
#if UNITY_EDITOR
        if (enableDebug)
        {
            UploadAndEstimatePose(filePath);
        } 
#endif
    }


    
    [Serializable] 
    public struct PoseRequest
    {
        public string fileName;
        public int index;
    }

    
    
    //starting coroutine for sending ASync to server
    public void UploadImageGauGan(string localFileName )
    {
        StartCoroutine(Upload(localFileName, serverUploadURL,(responce) => { StartCoroutine(GetGauGanImage(responce)); })); //Get estimates }));
    }
    
    
    
    //starting coroutine for sending ASync to server
    private void UploadAndEstimatePose(string localFileName)
    {

        StartCoroutine(Upload(localFileName, serverUploadURL,(responce) => { StartCoroutine(GetPoseEstimates(responce)); })); //Get estimates }));
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
    IEnumerator Upload(string localFileName, string url, Action<string> onFinishedUpload) {

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
        yield return www.SendWebRequest();
        
        if (www.result != UnityWebRequest.Result.Success) {
            Debug.Log(www.error);
        }
        else {
            byte[] results = www.downloadHandler.data;
            using (var stream = new MemoryStream(results))
            using (var binaryStream = new BinaryReader(stream))
            {             
                Debug.Log(results.Length);
            }
            
            Debug.Log(www.downloadHandler.text);
            //sending response to the action method
            onFinishedUpload(www.downloadHandler.text);
            Debug.Log("Upload complete!");
        }
    }
    
    //getting estimates for video pose
    IEnumerator GetPoseEstimates(string poseVideoName)
    {
        PoseRequest poseRequest = new PoseRequest();
        poseRequest.index = 0;
        poseRequest.fileName = poseVideoName;

        List<PoseJson> poseJsons = new List<PoseJson>();
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
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            yield return null;
        }

        frameReader.SetPosesQueue(poseJsons);
        yield break;
    }
    
    //getting GauGan image from the server
    IEnumerator GetGauGanImage(string serverResponse)
    {
        Debug.Log(serverResponse);
        yield break;
    }
}
