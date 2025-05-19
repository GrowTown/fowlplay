using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class AI_Manager : MonoBehaviour
{

    private Button[,] buttonGrid = new Button[3, 3];
    private Transform[,] pieceGrid = new Transform[3, 3];
    private int[,] board = new int[3, 3]; // 0: empty, 1: player, 2: AI

    private Queue<Vector2Int> playerMoves = new Queue<Vector2Int>();
    private Queue<Vector2Int> aiMoves = new Queue<Vector2Int>();

    private int playerTurnCount = 0;
    private int aiTurnCount = 0;

    private bool isPlayerTurn = true;
    private bool gameOver = false;

    private void Start()
    {
        UI_Manager.Instance.CreateGridButtons();
        TicTacToeHelper.InitializeGrid(UI_Manager.Instance.gridButtons, out buttonGrid);

        for (int i = 0; i < UI_Manager.Instance.gridButtons.Length; i++)
        {
            int row = i / 3;
            int col = i % 3;
            buttonGrid[row, col].onClick.AddListener(() => OnPlayerMove(row, col));
        }

        // Hook up restart button
        if (UI_Manager.Instance.restartButton != null)
            UI_Manager.Instance.restartButton.onClick.AddListener(ResetGame);

        UpdateStatus("Your Turn");
        UI_Manager.Instance.chickenBG.SetActive(true);
    }

    private void OnPlayerMove(int row, int col)
    {
        if (!isPlayerTurn || gameOver || board[row, col] != 0)
            return;

        TicTacToeHelper.PlacePiece(row, col, 1, UI_Manager.Instance.chickenPrefab, UI_Manager.Instance.duckPrefab, UI_Manager.Instance.gridButtons, pieceGrid);
        board[row, col] = 1;
        playerMoves.Enqueue(new Vector2Int(row, col));
        playerTurnCount++;

        if (playerMoves.Count > 3)
            RemoveOldestPiece(playerMoves, 1);

        List<Vector2Int> winPositions;
        if (TicTacToeHelper.CheckWin(board, 1, out winPositions))
        {
            gameOver = true;
            UpdateStatus("You Win!");
            TicTacToeHelper.HighlightWinningCells(winPositions, buttonGrid);
            StartCoroutine(ShowPanelAfterDelay("You Win!"));
            return;
        }

        isPlayerTurn = false;
        UpdateStatus("AI Turn");
        UI_Manager.Instance.chickenBG.SetActive(false);
        UI_Manager.Instance.duckBG.SetActive(true);
        Invoke(nameof(AIMove), 0.5f); 
    }

    private void AIMove()
    {
        if (gameOver)
            return;

        Vector2Int move = TicTacToeHelper.FindBestAIMove(board);
      /*  if (move.x == -1)
        {
            gameOver = true;
            UpdateStatus("It's a Draw!");
            return;
        }*/

        int row = move.x;
        int col = move.y;

        TicTacToeHelper.PlacePiece(row, col, 2, UI_Manager.Instance.chickenPrefab, UI_Manager.Instance.duckPrefab, UI_Manager.Instance.gridButtons, pieceGrid);
        board[row, col] = 2;
        aiMoves.Enqueue(move);
        aiTurnCount++;

        if (aiMoves.Count > 3)
            RemoveOldestPiece(aiMoves, 2);

        List<Vector2Int> winPositions;
        if (TicTacToeHelper.CheckWin(board, 2, out winPositions))
        {
            gameOver = true;
            UpdateStatus("AI Wins!");
            TicTacToeHelper.HighlightWinningCells(winPositions, buttonGrid);
            StartCoroutine(ShowPanelAfterDelay("AI Wins!"));
            return;
        }

        isPlayerTurn = true;
        UpdateStatus("Your Turn");
        UI_Manager.Instance.chickenBG.SetActive(true);
        UI_Manager.Instance.duckBG.SetActive(false);
    }


    private IEnumerator ShowPanelAfterDelay(string winTx)
    {
        yield return new WaitForSeconds(1f);

        UI_Manager.Instance.winPanel.SetActive(true);
        UI_Manager.Instance.winningText.text = winTx;
    }

    private void RemoveOldestPiece(Queue<Vector2Int> moveQueue, int player)
    {
        if (moveQueue.Count == 0) return;

        Vector2Int oldMove = moveQueue.Dequeue();
        board[oldMove.x, oldMove.y] = 0;

        if (pieceGrid[oldMove.x, oldMove.y] != null)
        {
            Destroy(pieceGrid[oldMove.x, oldMove.y].gameObject);
            pieceGrid[oldMove.x, oldMove.y] = null;
        }
    }

    public void ResetGame()
    {
        TicTacToeHelper.ResetGrid(board, pieceGrid, UI_Manager.Instance.gridButtons);

        playerMoves.Clear();
        aiMoves.Clear();
        playerTurnCount = 0;
        aiTurnCount = 0;
        gameOver = false;
        isPlayerTurn = true;
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

        UpdateStatus("Your Turn");
        UI_Manager.Instance.chickenBG.SetActive(true);
        UI_Manager.Instance.duckBG.SetActive(false);
    }

    private void UpdateStatus(string message)
    {
        if (UI_Manager.Instance.statusText != null)
            UI_Manager.Instance.statusText.text = message;
    }


}






