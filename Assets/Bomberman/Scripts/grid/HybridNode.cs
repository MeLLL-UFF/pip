using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class HybridNode : BaseNode {


    // -1 porque estado vazio não conta
    //[0,0,0,1,0,1] Testar se a representação das celulas podem ser feitas com números binários também
    private float[] binaryArray = new float[(int)StateType.ST_Size - 1];

    public override float[] getBinaryArray()
    {
        return binaryArray;
    }

    public override string getStringBinaryArray()
    {
        string s = "[" + string.Join(" ", binaryArray) + "]";
        return s;
    }

    public override void addFlags(List<StateType> flags)
    {
        for (int i = 0; i < flags.Count; ++i)
            addFlag(flags[i]);
    }

    public override void addFlag(StateType stateType)
    {
        if (stateType != StateType.ST_Empty && StateTypeExtension.existsFlag(stateType))
        {
            binaryArray[StateTypeExtension.convertToIntOrdinal(stateType) - 1] = 1;
        }
    }

    public override void removeFlag(StateType stateType)
    {
        if (stateType != StateType.ST_Empty && StateTypeExtension.existsFlag(stateType))
        {
            int temp = StateTypeExtension.convertToIntOrdinal(stateType) - 1;
            binaryArray[temp] = 0;
        }
    }

    public override void clearAllFlags()
    {
        for(int i = 0; i < binaryArray.Length; i++)
        {
            binaryArray[i] = 0;
        }
    }

    public override bool hasFlag(StateType stateType)
    {
        if (StateTypeExtension.existsFlag(stateType))
        {
            if (stateType != StateType.ST_Empty)
                return (binaryArray[StateTypeExtension.convertToIntOrdinal(stateType) - 1] == 1 ? true : false);
            else
            {
                for (int i = 0; i < binaryArray.Length; i++)
                {
                    if (binaryArray[i] == 1)
                        return false;
                }

                return true;
            }
        }
        return false;
    }

    //função não testa ST_Empty
    public override bool hasSomeFlag(List<StateType> flags)
    {
        for(int f = 0; f < flags.Count; f++)
        {
            StateType stateType = flags[f];
            if (StateTypeExtension.existsFlag(stateType) && stateType != StateType.ST_Empty)
            {
                bool result = (binaryArray[StateTypeExtension.convertToIntOrdinal(stateType) - 1] == 1 ? true : false);
                if (result)
                    return true;
            }
        }
        
        return false;
    }

    public HybridNode(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY, int _penalty, List<StateType> stateTypes)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        movementPenalty = _penalty;

        addFlags(stateTypes);

        cost = _penalty;
    }

    private void copyAllFlags(HybridNode hybridNode)
    {

        for (int i = 0; i < hybridNode.binaryArray.Length; i++)
        {
            binaryArray[i] = hybridNode.binaryArray[i];
        }
    }

    // para cópia
    public HybridNode(HybridNode hybridNode)
    {
        walkable = hybridNode.walkable;
        worldPosition = hybridNode.worldPosition;
        gridX = hybridNode.gridX;
        gridY = hybridNode.gridY;
        movementPenalty = hybridNode.movementPenalty;

        copyAllFlags(hybridNode);

        cost = hybridNode.cost;
    }
}
