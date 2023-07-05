using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Load the game checking if game mode is single player or not
    public void LoadGame(bool isOnePlayerMode)
    {
        GamemodeController.gameMode = isOnePlayerMode;
        SceneManager.LoadScene("Gameplay");
    }

    public void ExitButton()
    {
        Application.Quit();
    }
}