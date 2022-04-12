using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using FFmpegOut;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine.XR.WSA.Input;

public class VideoCapture : MonoBehaviour
{
    private static bool hasWebCam = false;
    public static WebCamDevice webCamDevice;
    [SerializeField] private SpriteRenderer renderer;
    [SerializeField] private Camera recorderCamera;
    private CameraInput cameraInput;
    private CameraCapture cameraCapture;
    private void Start()
    {
        CheckWebcamAvailable();
       // cameraCapture = GetComponent<CameraCapture>();
        //cameraCapture.s
    }

    public bool CheckWebcamAvailable()
    {
        WebCamDevice[] webCamDevices = WebCamTexture.devices;
        if (webCamDevices.Length > 0)
        {
            webCamDevice = webCamDevices[0];
            hasWebCam = true;
            return true;
        }
        else
        {
            hasWebCam = false;
        }

        return false;
    }

    public void Record()
    {
        if(!hasWebCam)
            return;
        WebCamTexture webcam = new WebCamTexture("NameOfDevice");
        renderer.material.mainTexture = webcam;
        webcam.Play();
    }
}