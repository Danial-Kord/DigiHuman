using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileManager : MonoBehaviour
{

    public static string GauGanOutputDir = Application.dataPath + "/RenderOutput";
    public static string SketchDir = Application.dataPath + "/Sketch";
    public static string AnimationsDir = Application.dataPath + "/CharacterAnimation";

    private void Awake()
    {
        if (!System.IO.Directory.Exists(SketchDir))
        {
            System.IO.Directory.CreateDirectory(SketchDir);
        }
        if (!System.IO.Directory.Exists(GauGanOutputDir))
        {
            System.IO.Directory.CreateDirectory(GauGanOutputDir);
        }
    }

    public static void SaveBinary(string path,byte[] bytes)
    {
        System.IO.File.WriteAllBytes(path, bytes);
        Debug.Log(bytes.Length / 1024 + "Kb was saved as: " + path);
        #if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
        #endif
    }
}
