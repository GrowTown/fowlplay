using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public bool isFirstPlayer = true;
    public GameObject waitingPanel;
    public GameObject EndPanel;
    public GameObject LoadingPanel;
    public GameObject LoadingPanelParent;
    public TileManager[] tileManagerList;
    public int turnCount = 0;
    public Sprite spriteX;
    public Sprite spriteO;
    public string textX;
    public string textO;
    public Sprite Tile_Empty;
    public Text Winner_Text;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }

    }

    public void AIGamePlayScene()
    {
        GameObject screen = Instantiate(LoadingPanel, LoadingPanelParent.transform);
        LoadingScreenController loader = screen.GetComponent<LoadingScreenController>();
        loader.StartLoading("AI_GamePlay");
        //SceneManager.LoadScene("AI_GamePlay");
        Audio_Manager.Instance.PlayMusic(Audio_Manager.Instance.buttonclick, Audio_Manager.Instance.sfxVolume);
    }

    public void MultiPlayerGameScene()
    {
        //SceneManager.LoadScene("Lobby");
        GameObject screen = Instantiate(LoadingPanel, LoadingPanelParent.transform);
        LoadingScreenController loader = screen.GetComponent<LoadingScreenController>();
        loader.StartLoading("Lobby");
        Audio_Manager.Instance.PlayMusic(Audio_Manager.Instance.buttonclick, Audio_Manager.Instance.sfxVolume);
    }

    public void ExitGame()
    {
        Application.Quit();
        Audio_Manager.Instance.PlayMusic(Audio_Manager.Instance.buttonclick, Audio_Manager.Instance.sfxVolume);
        Debug.Log("Game is exiting...");
    }


    public void EndTurn(bool isNotMyTurn = false)
    {
        turnCount++;
        if (CheckWinCondition())
        {
            Debug.Log("Game Over. Player " + (isFirstPlayer ? "1" : "2") + " wins!");
            if (isNotMyTurn)
            {
                Winner_Text.text = isFirstPlayer ? "Player  2  Won" : "Player  1  Won";
            }
            else
            {
                Winner_Text.text = isFirstPlayer ? "Player  1  Won" : "Player  2  Won";
            }

            EndPanel.SetActive(true);

        }
        else if (turnCount >= tileManagerList.Length)
        {
            Debug.Log("Game Over. It's a draw!");
            Winner_Text.text = "DRAW";
            EndPanel.SetActive(true);
        }
    }

    public string GetPlayersTurn(bool isPlayer1)
    {
        return isPlayer1 ? textX : textO;
    }

    private bool CheckWinCondition()
    {
        // Check Rows
        for (int i = 0; i < 3; i++)
        {
            if (tileManagerList[i * 3].Internal_Text.text == tileManagerList[i * 3 + 1].Internal_Text.text &&
                tileManagerList[i * 3 + 1].Internal_Text.text == tileManagerList[i * 3 + 2].Internal_Text.text &&
                !string.IsNullOrEmpty(tileManagerList[i * 3].Internal_Text.text))
            {
                return true;
            }
        }

        // Check Columns
        for (int i = 0; i < 3; i++)
        {
            if (tileManagerList[i].Internal_Text.text == tileManagerList[i + 3].Internal_Text.text &&
                tileManagerList[i + 3].Internal_Text.text == tileManagerList[i + 6].Internal_Text.text &&
                !string.IsNullOrEmpty(tileManagerList[i].Internal_Text.text))
            {
                return true;
            }
        }

        // Check Diagonals
        if (tileManagerList[0].Internal_Text.text == tileManagerList[4].Internal_Text.text &&
            tileManagerList[4].Internal_Text.text == tileManagerList[8].Internal_Text.text &&
            !string.IsNullOrEmpty(tileManagerList[0].Internal_Text.text))
        {
            return true;
        }

        if (tileManagerList[2].Internal_Text.text == tileManagerList[4].Internal_Text.text &&
            tileManagerList[4].Internal_Text.text == tileManagerList[6].Internal_Text.text &&
            !string.IsNullOrEmpty(tileManagerList[2].Internal_Text.text))
        {
            return true;
        }

        return false;
    }
}
