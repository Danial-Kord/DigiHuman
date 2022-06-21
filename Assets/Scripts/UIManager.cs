using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public enum WaitingModeUI
{
    None, ProgressBar, Loading
}


public class UIManager : MonoSingleton<UIManager>
{
    
    [Header("Requirements")] 
    [SerializeField] private FrameReader frameReader;
    [SerializeField] private Canvas canvas;

    
    [Header("ProgressBars and Loading")] 
    [SerializeField] private Image progressBarImage;
    [SerializeField] private TextMeshProUGUI progressBarText;
    [SerializeField] private GameObject waitingUI; //full background color for progressbar and loading
    [SerializeField] private GameObject loading;
    [SerializeField] private GameObject progressBar;


    // [Header("Slides")] 
    // [SerializeField] private GameObject imageSlideShowPanel;
    // [SerializeField] private GameObject characterSlideShowPanel;

    
    

    [Header("SideBar panels")] 
    [SerializeField] private GameObject rightPanel;
    [SerializeField] private GameObject leftPanel;
    
    
    [Header("Animation panel")] 
    [SerializeField] private Button saveAnimationButton;
    [SerializeField] private GameObject animationPanel;
    [SerializeField] private TextMeshProUGUI animationNameText;


    [Header("Messages")] 
    [SerializeField] private GameObject errorPanel;
    [SerializeField] private GameObject successPanel;
    

    public void UpdateProgressBar(float percent)
    {
        Debug.Log(percent);
        progressBarImage.fillAmount = percent;
        progressBarText.text = (percent*100).ToString("0.0") + "%";
    }
    public void CheckAndEnableWaitingModeUI(WaitingModeUI waitingModeUI,bool enable)
    {
        if (enable)
        {
            if (!waitingModeUI.Equals(WaitingModeUI.None))
            {
                waitingUI.SetActive(true);
                if (waitingModeUI.Equals(WaitingModeUI.Loading))
                {
                    loading.SetActive(true);
                    progressBar.SetActive(false);
                }
                else if (waitingModeUI.Equals(WaitingModeUI.ProgressBar))
                {
                    loading.SetActive(false);
                    progressBar.SetActive(true);
                }
            }
        }
        else
        {
            waitingUI.SetActive(false);
        }
    }
    
    //Messages
    public void ShowSuccessMessage(string message)
    {
        successPanel.SetActive(true);
        successPanel.GetComponentInChildren<TextMeshProUGUI>().text = message;
    }
    
    public void ShowErrorMessage(string message)
    {
        errorPanel.SetActive(true);
        errorPanel.GetComponentInChildren<TextMeshProUGUI>().text = message;
    }
    
    
    
    //Animation Actions
    public void OnPlayAnimation()
    {
        frameReader.ArrangeDataFrames();
        frameReader.pause = false;
    }

    public void ShowAnimationSavePanel()
    {
        animationPanel.SetActive(true);
        saveAnimationButton.gameObject.SetActive(false);
    }
    public void OnSubmitAnimationSave()
    {

        string animationName = animationNameText.text;
        bool result = FileManager.SaveAnimation(animationName,frameReader.GetFrameData());
        animationNameText.text = "";
        if (result)
        {
            ShowSuccessMessage("Animation Saved Successfully!");
            animationPanel.SetActive(false);
            saveAnimationButton.gameObject.SetActive(true);
        }
        else
            ShowErrorMessage("Use another animation name!");
    }
    
    
    //Slides
    public void OnSlideShowEnter(GameObject panel)
    {
        panel.SetActive(true);
        canvas.gameObject.SetActive(false);
    }
    public void OnSlideShowExit(GameObject panel)
    {
        panel.SetActive(false);
        canvas.gameObject.SetActive(true);
    }
    
    
    
    
    //side panels
    public void SideBarPanelTrigger(Animator panel)
    {
        panel.SetBool("BarTrigger",!panel.GetBool("BarTrigger"));
    }
    public void SideBarButtonTrigger(Animator button)
    {
        button.SetTrigger("Rotate");
    }
}
