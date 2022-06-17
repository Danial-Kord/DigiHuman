using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterChooser : MonoBehaviour
{

    [SerializeField] private Transform characterPlace;
    [SerializeField] private SlideShow slideShow;
    [SerializeField] private GameObject currentCharacter;
    private void Start()
    {
        slideShow.onSelection += OnCharacterSelect;
    }

    private void OnCharacterSelect(int index,GameObject node)
    {
        currentCharacter.SetActive(false);
        // currentCharacter = Instantiate(node);
    }
}
