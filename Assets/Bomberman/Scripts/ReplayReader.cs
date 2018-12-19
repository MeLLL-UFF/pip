using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class ReplayReader {

    bool initialized = false;
    private string fileFolder = "./replays/";
    private StreamReader sr;
    private string fileName;

    public class ReplayStep
    {
        public ReplayCommandLine command;
        public string epId;
        public int numberOfAgents;
        public Dictionary<string, Vector2Int> agentInitPositionMap;
        public Dictionary<string, int> agentActionMap;
        public int bombIteration;
        public bool hasCreatedBomb;
        public List<Vector2Int> bombList;

        public ReplayStep()
        {
            hasCreatedBomb = false;
            epId = "0";
            agentInitPositionMap = new Dictionary<string, Vector2Int>();
            agentActionMap = new Dictionary<string, int>();
            bombList = new List<Vector2Int>();
        }
    }

    ReplayStep lastLine;
    bool isSynchronized;

    public ReplayStep getLastLine()
    {
        return lastLine;
    }

    public ReplayReader(string filename)
    {
        if (!initialized)
        {
            if (!Directory.Exists(fileFolder))
            {
                Directory.CreateDirectory(fileFolder);
            }

            fileName = filename;
            try
            {
                sr = new StreamReader(fileFolder + filename);
            }
            catch (FileNotFoundException ex)
            {
                Debug.Log("Arquivo de replay " + fileFolder + fileName + " nao foi encontrado");
                Debug.Log(ex.Message);
            }

            isSynchronized = true;
            initialized = true;
        }
    }

    public void reopen()
    {
        sr.Close();
        sr = new StreamReader(fileFolder + fileName);
        isSynchronized = true;
    }

    public ReplayStep nextReplayStepEpisodeOrReopen()
    {
        ReplayStep rStep = readStep(ReplayCommandLine.RCL_Episode);

        if (rStep.command == ReplayCommandLine.RCL_Episode)
            return rStep;

        while(rStep.command != ReplayCommandLine.RCL_Episode && rStep.command != ReplayCommandLine.RCL_End)
        {
            rStep = readStep(ReplayCommandLine.RCL_Episode, true);
        }

        if (rStep.command == ReplayCommandLine.RCL_End)
        {
            reopen();
            rStep = readStep(ReplayCommandLine.RCL_Episode);
        }

        return rStep;
    }

    public ReplayStep readStep(ReplayCommandLine requiredCommand, bool force = false)
    {
        if (isSynchronized || force)
        {
            ReplayStep replayStep = new ReplayStep();
            string line = sr.ReadLine();

            if (line != null && line != "END")
            {
                if (line.Substring(0, 2).Equals("EP"))
                {
                    replayStep.command = ReplayCommandLine.RCL_Episode;

                    string[] parts = line.Split(':');
                    replayStep.epId = parts[1];

                    //return replayStep;
                }
                else if (line.Substring(0, 2).Equals("NA"))
                {
                    replayStep.command = ReplayCommandLine.RCL_NumberOfAgents;

                    string[] parts = line.Split(':');
                    replayStep.numberOfAgents = Int32.Parse(parts[1]);

                    //return replayStep;
                }
                else if (line.Substring(0, 2).Equals("IP"))
                {
                    replayStep.command = ReplayCommandLine.RCL_InitialPositions;

                    string[] parts = line.Split(':');
                    string positions = parts[1];
                    string[] agentPosParts = positions.Split(';');
                    for (int i = 0; i < agentPosParts.Length; ++i)
                    {
                        string[] agentPos = agentPosParts[i].Split(',');
                        Vector2Int pos = new Vector2Int(Int32.Parse(agentPos[1]), Int32.Parse(agentPos[2]));
                        replayStep.agentInitPositionMap.Add(agentPos[0], pos);
                    }

                    //return replayStep;
                }
                else if (line.Substring(0, 2).Equals("AC"))
                {
                    replayStep.command = ReplayCommandLine.RCL_Actions;

                    string[] parts = line.Split(':');
                    string positions = parts[1];
                    string[] agentActionParts = positions.Split(';');
                    for (int i = 0; i < agentActionParts.Length; ++i)
                    {
                        string[] agentAction = agentActionParts[i].Split(',');
                        replayStep.agentActionMap.Add(agentAction[0], Int32.Parse(agentAction[1]));
                    }

                    //return replayStep;
                }
                else if (line.Substring(0, 2).Equals("BO"))
                {
                    replayStep.command = ReplayCommandLine.RCL_BombPositions;

                    string[] parts = line.Split(':');
                    string positions = parts[1];
                    string[] agentActionParts = positions.Split(';');

                    replayStep.bombIteration = Int32.Parse(agentActionParts[0]);
                    replayStep.hasCreatedBomb = Int32.Parse(agentActionParts[1]) == 1 ? true : false;

                    if (replayStep.hasCreatedBomb)
                    {
                        for (int i = 2; i < agentActionParts.Length; ++i)
                        {
                            string[] agentPos = agentActionParts[i].Split(',');
                            Vector2Int pos = new Vector2Int(Int32.Parse(agentPos[0]), Int32.Parse(agentPos[1]));
                            replayStep.bombList.Add(pos);
                        }
                    }

                    //return replayStep;
                }
                else
                {
                    replayStep.command = ReplayCommandLine.RCL_Corrupted;
                    //return replayStep;
                }
            }
            else
            {
                replayStep.command = ReplayCommandLine.RCL_End;
                //return replayStep;
            }

            if (requiredCommand == replayStep.command)
                isSynchronized = true;
            else
                isSynchronized = false;

            lastLine = replayStep;
            return replayStep;
        }
        else
        {
            if (requiredCommand == lastLine.command)
                isSynchronized = true;

            return lastLine;
        }
    }

    public void finish()
    {
        if (sr != null)
            sr.Close();
    }
}
