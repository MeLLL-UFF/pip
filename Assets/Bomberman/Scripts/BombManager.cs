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

    public float getDanger(int x, int y, Player player)
    {
        Dictionary<ulong, Danger> dict = new Dictionary<ulong, Danger>();

        //iterando para descobrir dangers em uma célula
        foreach (KeyValuePair<ulong, Danger> entry in dangerZone)
        {
            Danger danger = entry.Value;
            Vector2 pos = danger.GetGridPosition();
            if ((int)pos.x == x && (int)pos.y == y)
            {
                if (!dict.ContainsKey((ulong)danger.bomberOwnerNumber))
                    dict.Add((ulong)danger.bomberOwnerNumber, danger);
            } 
        }

        float maxDangerLevel = 0.0f;
        int playerNumber = -1;

        //iterando para descobrir qual é o danger com valor absoluto mais alto. Se empate, escolhe danger do próprio player
        foreach(KeyValuePair<ulong, Danger> entry in dict)
        {
            Danger danger = entry.Value;
            if (danger != null)
            {
                float dangerLevel = danger.GetDangerLevelOfPositionRaw();

                if (dangerLevel == maxDangerLevel)
                {
                    if (danger.bomberOwnerNumber == player.getPlayerNumber())
                    {
                        maxDangerLevel = dangerLevel;
                        playerNumber = player.getPlayerNumber();
                    }
                }
                else if (dangerLevel > maxDangerLevel)
                {
                    maxDangerLevel = dangerLevel;
                    playerNumber = danger.bomberOwnerNumber;
                }
                
            }
        }

        // se houver mais de um danger numa celula, esse codigo está pegando apenas o danger de uma celula. Do primeiro danger possivelmente que foi adicionado a celula.
        // porem Dictionary não mantem ordem. Logo, o treinamento do ICAART deveria ser realizado novamente. Mas não temos tempo.

        //ICAART paper does not explain: the same grid cell can be affected by an agent bomb or an enemy bomb. In this case, there is no explanation of what to do to represent the danger level of the cell.

        float penalty = 1.0f;
        if (playerNumber != -1)
        {
            penalty = playerNumber == player.getPlayerNumber() ? -1.0f : 1.0f;
        }

        dict.Clear();

        return maxDangerLevel * penalty;
    }

    public Dictionary<int, DestroySelf> getExplosions(int x, int y)
    {
        Dictionary<int, DestroySelf> dict = new Dictionary<int, DestroySelf>();

        foreach (KeyValuePair<ulong, DestroySelf> entry in explosions)
        {
            DestroySelf explosion = entry.Value;
            Vector2 pos = explosion.GetGridPosition();
            if ((int)pos.x == x && (int)pos.y == y)
            {
                if (!dict.ContainsKey(explosion.bombermanOwnerNumber))
                    dict.Add(explosion.bombermanOwnerNumber, explosion);
            }
        }

        return dict;
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

    public void checkExplosions(Grid grid)
    {
        List<Bomb> list = new List<Bomb>();

        foreach (KeyValuePair<ulong, GameObject> entry in bombs)
        {
            Bomb bomb = entry.Value.GetComponent<Bomb>();

            if (!bomb.exploded)
            {
                Vector2 bombPos = bomb.GetGridPosition();

                if (grid.checkFire(bombPos))
                {
                    if (bomb.ForceExplode())
                    {
                        list.Add(bomb);
                    }
                }
            }
        }

        bool hasUpdate = list.Count > 0;

        for (int i = 0; i < list.Count; ++i)
        {
            removeBomb(list[i].bombId);
        }

        if (hasUpdate)
            checkExplosions(grid);
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
