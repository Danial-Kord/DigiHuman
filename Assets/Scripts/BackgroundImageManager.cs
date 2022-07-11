using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundImageManager : MonoBehaviour
{
    private List<Image> images;

    [Header("ImageSlideShow")] 
    [SerializeField] private GameObject imageNodePrefab;
    [SerializeField] private SlideShow slideShow;
    private static string dirPath;
    
    [Header("Camera Background")] 
    [SerializeField] private Image backgroundImage;


    
    [Header("Image")]
    [SerializeField] private GameObject resultImageCanvas;
    [SerializeField] private Image resultImage;
    private Sprite chosenGauGanSprite;


    private void Start()
    {
        slideShow.onSelection += SelectImage;
        dirPath = FileManager.GauGanOutputDir;
        string[] files = System.IO.Directory.GetFiles(dirPath);
        for (int i = 0; i < files.Length; i++)
        {
            if(files[i].EndsWith(".meta"))
                continue;
            AddNewImage(files[i]);
        }
    }


    private void SelectImage(int index,GameObject node)
    {
        SetBackgroundView(true,node.GetComponentInChildren<Image>().sprite);
    }
    
    private Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f) {
   
        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference
        Texture2D SpriteTexture = FileManager.LoadTexture(FilePath);
        Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height),new Vector2(0,0), PixelsPerUnit);
 
        return NewSprite;
    }



    public void SetLocalImageBackground()
    {
        string path = FileManager.OpenFileImageExplorer();
        Sprite sprite = LoadNewSprite(path);
        SetBackgroundView(true, sprite);
    }
    
    private void SetBackgroundView(bool show, Sprite chosenGauGanSprite)
    {
        if (show)
        {
            backgroundImage.sprite = chosenGauGanSprite;
            UIManager.Instancce.ShowSuccessMessage("Image background set!");
        }
        backgroundImage.gameObject.SetActive(show);
        
    }


    public void AddNewImage(string path)
    {
        GameObject newImageNode = Instantiate(imageNodePrefab);
        Sprite sprite = LoadNewSprite(path);
        newImageNode.GetComponentInChildren<Image>().sprite = sprite;
        slideShow.AddNode(newImageNode);
        resultImage.sprite = sprite;

    }
    
    public void ShowChoiceMenuCanvas()
    {
        resultImageCanvas.SetActive(true);
    }



    

    
    public void OnSetImage()
    {
        chosenGauGanSprite = resultImage.sprite;
        SetBackgroundView(true,chosenGauGanSprite);
    }

    public void OnRemoveImage()
    {
        resultImageCanvas.SetActive(false);
        FileManager.RemoveFile(GauGanClient.lastImageGeneratedPath);
    }
}
