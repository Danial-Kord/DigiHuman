using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GradientBackground : MonoBehaviour
{

    [SerializeField] private float minRGBValue;
    [SerializeField] private float maxRGBValue;
    [SerializeField] private float speed;
    [SerializeField] private Image image;

    private void Start()
    {
        StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        
        while (true)
        {
            if (!this.gameObject.activeSelf)
                yield return null;
            
        }
    }
}
