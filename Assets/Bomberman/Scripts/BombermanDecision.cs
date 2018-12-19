using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System;

public class BombermanDecision : MonoBehaviour, Decision {

    MapController mapController;
    public float[] Decide(  List<float> vectorObs,
                            List<Texture2D> visualObs,
                            float reward,
                            bool done,
                            List<float> memory,
                            int playerNumber)
    {
        if (gameObject.GetComponent<Brain>().brainParameters.vectorActionSpaceType == SpaceType.discrete)
        {
            if (!done)
            {
                if (mapController.currentReplayStep.agentActionMap.ContainsKey(playerNumber.ToString()))
                {
                    int action = mapController.currentReplayStep.agentActionMap[playerNumber.ToString()];

                    return new float[1] { action };
                }
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

    public void setMapController(MapController _mapController)
    {
        mapController = _mapController;
    }
} 