using UnityEngine;
using UnityEngine.UI;

public static class TicTacToeHelper
{

    public static void InitializeGrid(Button[] buttons, out Button[,] grid)
    {
        grid = new Button[3, 3];
        for (int i = 0; i < buttons.Length; i++)
        {
            int row = i / 3;
            int col = i % 3;
            grid[row, col] = buttons[i];
        }
    }

    // Places a piece (chicken or duck) at a specific grid position
    public static void PlacePiece(int row, int col, int player, GameObject chickenPrefab, GameObject duckPrefab, Button[] gridButtons, Transform[,] pieceGrid)
    {
        Vector3 spawnPosition = gridButtons[row * 3 + col].transform.position;
        GameObject prefab = (player == 1) ? chickenPrefab : duckPrefab;
        GameObject piece = Object.Instantiate(prefab, spawnPosition, Quaternion.identity, gridButtons[row * 3 + col].transform);
        pieceGrid[row, col] = piece.transform;
    }

    // Checks if a player has won the game
    public static bool CheckWin(int[,] board, int player)
    {
        // Rows and Columns
        for (int i = 0; i < 3; i++)
        {
            if ((board[i, 0] == player && board[i, 1] == player && board[i, 2] == player) ||
                (board[0, i] == player && board[1, i] == player && board[2, i] == player))
                return true;
        }

        // Diagonals
        if ((board[0, 0] == player && board[1, 1] == player && board[2, 2] == player) ||
            (board[0, 2] == player && board[1, 1] == player && board[2, 0] == player))
            return true;

        return false;
    }

    // Resets the board and UI
    public static void ResetGrid(int[,] board, Transform[,] pieceGrid, Button[] gridButtons)
    {
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                board[row, col] = 0;
                if (pieceGrid[row, col] != null)
                {
                    Object.Destroy(pieceGrid[row, col].gameObject);
                    pieceGrid[row, col] = null;
                }
            }
        }
    }

    // Simple AI: finds the first available cell (can upgrade to minimax later)
    public static Vector2Int FindBestAIMove(int[,] board)
    {
        // 1. Win if possible
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                if (board[row, col] == 0)
                {
                    board[row, col] = 2;
                    if (CheckWin(board, 2))
                    {
                        board[row, col] = 0;
                        return new Vector2Int(row, col);
                    }
                    board[row, col] = 0;
                }
            }
        }

        // 2. Block player win
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                if (board[row, col] == 0)
                {
                    board[row, col] = 1;
                    if (CheckWin(board, 1))
                    {
                        board[row, col] = 0;
                        return new Vector2Int(row, col);
                    }
                    board[row, col] = 0;
                }
            }
        }

        // 3. Pick center if available
        if (board[1, 1] == 0)
            return new Vector2Int(1, 1);

        // 4. Pick random empty cell
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                if (board[row, col] == 0)
                    return new Vector2Int(row, col);
            }
        }

        // 5. No move available
        return new Vector2Int(-1, -1);
    }
}


