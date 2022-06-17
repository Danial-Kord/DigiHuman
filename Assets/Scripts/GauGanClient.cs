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

    

    private string lastImageGeneratedPath;

    public void SendAndReceiveSketch()
    {
        string dirPath = drawable.SaveTexturePng();
        networkManager.UploadImageGauGan(dirPath,SaveImage);
    }

    private void SaveImage(string responce, byte[] bytes)
    {
        var dirPath = Application.dataPath + "/RenderOutput";
        if (!System.IO.Directory.Exists(dirPath))
        {
            System.IO.Directory.CreateDirectory(dirPath);
        }

        dirPath = dirPath + "/R_" + Random.Range(0, 100000) + ".png";
        System.IO.File.WriteAllBytes(dirPath, bytes);
        Debug.Log(bytes.Length / 1024 + "Kb was saved as: " + dirPath);
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        lastImageGeneratedPath = dirPath;
        backgroundImageManager.AddNewImage(lastImageGeneratedPath);
    }
}
