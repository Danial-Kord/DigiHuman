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
    [SerializeField] private Camera camera;
    private void Start()
    {
        StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        float R = minRGBValue;
        float G = minRGBValue;
        float B = maxRGBValue;
        bool RChanging = true;
        bool GChanging = false;
        bool BChanging = false;
        int direction = -1;
        Color color = new Color(R, G, B);
        camera.backgroundColor = color;
        while (true)
        {

            if (RChanging)
            {
                if (direction == -1 && R < minRGBValue)
                {
                    R = minRGBValue;
                    RChanging = false;
                    direction = +1;
                    if (G == minRGBValue)
                    {
                        GChanging = true;
                    }
                    else
                        BChanging = true;
                }
                else if (direction == +1 && R > maxRGBValue)
                {
                    R = maxRGBValue;
                    RChanging = false;
                    direction = -1;
                    if (G == maxRGBValue)
                    {
                        GChanging = true;
                    }
                    else
                        BChanging = true;
                }
                else
                {
                    R += (Time.deltaTime * speed * direction);
                }
            }
            else if (GChanging)
            {
                if (direction == -1 && G < minRGBValue)
                {
                    G = minRGBValue;
                    GChanging = false;
                    direction = +1;
                    if (R == minRGBValue)
                    {
                        RChanging = true;
                    }
                    else
                        BChanging = true;
                }
                else if (direction == +1 && G > maxRGBValue)
                {
                    G = maxRGBValue;
                    GChanging = false;
                    direction = -1;
                    if (R == maxRGBValue)
                    {
                        RChanging = true;
                    }
                    else
                        BChanging = true;
                }
                else
                {
                    G += (Time.deltaTime * speed * direction);
                }
            }
            else
            {
                if (direction == -1 && B < minRGBValue)
                {
                    B = minRGBValue;
                    BChanging = false;
                    direction = +1;
                    if (R == minRGBValue)
                    {
                        RChanging = true;
                    }
                    else
                        GChanging = true;
                }
                else if (direction == +1 && B > maxRGBValue)
                {
                    B = maxRGBValue;
                    BChanging = false;
                    direction = -1;
                    if (R == maxRGBValue)
                    {
                        RChanging = true;
                    }
                    else
                        GChanging = true;
                }
                else
                {
                    B += (Time.deltaTime * speed * direction);
                }
            }
            
            color.r = R / 255.0f;
            color.g = G / 255.0f;
            color.b = B / 255.0f;
            camera.backgroundColor = color;
            yield return new WaitForEndOfFrame();
        }
    }
}
