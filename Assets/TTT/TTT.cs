using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public enum PlayerOption
{
    NONE, //0
    X, // 1
    O // 2
}

public class TTT : MonoBehaviour
{
    public int Rows;
    public int Columns;
    int maxCorners = 4;
    [SerializeField] BoardView board;

    PlayerOption currentPlayer = PlayerOption.X;
    Cell[,] cells;

    // Start is called before the first frame update
    void Start()
    {
        cells = new Cell[Columns, Rows];

        board.InitializeBoard(Columns, Rows);

        for(int i = 0; i < Rows; i++)
        {
            for(int j = 0; j < Columns; j++)
            {
                cells[j, i] = new Cell();
                cells[j, i].current = PlayerOption.NONE;
            }
        }
    }

    public void MakeOptimalMove()
    {
        PlayerOption ai = currentPlayer;
        PlayerOption opponent = (ai == PlayerOption.X) ? PlayerOption.O : PlayerOption.X;

        for (int pass = 0; pass < 2; pass++)
        {
            PlayerOption check = (pass == 0) ? opponent : ai;
            (int, int)[] corners = new (int, int)[] { (0, 0), (0, 2), (2, 0), (2, 2) };

            // checking if board is empty

            int emptyCount = 0;
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    if (cells[row, col].current == PlayerOption.NONE)
                    {
                        emptyCount++;
                    }
                }
            }

            if (emptyCount == 9) // board is empty take a corner
            {
                int cornerCount = 4;
                var pick = corners[Random.Range(0, cornerCount)];
                ChooseSpace(pick.Item1, pick.Item2);
                return;
            }

            // checking if opponent controls a corner
            int oppCount = 0;
            foreach (var corner in corners)
            {
                if (cells[corner.Item1, corner.Item2].current == opponent)
                {
                    oppCount++;
                }
            }

            if (oppCount == 1 && cells[1,1].current == PlayerOption.NONE) // oppenent controls 1 coner
            {
                ChooseSpace(1, 1); // player takes center
                Debug.Log("CENTER PLACED");
                return;
            }

            // checking if player controls a corner
            (int, int)[] allAdjacentArr = new (int, int)[8];
            int playerCount = 0;
            int adjIndex = 0;
            foreach (var corner in corners)
            {
                if (cells[corner.Item1, corner.Item2].current == ai)
                {

                    if (corner == (0, 0))
                    {
                        allAdjacentArr[adjIndex] = (0, 1);
                        allAdjacentArr[adjIndex++] = (1, 0);
                    }
                    else if (corner == (0, 2))
                    {
                        allAdjacentArr[adjIndex] = (0, 1);
                        allAdjacentArr[adjIndex++] = (1, 2);
                    }
                    else if (corner == (2, 0))
                    {
                        allAdjacentArr[adjIndex] = (1, 0);
                        allAdjacentArr[adjIndex++] = (2, 1);
                    }
                    else if (corner == (2, 2))
                    {
                        allAdjacentArr[adjIndex] = (1, 2);
                        allAdjacentArr[adjIndex++] = (2, 1);
                    }
                    adjIndex++;
                    playerCount++;
                }
            }

            Debug.Log("Player Count: " + playerCount);

            (int, int)[] adjacentArr = new (int, int)[9];

            int maxIndex = 0;
            foreach (var adj in allAdjacentArr)
            {
                if (cells[adj.Item1, adj.Item2].current == PlayerOption.NONE)
                {
                    adjacentArr[maxIndex++] = adj;
                    
                }
            }

            if (playerCount == 1 && cells[1,1].current == PlayerOption.NONE) // player controls 1 coner & center is open
            {
                var pick = adjacentArr[Random.Range(0, maxIndex)];
                Debug.Log("ADJACENT CELL PLACED");
                ChooseSpace(pick.Item1, pick.Item2);
                return;
            }

