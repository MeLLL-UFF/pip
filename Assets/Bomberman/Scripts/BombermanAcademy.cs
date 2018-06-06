using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombermanAcademy : Academy {

    Player agent1;
    Player agent2;

    public void setAgent1(Player player)
    {
        agent1 = player;
    }

    public Player getAgent1()
    {
        return agent1;
    }

    public void setAgent2(Player player)
    {
        agent2 = player;
    }

    public Player getAgent2()
    {
        return agent2;
    }

    public bool isReady()
    {
        if ((agent1 != null && !agent1.wasInitialized) || (agent2 != null && !agent2.wasInitialized))
            return true;

        if (agent1 != null && agent2 != null)
        {
            if (agent1.isReady && agent2.isReady)
                return true;
        }
        else
        {
            if (agent1 != null)
                return true;

            if (agent2 != null)
                return true;
        }

        return false;
    }

    public override void AcademyReset()
    {
        if (agent1 != null)
            ServiceLocator.getManager(1).GetLogManager().episodePrint(GetEpisodeCount());

        if (agent2 != null)
            ServiceLocator.getManager(2).GetLogManager().episodePrint(GetEpisodeCount());
    }

    public override void AcademyStep()
    {
        if (agent1 != null)
            ServiceLocator.getManager(1).GetLogManager().globalStepPrint(GetStepCount());

        if (agent2 != null)
            ServiceLocator.getManager(2).GetLogManager().globalStepPrint(GetStepCount());
    }
}
