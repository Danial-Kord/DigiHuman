using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Generic singleton class, inherit this class to have singleton 
public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{

    private static T instance;

    public static T Instancce
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        //assigning the instance pointer
        if (instance == null)
        {
            instance = (T) this;
        }
        else
        {
            Destroy(instance);
            instance = (T) this;
        }

        OnAwake();
    }

    protected virtual void OnAwake()
    {
        
    }
}
