using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceStructure
{
    public float previousDistance;
    public float closestDistance;

    public DistanceStructure()
    {
        previousDistance = Mathf.Infinity;
        closestDistance = Mathf.Infinity;
    }
}

public class PlayerManager {

    bool initialized = false;
    bool updating = false;
    bool randomizeIterationOfAgents = false;
    bool lastManFound = false;

    private int deadCount;
    private int iterationCount;
    private int episodeCount;

    public string lastManAgent = "null";

    Dictionary<int, Player> playerDict = new Dictionary<int, Player>();
    List<Vector3> initPosList = new List<Vector3>();

    Dictionary<int, Dictionary<int, DistanceStructure>> distanceStructureDict = new Dictionary<int, Dictionary<int, DistanceStructure> >();

    public PlayerManager()
    {
        if (!initialized)
        {
            //Debug.Log("PlayerManager inicializado");
            deadCount = 0;
            iterationCount = 0;
            episodeCount = 1;
            restartInitList();
            initialized = true;
            updating = false;
            lastManAgent = "null";
            lastManFound = false;
        }
    }

    public void clear()
    {
        foreach (KeyValuePair<int, Player> entry in playerDict)
        {
            GameObject.Destroy(entry.Value.gameObject);
        }

        playerDict.Clear();
        clearDistanceStructuresForReward();
        clearDeadCount();
        restartInitList();

        iterationCount = 0;

        addEpisodeCount();

        lastManFound = false;
    }

    private void restartInitList()
    {
        initPosList.Clear();
        for (int i = 0; i < Player.initialPositions.Length; i++)
        {
            initPosList.Add(Player.initialPositions[i]);
        }
    }

    public Vector3 getNextRandomInitPosition()
    {
        if (initPosList.Count == 0)
            restartInitList();

        int number = UnityEngine.Random.Range(0, initPosList.Count);

        Vector3 randomPos = initPosList[number];
        initPosList.RemoveAt(number);

        return randomPos;
    }

    public bool containsPlayer(int playerNumber)
    {
        return playerDict.ContainsKey(playerNumber);
    }

    public void addPlayer(Player p, int playerNumber)
    {
        playerDict.Add(playerNumber, p);
    }

    public void removePlayer(int playerNumber)
    {
        playerDict.Remove(playerNumber);
    }

    public int getNumPlayers()
    {
        return playerDict.Count;
    }

    public void addDeadCount()
    {
        deadCount++;
    }

    public int getDeadCount()
    {
        return deadCount;
    }

    public void clearDeadCount()
    {
        deadCount = 0;
    }

    public void addIterationCount()
    {
        iterationCount++;
    }

    public int getIterationCount()
    {
        return iterationCount;
    }

    public void addEpisodeCount()
    {
        episodeCount++;
    }

    public int getEpisodeCount()
    {
        return episodeCount;
    }

    public Player getAgent(int playerNumber)
    {
        if (playerDict.ContainsKey(playerNumber))
        {
            return playerDict[playerNumber];
        }

        return null;
    }

    public void calculateDistanceEnemyPosition(Player agent)
    {
        foreach (KeyValuePair<int, Player> entry in playerDict)
        {
            Player enemy = entry.Value;
            if (!enemy.dead && enemy.getPlayerNumber() != agent.getPlayerNumber())
            {
                float distance = Vector2.Distance(agent.GetGridPosition(), entry.Value.GetGridPosition());
                if (distance < distanceStructureDict[agent.getPlayerNumber()][enemy.getPlayerNumber()].closestDistance)
                {
                    distanceStructureDict[agent.getPlayerNumber()][enemy.getPlayerNumber()].closestDistance = distance;
                    Player.AddRewardToAgent(agent, Config.REWARD_CLOSEST_DISTANCE, "Agente" + agent.getPlayerNumber() + " melhor aproximacao do inimigo " + enemy.getPlayerNumber());
                }

                if (distance < distanceStructureDict[agent.getPlayerNumber()][enemy.getPlayerNumber()].previousDistance)
                {
                    Player.AddRewardToAgent(agent, Config.REWARD_APPROACHED_DISTANCE, "Agente" + agent.getPlayerNumber() + " se aproximou do inimigo " + enemy.getPlayerNumber());
                }
                else if (distance > distanceStructureDict[agent.getPlayerNumber()][enemy.getPlayerNumber()].previousDistance)
                {
                    Player.AddRewardToAgent(agent, Config.REWARD_FAR_DISTANCE, "Agente" + agent.getPlayerNumber() + " se distanciou do inimigo " + enemy.getPlayerNumber());
                }

                distanceStructureDict[agent.getPlayerNumber()][enemy.getPlayerNumber()].previousDistance = distance;
            }
        }
    }

    public void createDistanceStructuresForReward()
    {
        foreach (KeyValuePair<int, Player> entry in playerDict)
        {
            int playerNumber = entry.Value.getPlayerNumber();

            foreach (KeyValuePair<int, Player> enemyEntry in playerDict)
            {
                int playerNumberEnemy = enemyEntry.Value.getPlayerNumber();
                if (playerNumber != playerNumberEnemy)
                {
                    if (!distanceStructureDict.ContainsKey(playerNumber))
                    {
                        Dictionary<int, DistanceStructure> enemyDistance = new Dictionary<int, DistanceStructure>();
                        distanceStructureDict.Add(playerNumber, enemyDistance);
                    }

                    distanceStructureDict[playerNumber].Add(playerNumberEnemy, new DistanceStructure());
                }
            }
        }
    }

    public void clearDistanceStructuresForReward()
    {
        distanceStructureDict.Clear();
    }

    public void verifyLastMan()
    {
        if (playerDict.Count == 1)
        {
            int lastManIndex = -1;
            foreach (KeyValuePair<int, Player> entry in playerDict)
            {
                lastManIndex = entry.Key;
            }

            if (lastManIndex != -1)
            {
                Player.AddRewardToAgent(playerDict[lastManIndex], Config.REWARD_LAST_MAN, "Agente" + playerDict[lastManIndex].getPlayerNumber() + ": foi o único sobrevivente");
                playerDict[lastManIndex].Done();
                lastManAgent = "Agente " + playerDict[lastManIndex].getPlayerNumber();
                lastManFound = true;
            }
        }
    }

    public bool isUpdating()
    {
        return updating;
    }

    public bool updateAgents()
    {
        updating = true;
        bool areThereUpdate = false;

        if (randomizeIterationOfAgents)
        {
            List<Player> playerList = new List<Player>(playerDict.Values);
            List<Player> randomList = new List<Player>();
            while (playerList.Count > 0)
            {
                int rand = Random.Range(0, playerList.Count);
                randomList.Add(playerList[rand]);
                playerList.RemoveAt(rand);
            }

            for (int i = 0; i < randomList.Count; ++i)
            {
                if (randomList[i].WaitIterationActions())
                    areThereUpdate = true;
            }
        }
        else
        {
            foreach (KeyValuePair<int, Player> entry in playerDict)
            {
                if (entry.Value.WaitIterationActions())
                    areThereUpdate = true;
            }
        }

        verifyLastMan();

        // se há algum agente vivo na cena
        if (areThereUpdate)
        {
            addIterationCount();
            updating = false;
            return true;
        }
        else
        {
            if (!lastManFound)
                lastManAgent = "draw";
            // precisamos resetar a cena
            updating = false;
            return false;
        }
    }

    public void setRandomizeIterationOfAgents(bool value)
    {
        randomizeIterationOfAgents = value;
    }
}
