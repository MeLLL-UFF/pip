using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class BombermanOnlyOneDecision : MonoBehaviour, Decision {

    private ReplayReader replayReader1;
    private string replayFileName1 = "onlyAgent1.txt";
    private string seqId1;
    public bool finishSeqs1;


    public void Awake()
    {
        replayReader1 = new ReplayReader(replayFileName1);
        finishSeqs1 = false;
    }

    void OnApplicationQuit()
    {
        if (replayReader1 != null)
            replayReader1.finish();
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
            if (replayStep.command == "CORRUPTED")
                return false;

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

    float[] findMatch(List<float> vectorObs, ref ReplayReader replayReader, ref string seqId)
    {
        int searchAll = 0;
        //lê linha do arquivo
        ReplayReader.ReplayStep replayStep = replayReader.readStep();
        //Debug.Log("Primeiro: " + replayStep.command);

        //se for última linha, reabre o arquivo e lê a primeira linha
        while (replayStep.command.Equals("END"))
        {
            //Debug.Log("Fim de arquivo de replay");
            replayReader.finish();
            replayReader.reopen();
            replayStep = replayReader.readStep();
        }

        //se linha for de sequencia, armazena o seqID e lê a próxima linha
        //Debug.Log("Segundo: " + replayStep.command);
        if (replayStep.command.Equals("SEQ"))
        {
            //Debug.Log("Entrou");
            seqId = replayStep.seqId;
            //Debug.Log("Sequencia iniciada: " + seqId);
            replayStep = replayReader.readStep();
        }

        //procura match entre observação e arquivo de replay
        //while (!checkObservations(vectorObs, replayStep))
        {
            //testa pra ver se é a ultima linha do arquivo. Se for incrementa contador e reabre o arquivo.
            /*if (replayStep.command.Equals("END"))
            {
                searchAll++;

                if (searchAll <= 1)
                {
                    //Debug.Log("Procurando matche");
                    replayReader.finish();
                    replayReader.reopen();
                    replayStep = replayReader.readStep();
                }
                else
                {
                    //Debug.Log("Nao foi encontrado matche. Evitando Loop infinito");
                    return new float[1] { 0 };
                }
            }*/

            //se linha for de sequencia, armazena o seqID e lê a próxima linha
            /*if (replayStep.command.Equals("SEQ"))
            {
                
                //procura por sequencia corrente
                while (!replayStep.seqId.Equals(seqId))
                {
                    replayStep = replayReader.readStep();

                    if (replayStep.command.Equals("END"))
                    {
                        searchAll++;

                        if (searchAll <= 1)
                        {
                            replayReader.finish();
                            replayReader.reopen();
                            replayStep = replayReader.readStep();
                        }
                        else
                        {
                            return new float[1] { 0 };
                        }
                    }
                }

                //lê a proxima linha que é uma de STEP
                replayStep = replayReader.readStep();
            }
            else
            {
                //Lê a proxima linha
                replayStep = replayReader.readStep();
            }*/
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
            if (!done)
            {
                if (!finishSeqs1)
                    return findMatch(vectorObs, ref replayReader1, ref seqId1);
            }

            return new float[1] { 0 };
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
