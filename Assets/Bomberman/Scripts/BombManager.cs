using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombManager {

    static bool initialized = false;

    private Dictionary<int, GameObject> bombs = new Dictionary<int, GameObject>();
    private static int count = 1;


    public Dictionary<int, DestroySelf> explosions = new Dictionary<int, DestroySelf>();
    private static int explosionCount = 1;

    internal BombManager()
    {
        if (!initialized)
        {
            initialized = true;
        }
    }

    public void addBomb(GameObject bomb)
    {
        bomb.GetComponent<Bomb>().bombId = count;
        bombs.Add(count, bomb);
        count++;
    }

    public void removeBomb(int bombId)
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
}
