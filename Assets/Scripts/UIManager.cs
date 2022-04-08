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
    [Header("ProgressBars and Loading")] 
    [SerializeField] private Image progressBarImage;
    [SerializeField] private TextMeshProUGUI progressBarText;
    [SerializeField] private GameObject waitingUI; //full background color for progressbar and loading
    [SerializeField] private GameObject loading;
    [SerializeField] private GameObject progressBar;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UpdateProgressBar(float percent)
    {
        progressBarImage.fillAmount = percent;
        progressBarText.text = percent + "%";
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

}
