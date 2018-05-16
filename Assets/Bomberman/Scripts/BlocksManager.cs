﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlocksManager {

    static bool initialized = false;

    private List<Destructable> blocks = new List<Destructable>();

    int[,] grid = new int[8, 7];

    internal BlocksManager()
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

    public void enableBlockOnGrid(Destructable block)
    {
        Vector2 pos = block.GetGridPosition();
        int x = (int)pos.x;
        int z = (int)pos.y;
        grid[x, z] = 1;
    }

    public void disableBlockOnGrid(Destructable block)
    {
        Vector2 pos = block.GetGridPosition();
        int x = (int)pos.x;
        int z = (int)pos.y;
        grid[x, z] = 0;
    }

    public void addBlock(Destructable block)
    {
        blocks.Add(block);

        enableBlockOnGrid(block);
    }

    public void resetBlocks()
    {
        clearGrid();

        for (int i = 0; i < blocks.Count; i++)
        {
            blocks[i].reset();
            enableBlockOnGrid(blocks[i]);
        }
    }

    public void clear()
    {
        blocks.Clear();
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

    public int[,] getGrid()
    {
        return grid;
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