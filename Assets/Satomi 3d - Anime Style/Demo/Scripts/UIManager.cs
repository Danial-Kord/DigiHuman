using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Transform character;
    public Animator characterAnimator;
    public Text viewStatusText;
    private Quaternion characterFrontRotation;
    private Quaternion characterLeftRotation;
    private Quaternion characterRightRotation;
    private Quaternion characterBackRotation;

    private string frontText = "Front View";
    private string backText = "Back View";
    private string rightText = "Right View";
    private string leftText = "Left View";

    public float frontYRotationValue = -18f;
    public float backYRotationValue = 150f;
    public float rightYRotationValue = 60f;
    public float leftYRotationValue = -100f;


    private enum ViewState
    {
        frontView,
        LeftView,
        rightView,
        backView
    }

    private ViewState currentState;

    // Use this for initialization
    void Start()
    {
        currentState = ViewState.frontView;

        characterBackRotation = Quaternion.Euler(0, backYRotationValue, 0);
        characterFrontRotation = Quaternion.Euler(0, frontYRotationValue, 0);
        characterLeftRotation = Quaternion.Euler(0, leftYRotationValue, 0);
        characterRightRotation = Quaternion.Euler(0, rightYRotationValue, 0);

    }

    public void TriggerIdle()
    {
        characterAnimator.SetTrigger("idleTrigger");
    }

    public void TriggerWalk()
    {
        characterAnimator.SetTrigger("walkTrigger");
    }

    public void TriggerRun()
    {
        characterAnimator.SetTrigger("runTrigger");
    }

    public void TriggerBow()
    {
        characterAnimator.SetTrigger("bowTrigger");
    }

    public void LeftArrow()
    {
        if (currentState == ViewState.frontView)
        {
            currentState = ViewState.rightView;
            character.transform.rotation = characterRightRotation;
            viewStatusText.text = leftText;
        }
        else if (currentState == ViewState.rightView)
        {
            currentState = ViewState.backView;
            character.transform.rotation = characterBackRotation;
            viewStatusText.text = backText;
        }
        else if (currentState == ViewState.backView)
        {
            currentState = ViewState.LeftView;
            character.transform.rotation = characterLeftRotation;
            viewStatusText.text = rightText;
        }
        else if (currentState == ViewState.LeftView)
        {
            currentState = ViewState.frontView;
            character.transform.rotation = characterFrontRotation;
            viewStatusText.text = frontText;
        }
    }

    public void RightArrow()
    {
        if (currentState == ViewState.frontView)
        {
            currentState = ViewState.LeftView;
            character.transform.rotation = characterLeftRotation;
            viewStatusText.text = rightText;
        }
        else if (currentState == ViewState.LeftView)
        {
            currentState = ViewState.backView;
            character.transform.rotation = characterBackRotation;
            viewStatusText.text = backText;
        }
        else if (currentState == ViewState.backView)
        {
            currentState = ViewState.rightView;
            character.transform.rotation = characterRightRotation;
            viewStatusText.text = leftText;
        }
        else if (currentState == ViewState.rightView)
        {
            currentState = ViewState.frontView;
            character.transform.rotation = characterFrontRotation;
            viewStatusText.text = frontText;
        }
    }
}
