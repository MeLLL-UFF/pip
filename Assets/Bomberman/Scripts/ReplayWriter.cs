using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class ReplayWriter {

    bool initialized = false;
    private string fileName = "./replays/replay_scenario_";
    private StreamWriter sw;
    private string tabFormat;

    public ReplayWriter(int agentId, int scenarioId)
    {
        if (!initialized)
        {
            if (!Directory.Exists("./replays/"))
            {
                Directory.CreateDirectory("./replays/");
            }

            sw = new StreamWriter(fileName + scenarioId + "_agent_" + agentId + DateTime.Now.ToString("_yyyy_MM_dd_HH_mm_ss") + ".txt", true);
            tabFormat = "\t\t";
            initialized = true;
        }
    }

    public void printStep(string observationGrid, string actionId)
    {
        sw.WriteLine(observationGrid + ";" + actionId);
    }

    public void finish()
    {
        sw.WriteLine("Fim");
        sw.Close();
    }
}
