using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class UI_Manager : MonoBehaviour
{

    public static UI_Manager Instance;

    [Header("Grid Setup")]
    public Button[] gridButtons;   
    public GameObject chickenPrefab;  
    public GameObject duckPrefab;
    public GameObject gridButtonPrefab; 
    public Transform gridParent;

    [Header("Game Info")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI winningText;
    public Button restartButton;

    public GameObject chickenBG;
    public GameObject duckBG;
    public GameObject winPanel;
    

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
    }

    internal void CreateGridButtons()
    {
        gridButtons = new Button[9];

        for (int i = 0; i < 9; i++)
        {
            GameObject buttonObj = Instantiate(gridButtonPrefab, gridParent);
            Button btn = buttonObj.GetComponent<Button>();
            gridButtons[i] = btn;
        }
    }

    public void GoBackToMainMenu()
    {
        SceneManager.LoadScene("SampleScene");
    }
}


