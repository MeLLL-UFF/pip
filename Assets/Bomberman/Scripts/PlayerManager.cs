using System;
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
    public uint lastManAgentResult = 0;

    Dictionary<int, Player> playerDict = new Dictionary<int, Player>();
    List<Vector3> initPosList = new List<Vector3>();

    Dictionary<int, Dictionary<int, DistanceStructure>> distanceStructureDict = new Dictionary<int, Dictionary<int, DistanceStructure> >();

    public Vector3[] staticInitialPositions; 

    public class DistanceReward
    {
        public bool applyRewardClosest;
        public string applyRewardClosestMessage;

        public bool applyRewardFar;
        public string applyRewardFarMessage;
        public bool applyRewardApproach;
        public string applyRewardApproachMessage;

        public DistanceReward()
        {
            reset();
        }

        public void reset()
        {
            applyRewardClosest = false;
            applyRewardClosestMessage = "";

            applyRewardFar = false;
            applyRewardFarMessage = "";
            applyRewardApproach = false;
            applyRewardApproachMessage = "";
        }
    }

    public PlayerManager()
    {
        if (!initialized)
        {
            //Debug.Log("PlayerManager inicializado");
            deadCount = 0;
            iterationCount = 0;
            episodeCount = 1;
            initialized = true;
            updating = false;
            lastManAgent = "null";
            lastManAgentResult = 0;
            lastManFound = false;
        }
    }

    public void stopToSendExperienceToAllPlayers()
    {
        foreach (KeyValuePair<int, Player> entry in playerDict)
        {
            entry.Value.stopToSendExperience();
        }
    }

    public string processReplayWriteInitialPosition()
    {
        int numberOfAgents = getNumPlayers();
        string line = "IP:";
        int i = 1;

        foreach (KeyValuePair<int, Player> entry in playerDict)
        {
            Player player = entry.Value;
            if (player != null)
            {
                {
                    Vector2Int pos = Vector2Int.FloorToInt(player.GetGridPosition());
                    if (i != numberOfAgents)
                        line += player.getPlayerNumber() + "," + pos.x + "," + pos.y + ";";
                    else
                        line += player.getPlayerNumber() + "," + pos.x + "," + pos.y;
                }
            }
            ++i;
        }

        return line;
    }

    public string processReplayWriteActions()
    {
        string line = "";
        int numberOfAgents = getNumPlayers();
        int i = 1;

        foreach (KeyValuePair<int, Player> entry in playerDict)
        {
            Player player = entry.Value;
            if (player != null)
            {
                string lastAction = player.actionIdString;
                if (lastAction != "empty")
                {
                    if (i != numberOfAgents)
                        line += player.getPlayerNumber() + "," + lastAction + ";";
                    else
                        line += player.getPlayerNumber() + "," + lastAction;
                }
            }

            ++i;
        }

        if (line != "")
        {
            line = "AC:" + line;
        }

        return line;
    }

    public void initInitialPositions(Grid grid)
    {
        //porque as bordas são paredes
        float maxX = grid.GetGridSizeX() - 2.0f;
        float maxY = grid.GetGridSizeY() - 2.0f;

        staticInitialPositions  = new Vector3[] {  new Vector3(1.0f, 0.5f, maxY),
                                             new Vector3(maxX, 0.5f, maxY) ,
                                             new Vector3(maxX, 0.5f, 1.0f) ,
                                             new Vector3(1.0f, 0.5f, 1.0f)};

        restartInitList();
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

    Vector3 getOppositeInitialPosition(int index)
    {
        if (index == 0)
            return staticInitialPositions[2];
        else if (index == 1)
            return staticInitialPositions[3];
        else if (index == 2)
            return staticInitialPositions[0];
        else
            return staticInitialPositions[1];
    }

    private void restartInitList()
    {
        initPosList.Clear();
        for (int i = 0; i < staticInitialPositions.Length; i++)
        {
            initPosList.Add(staticInitialPositions[i]);
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

    /*public void calculateDistanceEnemyPositionAndApplyRewards(Player agent)
    {
        foreach (KeyValuePair<int, Player> entry in playerDict)
        {
            Player enemy = entry.Value;
            if (!enemy.dead && enemy.getPlayerNumber() != agent.getPlayerNumber())
            {
                float distance = Vector2.Distance(agent.GetGridPosition(), enemy.getLastOldGridPosition());
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
    }*/

    //chamar no momento da observação
    public List<DistanceReward> CalculateDistanceEnemyPositionRewards(Player agent)
    {
        List<DistanceReward> distanceRewardList = new List<DistanceReward>();

        foreach (KeyValuePair<int, Player> entry in playerDict)
        {
            Player enemy = entry.Value;
            if (/*!enemy.dead &&*/ enemy.getPlayerNumber() != agent.getPlayerNumber())
            {
                DistanceReward distanceReward = new DistanceReward();
                float distance = Util.ManhattanDistance(agent.GetGridPosition(), enemy.GetGridPosition());
                if (distance < distanceStructureDict[agent.getPlayerNumber()][enemy.getPlayerNumber()].closestDistance)
                {
                    distanceStructureDict[agent.getPlayerNumber()][enemy.getPlayerNumber()].closestDistance = distance;
                    distanceReward.applyRewardClosest = true;
                    distanceReward.applyRewardClosestMessage = "Agente" + agent.getPlayerNumber() + " melhor aproximacao do inimigo " + enemy.getPlayerNumber();
                }

                if (distance < distanceStructureDict[agent.getPlayerNumber()][enemy.getPlayerNumber()].previousDistance)
                {
                    distanceReward.applyRewardApproach = true;
                    distanceReward.applyRewardApproachMessage = "Agente" + agent.getPlayerNumber() + " se aproximou do inimigo " + enemy.getPlayerNumber();
                }
                else if (distance > distanceStructureDict[agent.getPlayerNumber()][enemy.getPlayerNumber()].previousDistance)
                {
                    distanceReward.applyRewardFar = true;
                    distanceReward.applyRewardFarMessage = "Agente" + agent.getPlayerNumber() + " se distanciou do inimigo " + enemy.getPlayerNumber();
                }

                distanceStructureDict[agent.getPlayerNumber()][enemy.getPlayerNumber()].previousDistance = distance;
                distanceRewardList.Add(distanceReward);
            }
        }

        return distanceRewardList;
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

                    float distance = Util.ManhattanDistance(entry.Value.GetGridPosition(), enemyEntry.Value.GetGridPosition());
                    DistanceStructure dStruct = new DistanceStructure();
                    dStruct.closestDistance = distance;
                    dStruct.previousDistance = distance;

                    distanceStructureDict[playerNumber].Add(playerNumberEnemy, dStruct);
                }
            }
        }
    }

    public void clearDistanceStructuresForReward()
    {
        distanceStructureDict.Clear();
    }

    public void resetLevelIfPlayerOneDie()
    {
        if (!lastManFound)
        {
            if (!containsPlayer(1))
            {
                foreach (KeyValuePair<int, Player> entry in playerDict)
                {
                    entry.Value.Done();
                }
            }
        }
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
                playerDict[lastManIndex].lastMan = true;
                lastManAgent = "Agente " + playerDict[lastManIndex].getPlayerNumber();
                lastManAgentResult = (uint)playerDict[lastManIndex].getPlayerNumber();
                lastManFound = true;
            }
        }
    }

    public bool isUpdating()
    {
        return updating;
    }

    public void setIsUpdating(bool _isUpdating)
    {
        updating = _isUpdating;
    }

    public bool updateAgents()
    {
        if (!updating)
        {
            updating = true;
            bool areThereUpdate = false;

            if (randomizeIterationOfAgents)
            {
                List<Player> playerList = new List<Player>(playerDict.Values);
                List<Player> randomList = new List<Player>();
                while (playerList.Count > 0)
                {
                    int rand = UnityEngine.Random.Range(0, playerList.Count);
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
                //updating = false;
                return true;
            }
            else
            {
                if (!lastManFound)
                {
                    lastManAgent = "draw";
                    lastManAgentResult = 0;
                }
                // precisamos resetar a cena
                //updating = false;
                return false;
            }
        }

        return false;
    }

    public void setRandomizeIterationOfAgents(bool value)
    {
        randomizeIterationOfAgents = value;
    }
}
