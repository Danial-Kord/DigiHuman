using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{

    [Header("Server")]
    [SerializeField] private string serverURL; //server URL
    
    //for testing in engine only
#if UNITY_EDITOR
    [Header("Debug")] 
    [SerializeField] private bool enableDebug; //if this is true we are in debug mode!
    [SerializeField] private string filePath; //for testing system
#endif

    private void Start()
    {
        //for testing in engine only
#if UNITY_EDITOR
        if (enableDebug)
        {
            UploadFile(filePath,serverURL);
        } 
#endif
    }

    //starting coroutine for sending ASync to server
    public void UploadFile(string localFileName, string uploadURL)
    {
        
        StartCoroutine(UploadFileCo(localFileName, uploadURL));
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
        // version 1
        //postForm.AddBinaryData("theFile",localFile.bytes);
        // version 2
        postForm.AddBinaryData("file",localFile.bytes,localFileName,"text/plain");
        WWW upload = new WWW(uploadURL,postForm);        
        yield return upload;
        if (upload.error == null)
            Debug.Log("upload done :" + upload.text);
        else
            Debug.Log("Error during upload: " + upload.error);
    }

}
