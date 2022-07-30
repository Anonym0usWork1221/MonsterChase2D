using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class UIController : MonoBehaviour
{
    public void RestartGame()
    {
        SceneManager.LoadScene("GamePlay");
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void HomeButton()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
