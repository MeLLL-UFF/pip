using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombManager {

    static bool initialized = false;

    private Dictionary<int, GameObject> bombs = new Dictionary<int, GameObject>();
    private static int count = 1;

    int[,] grid = new int[8, 7];

    public Dictionary<int, DestroySelf> explosions = new Dictionary<int, DestroySelf>();
    private static int explosionCount = 1;

    internal BombManager()
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

    public void updateBombOnGrid(Bomb bomb, bool enable)
    {
        Vector2 pos = bomb.GetGridPosition();
        int x = (int)pos.x;
        int z = (int)pos.y;
        if (x >= 0 && x < 8 && z >= 0 && z < 7)
            grid[x, z] = System.Convert.ToInt32(enable);
    }

    public void addBomb(GameObject bomb)
    {
        bomb.GetComponent<Bomb>().bombId = count;
        bombs.Add(count, bomb);
        count++;

        updateBombOnGrid(bomb.GetComponent<Bomb>(), true);
    }

    public void removeBomb(int bombId)
    {
        if (bombs.ContainsKey(bombId))
        {
            updateBombOnGrid(bombs[bombId].GetComponent<Bomb>(), false);
            bombs.Remove(bombId);
        }
    }

    public void addExplosion(DestroySelf explosion)
    {
        explosion.id = explosionCount;
        explosions.Add(explosionCount, explosion);
        explosionCount++;
    }

    public void removeExplosion(int exId)
    {
        if (explosions.ContainsKey(exId))
        {
            explosions.Remove(exId);
        }
    }

    public void clearBombs()
    {
        foreach (KeyValuePair<int, DestroySelf> entry in explosions)
        {
            entry.Value.forceDestroy();
        }
        explosions.Clear();

        foreach (KeyValuePair<int, GameObject> entry in bombs)
        {
            entry.Value.GetComponent<Bomb>().autoDestroy();
        }

        bombs.Clear();
        clearGrid();
    }

    public List<Bomb> getBombs(int maxBombs)
    {
        List<Bomb> list = new List<Bomb>();

        foreach (KeyValuePair<int, GameObject> entry in bombs)
        {
            list.Add(entry.Value.GetComponent<Bomb>());
        }

        //preenchendo vetor com bombas nulas para deixá-lo fixo
        if (list.Count < maxBombs)
        {
            for(int i = list.Count-1; i < maxBombs-1; i++)
            {
                list.Add(null);
            }
        }

        return list;
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
