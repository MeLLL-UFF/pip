using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayersManager {

    static bool initialized = false;

    private List<Player> players = new List<Player>();

    int[,] grid = new int[8, 7];

    internal PlayersManager()
    {
        if (!initialized)
        {
            clearGrid();

            initialized = true;
        }
    }

    private void clearGrid()
    {
        for (int x = 0; x < 8; ++x)
            for (int y = 0; y < 7; ++y)
                grid[x, y] = 0;
    }

    public int[,] getGrid()
    {
        return grid;
    }

    public void updatePlayerOnGrid(Player player)
    {
        Vector2 pos = player.GetOldGridPosition();
        int x = Mathf.RoundToInt(pos.x);
        int z = Mathf.RoundToInt(pos.y);
        if (x >= 0 && x < 8 && z >= 0 && z < 7)
            grid[x, z] = 0;

        pos = player.GetGridPosition();
        x = Mathf.RoundToInt(pos.x);
        z = Mathf.RoundToInt(pos.y);
        if (x >= 0 && x < 8 && z >= 0 && z < 7)
            grid[x, z] = 1;
    }

    public void clearPlayerOnGrid(Player player)
    {
        Vector2 pos = player.GetOldGridPosition();
        int x = (int)pos.x;
        int z = (int)pos.y;
        if (x >= 0 && x < 8 && z >= 0 && z < 7)
            grid[x, z] = 0;

        pos = player.GetGridPosition();
        x = (int)pos.x;
        z = (int)pos.y;
        if (x >= 0 && x < 8 && z >= 0 && z < 7)
            grid[x, z] = 0;
    }

    public void print()
    {
        string saida = "";
        for (int x = 0; x < 8; ++x)
        {
            for (int y = 0; y < 7; ++y)
            {
                saida += grid[x, y].ToString() + " | ";
            }
            saida += "\n";
        }

        ServiceLocator.GetLogManager().print(saida);
    }

    public string gridToString()
    {
        string saida = "";
        for (int x = 0; x < 8; ++x)
        {
            for (int y = 0; y < 7; ++y)
            {
                saida += grid[x, y].ToString() + " | ";
            }
            saida += "\n";
        }

        return saida;
    }
}
