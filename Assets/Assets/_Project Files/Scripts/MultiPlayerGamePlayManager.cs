using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MultiPlayerGamePlayManager : MonoBehaviourPunCallbacks
{

    internal Button[,] buttonGrid = new Button[3, 3];
    private Transform[,] pieceGrid = new Transform[3, 3];
    private int[,] board = new int[3, 3];

    private Queue<Vector2Int> myMoves = new Queue<Vector2Int>();
    private int myPlayerNumber;

    private bool isMyTurn = false;
    private bool gameOver = false;

    private void Start()
    {
        UI_Manager.Instance.CreateGridButtons();
        UI_Manager.Instance.restartButton.onClick.AddListener(() => {GoogleAdsManager.Instance.OnGameFinished(GoogleAdsManager.GameMode.Multiplayer); });
        TicTacToeHelper.InitializeGrid(UI_Manager.Instance.gridButtons, out buttonGrid);

        for (int i = 0; i < UI_Manager.Instance.gridButtons.Length; i++)
        {
            int row = i / 3;
            int col = i % 3;
            int r = row, c = col;
            buttonGrid[row, col].onClick.AddListener(() => OnCellClicked(r, c));
        }

        /*if (UI_Manager.Instance.restartButton != null)
            UI_Manager.Instance.restartButton.onClick.AddListener(ResetGame);*/

        myPlayerNumber = PhotonNetwork.IsMasterClient ? 1 : 2;
        isMyTurn = myPlayerNumber == 1;
        photonView.RPC("InstantiateTurnIcon", RpcTarget.All);
        UpdateStatus(isMyTurn ? "Your Turn" : "Opponent Turn");

    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game is exiting...");
        Audio_Manager.Instance.PlayMusic(Audio_Manager.Instance.buttonclick, Audio_Manager.Instance.sfxVolume);
    }

    private void OnCellClicked(int row, int col)
    {
        if (!isMyTurn || gameOver || board[row, col] != 0)
            return;

        photonView.RPC("MakeMove", RpcTarget.All, row, col, myPlayerNumber);
    }

    /*[PunRPC]
    private void MakeMove(int row, int col, int playerNumber)
    {
        if (board[row, col] != 0 || gameOver)
            return;

        board[row, col] = playerNumber;

        GameObject piecePrefab = (playerNumber == 1) ? UI_Manager.Instance.chickenPrefab : UI_Manager.Instance.duckPrefab;
        TicTacToeHelper.PlacePiece(row, col, piecePrefab, buttonGrid, pieceGrid);

        if (playerNumber == myPlayerNumber)
        {
            myMoves.Enqueue(new Vector2Int(row, col));
            if (myMoves.Count > 3)
                RemoveOldestPiece();
        }

        List<Vector2Int> winPositions;
        if (TicTacToeHelper.CheckWin(board, playerNumber, out winPositions))
        {
            gameOver = true;
            TicTacToeHelper.HighlightWinningCells(winPositions, buttonGrid);
            UI_Manager.Instance.winPanel.SetActive(true);
            UI_Manager.Instance.winningText.text = (playerNumber == myPlayerNumber) ? "You Win!" : "Opponent Wins!";
            UpdateStatus((playerNumber == myPlayerNumber) ? "You Win!" : "Opponent Wins!");
        }

        isMyTurn = !isMyTurn;
        UI_Manager.Instance.chickenBG.SetActive(isMyTurn);
        UI_Manager.Instance.duckBG.SetActive(!isMyTurn);
        UpdateStatus(isMyTurn ? "Your Turn" : "Opponent Turn");
    }
*/

    [PunRPC]
    private void MakeMove(int row, int col, int playerNumber)
    {
        if (board[row, col] != 0 || gameOver)
            return;
        Audio_Manager.Instance.PlayMusic(Audio_Manager.Instance.gridMusic, Audio_Manager.Instance.sfxVolume);

        board[row, col] = playerNumber;

        /* GameObject piecePrefab = (playerNumber == 1) ? UI_Manager.Instance.chickenPrefab : UI_Manager.Instance.duckPrefab;
         TicTacToeHelper.PlacePiece(row, col, piecePrefab, buttonGrid, pieceGrid);*/
        TicTacToeHelper.PlacePiece(row, col, playerNumber, UI_Manager.Instance.chickenPrefab, UI_Manager.Instance.duckPrefab, UI_Manager.Instance.gridButtons, pieceGrid);

        if (playerNumber == myPlayerNumber)
        {
            myMoves.Enqueue(new Vector2Int(row, col));
            if (myMoves.Count > 3)
            {
                Vector2Int oldestMove = myMoves.Dequeue();
                photonView.RPC("RemovePieceAt", RpcTarget.All, oldestMove.x, oldestMove.y);
            }
        }

        List<Vector2Int> winPositions;
        if (TicTacToeHelper.CheckWin(board, playerNumber, out winPositions))
        {
            gameOver = true;

            List<int> cellData = new List<int>();
            foreach (var pos in winPositions)
            {
                cellData.Add(pos.x);
                cellData.Add(pos.y);
            }

            photonView.RPC("ShowWinPanel", RpcTarget.All, playerNumber, cellData.ToArray());
        }

        isMyTurn = !isMyTurn;
        photonView.RPC("AssignBgForHighlightTheTurns", RpcTarget.All, playerNumber);

        UpdateStatus(isMyTurn ? "Your Turn" : "Opponent Turn");
    }


    [PunRPC]
    private void ShowWinPanel(int winnerPlayerNumber, int[] winCells)
    {
        gameOver = true;

        // Rebuild the winning positions
        List<Vector2Int> winPositions = new List<Vector2Int>();
        for (int i = 0; i < winCells.Length; i += 2)
            winPositions.Add(new Vector2Int(winCells[i], winCells[i + 1]));

        HighlightWinningCells(winPositions);
        StartCoroutine(ShowPanelAfterDelay(winnerPlayerNumber));
        /*
                UI_Manager.Instance.winPanel.SetActive(true);
                UI_Manager.Instance.winningText.text = (winnerPlayerNumber == myPlayerNumber) ? "You Win!" : "Opponent Wins!";
                UpdateStatus((winnerPlayerNumber == myPlayerNumber) ? "You Win!" : "Opponent Wins!");*/
    }

    private IEnumerator ShowPanelAfterDelay(int winnerPlayerNumber)
    {
        yield return new WaitForSeconds(1f);

        UpdateStatus((winnerPlayerNumber == myPlayerNumber) ? "You Win!" : "Opponent Wins!");
        UI_Manager.Instance.winPanel.SetActive(true);
        UI_Manager.Instance.winningText.text = (winnerPlayerNumber == myPlayerNumber) ? "You Win!" : "Opponent Wins!";
    }
    private void HighlightWinningCells(List<Vector2Int> winningCells)
    {
        foreach (var pos in winningCells)
        {
            Button btn = buttonGrid[pos.x, pos.y];
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.green;
            btn.colors = colors;
            Image img = btn.GetComponent<Image>();
            if (img != null)
            {
                Color imgColor = img.color;
                imgColor.a = 1f;
                img.color = imgColor;
            }
        }
    }

    [PunRPC]
    private void RemovePieceAt(int row, int col)
    {
        board[row, col] = 0;

        if (pieceGrid[row, col] != null)
        {
            Destroy(pieceGrid[row, col].gameObject);
            pieceGrid[row, col] = null;
        }
    }

    /* private void RemoveOldestPiece()
     {
         if (myMoves.Count == 0) return;

         Vector2Int oldMove = myMoves.Dequeue();
         board[oldMove.x, oldMove.y] = 0;

         if (pieceGrid[oldMove.x, oldMove.y] != null)
         {
             Destroy(pieceGrid[oldMove.x, oldMove.y].gameObject);
             pieceGrid[oldMove.x, oldMove.y] = null;
         }
     }*/

    public void RestartMultiplayerGame()
    {
        photonView.RPC(methodName: "ResetGame", RpcTarget.All);
    }

    [PunRPC]
    public void ResetGame()
    {
        TicTacToeHelper.ResetGrid(board, pieceGrid, UI_Manager.Instance.gridButtons);
        Audio_Manager.Instance.PlayMusic(Audio_Manager.Instance.buttonclick, Audio_Manager.Instance.sfxVolume);
        myMoves.Clear();
        gameOver = false;
        isMyTurn = myPlayerNumber == 1;
        foreach (var btn in UI_Manager.Instance.gridButtons)
        {
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            btn.colors = colors;
            Image img = btn.GetComponent<Image>();
            if (img != null)
            {
                Color imgColor = img.color;
                imgColor.a = 0.5f;
                img.color = imgColor;
            }
        }

        UpdateStatus(isMyTurn ? "Your Turn" : "Opponent Turn");
        UI_Manager.Instance.chickenBG.SetActive(isMyTurn);
        UI_Manager.Instance.duckBG.SetActive(!isMyTurn);
    }

    [PunRPC]
    private void InstantiateTurnIcon()
    {
        if (!UI_Manager.Instance.duckBG.activeSelf && !UI_Manager.Instance.chickenBG.activeSelf)
        {
            UI_Manager.Instance.chickenBG.SetActive(true);
            UI_Manager.Instance.duckBG.SetActive(true);
        }
    }

    [PunRPC]
    private void AssignBgForHighlightTheTurns(int myPlayerNum)
    {
       
        if (myPlayerNum == 1)
        {
            var go=UI_Manager.Instance.chickenBG;
            go.transform.GetChild(0).gameObject.SetActive(true);
            UI_Manager.Instance.duckBG.transform.GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            UI_Manager.Instance.chickenBG.transform.GetChild(0).gameObject.SetActive(false);
            var go =UI_Manager.Instance.duckBG;
            go.transform.GetChild(0).gameObject.SetActive(true);
        }
    }
    private void UpdateStatus(string message)
    {
        if (message == "You Win!")
        {
            Audio_Manager.Instance.PlayMusic(Audio_Manager.Instance.winMusic, Audio_Manager.Instance.sfxVolume);
        }
        if (message == "Opponent Wins!")
        {
            Audio_Manager.Instance.PlayMusic(Audio_Manager.Instance.loseMusic, Audio_Manager.Instance.sfxVolume);
        }
        if (UI_Manager.Instance.statusText != null)
            UI_Manager.Instance.statusText.text = message;
    }
}


