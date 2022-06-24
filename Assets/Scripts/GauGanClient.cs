using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FreeDraw;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GauGanClient : MonoBehaviour
{

    [Header("Requirements")]
    [SerializeField] private Drawable drawable;
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private BackgroundImageManager backgroundImageManager;

    

    public static string lastImageGeneratedPath;

    public void SendAndReceiveSketch()
    {
        string dirPath = drawable.SaveTexturePng();
        networkManager.UploadImageGauGan(dirPath,SaveImage);
    }

    private void SaveImage(NetworkManager.UploadResponse response, byte[] bytes)
    {
        var dirPath = FileManager.GauGanOutputDir;
        dirPath = dirPath + "/R_" + Random.Range(0, 100000) + ".png";
        FileManager.SaveBinary(dirPath,bytes);
        lastImageGeneratedPath = dirPath;
        backgroundImageManager.AddNewImage(lastImageGeneratedPath);
        backgroundImageManager.ShowChoiceMenuCanvas();
    }

    private void DeleteImage()
    {
        
    }
}
