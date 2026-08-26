using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject settingMenu;
    // Start is called before the first frame update
    private bool isSettingOpen = false;
    
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
}
