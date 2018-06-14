﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager {

    bool initialized = false;
    private Player agent1;
    private Player agent2;
    private int deadCount;
    private int targetCount;

    public PlayerManager()
    {
        if (!initialized)
        {
            //Debug.Log("PlayerManager inicializado");
            deadCount = 0;
            targetCount = 0;
            initialized = true;
        }
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

    public void addTargetCount()
    {
        targetCount++;
    }

    public int getTargetCount()
    {
        return targetCount;
    }

    public void clearTargetCount()
    {
        targetCount = 0;
    }

    public void setAgent1(Player p)
    {
        agent1 = p;
    }

    public void setAgent2(Player p)
    {
        agent2 = p;
    }

    public Player getAgent1()
    {
        return agent1;
    }

    public Player getAgent2()
    {
        return agent2;
    }
}
