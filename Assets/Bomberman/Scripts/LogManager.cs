using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class LogManager {

    static bool initialized = false;
    private string fileName = "./logdir/logtest";
    private StreamWriter sw;
    private long countStep;
    private string tabFormat;

    internal LogManager()
    {
        if (!initialized)
        {
            if (!Directory.Exists("./logdir/"))
            {
                Directory.CreateDirectory("./logdir/");
            }

            sw = new StreamWriter(fileName + DateTime.Now.ToString("_yyyy_MM_dd_HH_mm_ss_fff") + ".txt", true);
            countStep = 0;
            tabFormat = "\t\t";
            initialized = true;
        }
    }

    public void separator()
    {
        sw.WriteLine("-------------------------------------------------------------------------------------");
    }

    public void simplePrint(string message)
    {
        sw.WriteLine(message);
    }

    public void print(string message)
    {
        sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + message);
    }

    public void print(string message, string prefix)
    {
        sw.WriteLine(DateTime.Now.ToString(prefix + "yyyy-MM-dd HH:mm:ss.fff") + " " + message);
    }

    public void rewardPrint(string message, float reward)
    {
        string result = "R: " + reward + " -> " + message;
        print(result);
    }

    public void rewardResumePrint(float stepReward, float episodeReward)
    {
        simplePrint("\t\t stepReward: " + stepReward + " episodeReward: " + episodeReward);
    }

    public void globalStepPrint(int academyStep)
    {
        if (countStep != 0 && countStep % 10000 == 0)
        {
            sw.Close();
            sw = new StreamWriter(fileName + DateTime.Now.ToString("_yyyy_MM_dd_HH_mm_ss_fff") + ".txt", true);
        }

        print("Global Step " + countStep);
        print("Academy Step " + academyStep);
        ++countStep;
    }

    public void localStepPrint(Player player)
    {
        print("Agent Step " + player.GetStepCount());
        print("Recompensas:", "\n");
    }

    public void episodePrint(int epCount)
    {
        print("Global Episode " + epCount, "\n");
    }

    public void localEpisodePrint(int epCount)
    {
        print("Local Episode " + epCount);
    }

    public void statePrint(string agentName, Vector2 agentGridPos, Vector2 targetGridPos, Vector2 velocity, string playerGrid, string blocksGrid, string bombsGrid)
    {
        string result = "Estado Atual - " + agentName + "\n";
        result += tabFormat + "pos: " + agentGridPos + "\n";
        result += tabFormat + "tar: " + targetGridPos + "\n";
        result += tabFormat + "vel: " + velocity + "\n";
        result += tabFormat + "player grid:" + "\n" + playerGrid;
        result += tabFormat + "blocks grid:" + "\n" + blocksGrid;
        result += tabFormat + "bombs grid:" + "\n" + bombsGrid;

        separator();
        print(result);
    }

    public void actionPrint(string agentName, bool key_W, bool key_S, bool key_D, bool key_A, bool putBomb, bool canDropBombs, bool stopped)
    {
        string result = "Acoes Atuais - " + agentName + "\n";
        result += tabFormat + "Parado: " + stopped + "\n";
        result += tabFormat + "W: " + key_W + " S: " + key_S + " D: " + key_D + " A: " + key_A + "\n";
        result += tabFormat + "canDropBombs: " + canDropBombs + " putBomb: " + putBomb;

        print(result, "\n");
    }

    public void finish()
    {
        sw.WriteLine("Fechou");
        sw.Close();
    }
}
