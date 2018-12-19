using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class ReplayWriter {

    bool initialized = false;
    private string fileName = "./replays/replay_scenario_";
    private StreamWriter sw;

    public ReplayWriter(int scenarioId)
    {
        if (!initialized)
        {
            if (!Directory.Exists("./replays/"))
            {
                Directory.CreateDirectory("./replays/");
            }

            sw = new StreamWriter(fileName + scenarioId + "_" + DateTime.Now.ToString("_yyyy_MM_dd_HH_mm_ss") + ".txt", true);
            initialized = true;
        }
    }

    public void printEpisode(int numEpisode)
    {
        sw.WriteLine("EP:" + numEpisode);
    }

    public void printNumberOfAgents(int numAgents)
    {
        sw.WriteLine("NA:" + numAgents);
    }

    public void printStep(string line)
    {
        sw.WriteLine(line);
    }

    public void printBombs(int iteration, bool hasCreatedBomb, List<Vector2Int> list)
    {
        string line = "BO:" + iteration.ToString() + ";" + (hasCreatedBomb ? "1" : "0");

        for (int i = 0; i < list.Count; ++i)
        {
            line += ";" + list[i].x + "," + list[i].y;
        }

        sw.WriteLine(line);
    }

    public void finish()
    {
        sw.WriteLine("END");
        sw.Close();
    }
}
