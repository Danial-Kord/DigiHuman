using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class AnimationChooser : MonoSingleton<AnimationChooser>
{
    [Header("AnimationSlideShow")] 
    [SerializeField] private GameObject animationNodePrefab;
    [SerializeField] private SlideShow slideShow;
    [SerializeField] private FrameReader frameReader;
    private static string dirPath;
    
    private void Start()
    {
        slideShow.onSelection += SelectAnimation;
        dirPath = FileManager.AnimationsDir;
        string[] files = System.IO.Directory.GetFiles(dirPath);
        for (int i = 0; i < files.Length; i++)
        {
            if(files[i].EndsWith(".meta"))
                continue;
            AddNewAnimation(Path.GetFileName(files[i]));
        }
    }
    
    public void AddNewAnimation(string name)
    {
        GameObject newAnimationNode = Instantiate(animationNodePrefab);
        newAnimationNode.GetComponentInChildren<TextMeshProUGUI>().text = name;
        slideShow.AddNode(newAnimationNode);
    }

    private void SelectAnimation(int index,GameObject node)
    {
        string animationName = node.GetComponentInChildren<TextMeshProUGUI>().text;
        FrameData[] frameData = FileManager.LoadAnimation(animationName);
        frameReader.LoadFrames(frameData);
        
    }
}
