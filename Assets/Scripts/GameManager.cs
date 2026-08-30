using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject settingMenu;
    [SerializeField] private GameObject controlsMenu;
    //[SerializeField] private NarratorController narrator;
    [SerializeField] private ScriptController script;
    // Start is called before the first frame update
    private bool isSettingOpen = false;
    private bool firstTime = true;

    private void Awake()
    {
        Debug.Log("test");
        controlsMenu.SetActive(true);
        Time.timeScale = 0.0f;
        //script.StartScript(script.openingScript);
    }

    // Update is called once per frame
    void Update()
    {
        if(Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame)
        {
            ToggleSettingMenu();
        }
    }

    public void OnResumeButtonPressed()
    {
        ToggleSettingMenu();
    }

    private void ToggleSettingMenu()
    {
        isSettingOpen = !isSettingOpen;

        Time.timeScale = isSettingOpen ? 0.0f : 1.0f;
        settingMenu.SetActive(isSettingOpen);

        if (isSettingOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            PlayerController.CanMove = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            PlayerController.CanMove = true;
        }
    }

    public void OnQuitButtonPressed()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnControlsMenuClose()
    {
        if (firstTime)
        {
            Time.timeScale = 1.0f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            //narrator.StartScript(narrator.welcomeScript);
            script.StartScript(script.openingScript);
            //PlayerController.CanMove = true;
            firstTime = false;
            
        }
    }

}
