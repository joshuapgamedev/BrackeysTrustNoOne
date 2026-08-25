using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject narratorBox;
    [SerializeField] private TypewriterEffect narratorText;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
            
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame)
        {
            if(narratorBox.activeSelf)
            {
                narratorText.SkipToEnd();
            }
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartNarrator("Good!");
        }

       
    }

    public void StartNarrator(string text)
    {
        narratorBox.SetActive(true);
        narratorText.ShowText(text);
    }
}
