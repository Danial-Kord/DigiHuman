using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class FileManager : MonoBehaviour
{

    public static string GauGanOutputDir;
    public static string SketchDir;
    public static string AnimationsDir;
    public static string charactersDir;

    private void Awake()
    {
        GauGanOutputDir = Application.dataPath + "/RenderOutput";
        SketchDir = Application.dataPath + "/Sketch";
        AnimationsDir = Application.dataPath + "/CharacterAnimation";
        charactersDir = Application.dataPath + "/Characters";
        if (!Directory.Exists(SketchDir))
        {
            Directory.CreateDirectory(SketchDir);
        }
        if (!Directory.Exists(GauGanOutputDir))
        {
            Directory.CreateDirectory(GauGanOutputDir);
        }
        if (!Directory.Exists(AnimationsDir))
        {
            Directory.CreateDirectory(AnimationsDir);
        }
    }
    

    public static void SaveBinary(string path,byte[] bytes)
    {
        File.WriteAllBytes(path, bytes);
        Debug.Log(bytes.Length / 1024 + "Kb was saved as: " + path);
        #if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
        #endif
    }
    
    
    public static Texture2D LoadTexture(string FilePath) {
 
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

    [Serializable]
    public class AnimationData
    {
        public FrameData[] frameData;
    }
    public static bool SaveAnimation(string name, FrameData[] frameData)
    {
        string path = AnimationsDir + "/" + name;
        if (File.Exists(path))
            return false;
        
        AnimationData animationData = new AnimationData
        {
            frameData = frameData
        };
        string data = JsonUtility.ToJson(animationData); 
        Debug.Log(data);
        File.WriteAllText(path,data);
        return true;
    }
    
    public static FrameData[] LoadAnimation(string name)
    {
        string path = AnimationsDir + "/" + name;
        if (!File.Exists(path))
            return null;


        string data = File.ReadAllText(path);
        AnimationData animationData = JsonUtility.FromJson<AnimationData>(data);
        FrameData[] frameData = animationData.frameData;
        return frameData;
    }

    public static string OpenFileVideoExplorer()
    {
        return EditorUtility.OpenFilePanel("Hello", "","mp4");
    }
    public static string OpenFileImageExplorer()
    {
        return EditorUtility.OpenFilePanel("Hello", "","png");
    }

    public static void RemoveFile(string path)
    {
        File.Delete(path);
    }
    
}
