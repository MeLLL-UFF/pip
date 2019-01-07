using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class LogManager {

    bool initialized = false;

    // variável foi criada porque agora tem muitos cenários, se não tivermos controle vai gerar muito log
    bool disabled = true;


    private string fileName = "./logdir/logtest_scenario_";
    private StreamWriter sw;
    private long countStep;
    private string tabFormat;

    internal LogManager(int scenarioId)
    {
        if (!initialized)
        {
            if (!Directory.Exists("./logdir/"))
            {
                Directory.CreateDirectory("./logdir/");
            }

            if (!disabled)
            {
                sw = new StreamWriter(fileName + scenarioId + DateTime.Now.ToString("_yyyy_MM_dd_HH_mm_ss_fff") + ".txt", true);
                countStep = 1;
                tabFormat = "\t\t";
                initialized = true;
            }
            
        }
    }

    public void separator()
    {
        if (!disabled)
        {
            sw.WriteLine("-------------------------------------------------------------------------------------");
        }
    }

    public void simplePrint(string message)
    {
        if (!disabled)
        {
            sw.WriteLine(message);
        }
    }

    public void print(string message)
    {
        if (!disabled)
        {
            sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + message);
        }
    }

    public void print(string message, string prefix)
    {
        if (!disabled)
        {
            sw.WriteLine(DateTime.Now.ToString(prefix + "yyyy-MM-dd HH:mm:ss.fff") + " " + message);
        }
    }

    public void rewardPrint(string message, float reward)
    {
        if (!disabled)
        {
            string result = "R: " + reward + " -> " + message;
            print(result);
        }
    }

    public void rewardResumePrint(float stepReward, float episodeReward)
    {
        if (!disabled)
        {
            simplePrint("\t\t stepReward: " + stepReward + " episodeReward: " + episodeReward);
        }
    }

    public void globalStepPrint(int academyStep)
    {
        if (!disabled)
        {
            if (countStep != 0 && countStep % 50000 == 0)
            {
                sw.Close();
                sw = new StreamWriter(fileName + DateTime.Now.ToString("_yyyy_MM_dd_HH_mm_ss_fff") + ".txt", true);
            }

            print("Global Step " + countStep);
            print("Academy Step " + academyStep);
            ++countStep;
        }
    }

    public void localStepPrint(Player player)
    {
        if (!disabled)
        {
            print("Agent " + player.getPlayerNumber() + " Step " + player.getLocalStep());
            //print("Recompensas:", "\n");
        }
    }

    public void episodePrint(int epCount)
    {
        if (!disabled)
        {
            print("Global Episode " + epCount, "\n");
        }
    }

    public void localEpisodePrint(int epCount, Player player)
    {
        if (!disabled)
        {
            print("Local Episode of agent " + player.getPlayerNumber() + ": " + epCount);
        }
    }

    public void statePrint(string agentName, Vector2 agentGridPos, /*Vector2 velocity,*/ string grid, bool canDropBombs, bool isInDanger, bool existBombs)
    {
        if (!disabled)
        {
            string result = "Estado Atual - " + agentName + "\n";
            result += tabFormat + "pos: " + agentGridPos + "\n";

            //result += tabFormat + "vel: " + velocity + "\n";
            result += tabFormat + "canDropBombs: " + canDropBombs + "\n";
            result += tabFormat + "isInDanger: " + isInDanger + "\n";
            result += tabFormat + "existBombs: " + existBombs + "\n";
            result += tabFormat + "grid:" + "\n" + grid;


            separator();
            print(result);
        }
    }

    public void actionPrint(string agentName, ActionType action)
    {
        if (!disabled)
        {
            string result = "Acoes Atuais - " + agentName + "\n";
            result += tabFormat + "Acao: " + action.ToString() + "\n";

            print(result, "\n");
        }
    }

    public void finish()
    {
        if (!disabled)
        {
            sw.WriteLine("Fechou");
            sw.Close();
        }
    }
}
