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
        string line = sr.ReadLine();
        if (line != null && line != "Fim")
        {
            counter++;
            ReplayStep replayStep = new ReplayStep();
            string[] parts = line.Split(';');
            if (parts.Length != 2)
                return null;

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

            return replayStep;
        }
        else
        {
            return null;
        }
    }

    public void finish()
    {
        if (sr != null)
            sr.Close();
    }
}
