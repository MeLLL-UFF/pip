using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class BombermanDecision : MonoBehaviour, Decision {

    private ReplayReader replayReader;
    public string replayFileName;

    public void Awake()
    {
        replayReader = new ReplayReader(replayFileName);
    }

    void OnApplicationQuit()
    {
        if (replayReader != null)
            replayReader.finish();
    }

    bool checkObservations(List<float> vectorObs, ReplayReader.ReplayStep replayStep)
    {
        if (vectorObs == null)
        {
            //Debug.Log("vectorObs nulo");
            return false;
        }

        if (replayStep == null)
        {
            //Debug.Log("replayStep nulo");
            return false;
        }
        else
        {
            if (replayStep.observationGrid == null)
            {
                //Debug.Log("observationGrid nulo");
                return false;
            }
        }

        if (vectorObs.Count != replayStep.observationGrid.Length)
        {
            //Debug.Log("Vetores de tamanhos diferentes");
            return false;
        }

        for(int i = 0; i < vectorObs.Count; i++)
        {
            int obs = (int)vectorObs[i];
            if (obs != replayStep.observationGrid[i])
                return false;
        }

        return true;
    }

    float[] findMatch(List<float> vectorObs)
    {
        int searchAll = 0;
        ReplayReader.ReplayStep replayStep = replayReader.readStep();
        while(replayStep == null)
        {
            //Debug.Log("Fim de arquivo de replay");
            replayReader.finish();
            replayReader.reopen();
            replayStep = replayReader.readStep();
        }
  

        while (!checkObservations(vectorObs, replayStep))
        {
            replayStep = replayReader.readStep();
            if (replayStep == null)
            {
                searchAll++;

                if (searchAll <= 1)
                {
                    //Debug.Log("Procurando matche");
                    replayReader.finish();
                    replayReader.reopen();
                }
                else
                {
                    //Debug.Log("Nao foi encontrado matche. Evitando Loop infinito");
                    return new float[1] { 0 };
                }
            }
        }

        return new float[1] { replayStep.actionId };
    }

    public float[] Decide(
        List<float> vectorObs,
        List<Texture2D> visualObs,
        float reward,
        bool done,
        List<float> memory)
    {
        if (gameObject.GetComponent<Brain>().brainParameters.vectorActionSpaceType
            == SpaceType.discrete)
        {
            ReplayReader.ReplayStep replayStep = replayReader.readStep();
            if (replayStep == null)
            {
                //Debug.Log("Fim de arquivo de replay");
                replayReader.finish();
                replayReader.reopen();
            }
            else
            {
                if (checkObservations(vectorObs, replayStep))
                    return new float[1] { replayStep.actionId };
                else
                {
                    return findMatch(vectorObs);
                }
            }
        }

        // If the vector action space type is discrete, then we don't do anything.     
        return new float[1] { 0 };
    }

    public List<float> MakeMemory(
        List<float> vectorObs,
        List<Texture2D> visualObs,
        float reward,
        bool done,
        List<float> memory)
    {
        return new List<float>();
    }
}