            // check if move is winning?

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    if (cells[row, col].current == PlayerOption.NONE)
                    {
                        cells[row, col].current = check;
                        if (GetWinner() == check)
                        {
                            cells[row, col].current = PlayerOption.NONE;
                            ChooseSpace(row, col);
                            Debug.Log("MADE WINNING MOVED OR BLOCKED!");
                            return;
                        }
                        cells[row, col].current = PlayerOption.NONE;
                    }
                }
            }


            // failsafe feature here
            (int, int)[] open = new (int, int)[9];
            int count = 0;

            for (int row = 0; row < Columns; row++)
            {
                for (int col = 0; col < Rows; col++)
                {
                    if (cells[row, col].current == PlayerOption.NONE)
                    {
                        open[count] = (row, col);
                        count++;
                    }
                }

            }

            if(count > 0 ) // place random space here
            {
                Debug.LogError("FAILSAFE ACTIVATED!");
                var pick = open[Random.Range(0, count)];
                ChooseSpace(pick.Item1 , pick.Item2);
                return;
            }
          
        }


        
    }

    public void ChooseSpace(int column, int row)
    {
        // can't choose space if game is over
        if (GetWinner() != PlayerOption.NONE)
            return;

        // can't choose a space that's already taken
        if (cells[column, row].current != PlayerOption.NONE)
            return;

        // set the cell to the player's mark
        cells[column, row].current = currentPlayer;

        // update the visual to display X or O
        board.UpdateCellVisual(column, row, currentPlayer);

        // if there's no winner, keep playing, otherwise end the game
        if(GetWinner() == PlayerOption.NONE)
            EndTurn();
        else
        {
            Debug.Log("GAME OVER!");
            Time.timeScale = 0;
        }
    }

    public void EndTurn()
    {
        // increment player, if it goes over player 2, loop back to player 1
        currentPlayer += 1;
        if ((int)currentPlayer > 2)
            currentPlayer = PlayerOption.X;
    }

    public PlayerOption GetWinner()
    {
        // sum each row/column based on what's in each cell X = 1, O = -1, blank = 0
        // we have a winner if the sum = 3 (X) or -3 (O)
        int sum = 0;

        // check rows
        for (int i = 0; i < Rows; i++)
        {
            sum = 0;
            for (int j = 0; j < Columns; j++)
            {
                var value = 0;
                if (cells[j, i].current == PlayerOption.X)
                    value = 1;
                else if (cells[j, i].current == PlayerOption.O)
                    value = -1;

                sum += value;
            }

            if (sum == 3)
                return PlayerOption.X;
            else if (sum == -3)
                return PlayerOption.O;

        }

        // check columns
        for (int j = 0; j < Columns; j++)
        {
            sum = 0;
            for (int i = 0; i < Rows; i++)
            {
                var value = 0;
                if (cells[j, i].current == PlayerOption.X)
                    value = 1;
                else if (cells[j, i].current == PlayerOption.O)
                    value = -1;

                sum += value;
            }

            if (sum == 3)
                return PlayerOption.X;
            else if (sum == -3)
                return PlayerOption.O;

        }

        // check diagonals
        // top left to bottom right
        sum = 0;
        for(int i = 0; i < Rows; i++)
        {
            int value = 0;
            if (cells[i, i].current == PlayerOption.X)
                value = 1;
            else if (cells[i, i].current == PlayerOption.O)
                value = -1;

            sum += value;
        }

        if (sum == 3)
            return PlayerOption.X;
        else if (sum == -3)
            return PlayerOption.O;

        // top right to bottom left
        sum = 0;
        for (int i = 0; i < Rows; i++)
        {
            int value = 0;

            if (cells[Columns - 1 - i, i].current == PlayerOption.X)
                value = 1;
            else if (cells[Columns - 1 - i, i].current == PlayerOption.O)
                value = -1;

            sum += value;
        }

        if (sum == 3)
            return PlayerOption.X;
        else if (sum == -3)
            return PlayerOption.O;

        return PlayerOption.NONE;
    }
}
