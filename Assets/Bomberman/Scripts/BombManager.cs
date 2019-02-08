using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombManager {

    bool initialized = false;

    private Dictionary<ulong, GameObject> bombs = new Dictionary<ulong, GameObject>();
    private static ulong count = 1;

    public Dictionary<ulong, DestroySelf> explosions = new Dictionary<ulong, DestroySelf>();
    private static ulong explosionCount = 1;

    public Dictionary<ulong, Danger> dangerZone = new Dictionary<ulong, Danger>();
    private static ulong dangerCount = 1;

    internal BombManager()
    {
        if (!initialized)
        {
            initialized = true;
        }
    }

    public bool existsBombOrDanger()
    {
        if (bombs.Count > 0 || dangerZone.Count > 0)
            return true;

        return false;
    }

    public void addBomb(GameObject bomb)
    {
        bomb.GetComponent<Bomb>().bombId = count;
        bombs.Add(count, bomb);
        count++;
    }

    public void removeBomb(ulong bombId)
    {
        if (bombs.ContainsKey(bombId))
        {
            bombs.Remove(bombId);
        }
    }

    public void addExplosion(DestroySelf explosion)
    {
        explosion.id = explosionCount;
        explosions.Add(explosionCount, explosion);
        explosionCount++;
    }

    public void removeExplosion(ulong exId)
    {
        if (explosions.ContainsKey(exId))
        {
            explosions.Remove(exId);
        }
    }

    public void addDanger(Danger danger)
    {
        danger.id = dangerCount;
        dangerZone.Add(dangerCount, danger);
        dangerCount++;
    }

    public void removeDanger(ulong dId)
    {
        if (dangerZone.ContainsKey(dId))
        {
            dangerZone.Remove(dId);
        }
    }

    public Danger getDanger(int x, int y)
    {
        foreach (KeyValuePair<ulong, Danger> entry in dangerZone)
        {
            Vector2 pos = entry.Value.GetGridPosition();
            if ((int)pos.x == x && (int)pos.y == y)
                return entry.Value;
        }

        return null;
    }

    public DestroySelf getExplosion(int x, int y)
    {
        foreach (KeyValuePair<ulong, DestroySelf> entry in explosions)
        {
            Vector2 pos = entry.Value.GetGridPosition();
            if ((int)pos.x == x && (int)pos.y == y)
                return entry.Value;
        }

        return null;
    }

    public void clearBombs()
    {
        foreach (KeyValuePair<ulong, DestroySelf> entry in explosions)
        {
            entry.Value.forceDestroy();
        }
        explosions.Clear();

        foreach (KeyValuePair<ulong, Danger> entry in dangerZone)
        {
            entry.Value.forceDestroy();
        }
        dangerZone.Clear();

        foreach (KeyValuePair<ulong, GameObject> entry in bombs)
        {
            entry.Value.GetComponent<Bomb>().autoDestroy();
        }

        bombs.Clear();
    }

    public void timeIterationUpdate()
    {
        // explosões devem ser chamadas antes da atualização da bomba porque senão elas sofrem já uma atualização após a bomba explodir.
        List<DestroySelf> listExplosions = new List<DestroySelf>();
        foreach (KeyValuePair<ulong, DestroySelf> entry in explosions)
        {
            if (entry.Value.iterationUpdate())
                listExplosions.Add(entry.Value);
        }

        for (int i = 0; i < listExplosions.Count; ++i)
        {
            removeExplosion(listExplosions[i].id);
        }

        List<Bomb> list = new List<Bomb>();
        foreach (KeyValuePair<ulong, GameObject> entry in bombs)
        {
            if (entry.Value.GetComponent<Bomb>().iterationUpdate())
                list.Add(entry.Value.GetComponent<Bomb>());
        }

        for (int i = 0; i < list.Count; ++i)
        {
            removeBomb(list[i].bombId);
        }

        List<Danger> listDanger = new List<Danger>();
        foreach (KeyValuePair<ulong, Danger> entry in dangerZone)
        {
            if (entry.Value.iterationUpdate())
                listDanger.Add(entry.Value);
        }

        for (int i = 0; i < listDanger.Count; ++i)
        {
            removeDanger(listDanger[i].id);
        }
    }

    public List<Bomb> getBombs(int maxBombs)
    {
        List<Bomb> list = new List<Bomb>();

        foreach (KeyValuePair<ulong, GameObject> entry in bombs)
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
}
