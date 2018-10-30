using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class BinaryNode : BaseNode {

    private StateType binary;

    public override int getBinary()
    {
        return (int)binary;
    }

    public override string getStringBinaryArray()
    {
        string s = StateTypeExtension.getIntBinaryString(binary);

        return s;
    }

    public override int getFreeBreakableObstructedCell()
    {
        if (hasFlag(StateType.ST_Wall))
        {
            return -1;
        }
        else if (hasFlag(StateType.ST_Block))
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public override int getPositionAgent(int playerNumber)
    {
        StateType testFlag = StateType.ST_EnemyAgent;

        if (playerNumber == 1)
            testFlag = StateType.ST_Agent;

        if (hasFlag(testFlag))
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public override int getPositionTarget()
    {
        if (hasFlag(StateType.ST_Target))
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public override bool getDangerPosition()
    {
        /*if (hasFlag(StateType.ST_Danger))
            return true;*/

        return false;
    }

    public override void addFlags(List<StateType> flags)
    {
        for (int i = 0; i < flags.Count; ++i)
            addFlag(flags[i]);
    }

    public override void addFlag(StateType stateType)
    {
        if (StateTypeExtension.existsFlag(stateType))
        {
            binary = binary | stateType;
        }
    }

    public override void removeFlag(StateType stateType)
    {
        if (StateTypeExtension.existsFlag(stateType))
        {
            binary = binary & (~stateType);
        }
    }

    public override void clearAllFlags()
    {
        binary = StateType.ST_Empty;
    }

    public override bool hasFlag(StateType stateType)
    {
        if (stateType != StateType.ST_Empty)
        {
            if (StateTypeExtension.existsFlag(stateType))
            {
                return ((binary & stateType) == stateType ? true : false);
            }
        }
        else
        {
            if (binary == stateType)
                return true;
        }
        return false;
    }

    //função não testa ST_Empty
    public override bool hasSomeFlag(StateType flags)
    {
        if (StateTypeExtension.existsFlag(flags))
        {
            return ((binary & flags) != 0 ? true : false);
        }
        
        return false;
    }

    public BinaryNode(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY, int _penalty, List<StateType> stateTypes)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        movementPenalty = _penalty;

        addFlags(stateTypes);

        cost = _penalty;
    }
}
