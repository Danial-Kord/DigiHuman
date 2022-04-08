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
    

    
    [Header("Image")]
    [SerializeField] private Image imageSource;
    
    
    

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
        imageSource.sprite = LoadNewSprite(dirPath);
    }
    
    private Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f) {
   
        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference
        Texture2D SpriteTexture = LoadTexture(FilePath);
        Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height),new Vector2(0,0), PixelsPerUnit);
 
        return NewSprite;
    }
 
    private Texture2D LoadTexture(string FilePath) {
 
        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails
 
        Texture2D Tex2D;
        byte[] FileData;
 
        if (File.Exists(FilePath)){
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                 // If data = readable -> return texture
        }  
        return null;                     // Return null if load failed
    }
    
}
