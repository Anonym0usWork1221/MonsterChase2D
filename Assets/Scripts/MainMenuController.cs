using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject difPanel;
    [SerializeField]
    private GameObject aboutPanel;

    public void PlayGame()
    {
        int selectedCharacter = int.Parse(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name);

        GameManager.instance.CharIndex = selectedCharacter;
        SceneManager.LoadScene("GamePlay");
    }


    public void difficultyOpen()
    {
        difPanel.SetActive(true);
    }

    public void difficultyClose()
    {
        difPanel.SetActive(false);
    }

    public void AboutOpen()
    {
        aboutPanel.SetActive(true);
    }  
    
    public void AboutClose()
    {
        aboutPanel.SetActive(false);
    }

    public void openGit()
    {
        Application.OpenURL("https://github.com/Anonym0usWork1221/MonsterChase2D");
    }
    // about and diologs

    //public void setDifficulty()
    //{
    //    EditorUtility.DisplayDialog("Are you looking for difficulty?",
    //         "Are you sure you want to change difficulty? "
    //         + "sorry we dont do that here.", "OK");
    //}
    //public void BeautifulAuthor()
    //{
    //    EditorUtility.DisplayDialog("Are you looking for handsome developer?",
    //         "Are you sure you want to see Abdul Moez? "
    //         + "ok then we dont do that here.", "OK");
    //}

}
