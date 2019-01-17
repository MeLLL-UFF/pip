using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class ReplayReader {

    bool initialized = false;
    private string fileFolder = "./replays/";
    private StreamReader sr;
    private int counter;
    private string fileName;

    public class ReplayStep
    {
        public int[] observationGrid;
        public int actionId;
        public string command;
        public string seqId;

        public ReplayStep()
        {
            seqId = "null";
        }
    }

    public ReplayReader(string filename)
    {
        if (!initialized)
        {
            counter = 0;
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

            initialized = true;
        }
    }

    public void reopen()
    {
        counter = 0;
        sr = new StreamReader(fileFolder + fileName);
    }

    public ReplayStep readStep()
    {
        ReplayStep replayStep = new ReplayStep();
        string line = sr.ReadLine();
        if (line != null && line != "Fim")
        {
            if (line.Substring(0, 3).Equals("SEQ"))
            {
                //Debug.Log("Substring SEQ encontrada");
                replayStep.command = "SEQ";
                replayStep.seqId = line;
                //Debug.Log("Interno: " + replayStep.command);

                counter = 0;

                return replayStep;
            }
            else
            {
                string[] parts = line.Split(';');
                if (parts.Length != 2)
                {
                    replayStep.command = "CORRUPTED";
                    return replayStep;
                }

                replayStep.command = "STEP";
                string firstPart = parts[0];
                string actionPart = parts[1];

                replayStep.actionId = Int32.Parse(actionPart);

                string[] observationParts = firstPart.Split(',');
                int[] obs = new int[observationParts.Length];
                for (int i = 0; i < observationParts.Length; ++i)
                {
                    obs[i] = Int32.Parse(observationParts[i]);
                }
                replayStep.observationGrid = obs;

                counter++;
                return replayStep;
            }
        }
        else
        {
            replayStep.command = "END";
            return replayStep;
        }
    }

    public void finish()
    {
        if (sr != null)
            sr.Close();
    }
}
