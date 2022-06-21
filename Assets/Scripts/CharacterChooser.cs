using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterChooser : MonoBehaviour
{

    [SerializeField] private FrameReader frameReader;
    [SerializeField] private SlideShow slideShow;
    private void Start()
    {
        slideShow.onSelection += OnCharacterSelect;
    }

    private void OnCharacterSelect(int index,GameObject node)
    {
        frameReader.SetNewCharacter(Instantiate(node));
    }
}
