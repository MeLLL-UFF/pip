using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionType
{
    AT_Wait =   0,
    AT_Up =     1,
    AT_Down =   2,
    AT_Left =   3,
    AT_Right =  4,
    AT_Bomb =   5,

    AT_Size = 6
}

public class ActionTypeExtension
{
    public static ActionType convert(int actionInt)
    {
        ActionType actionType = (ActionType)actionInt;

        return actionType;
    }
}

public enum StateType
{
    ST_Empty = 0,
    ST_Wall = 1,
    ST_Block = 2,
    ST_Bomb = 3,
    ST_Fire = 4,
    ST_Danger = 5,
    ST_Agent = 6,
    ST_Target = 7,

    ST_Size = 8,
}
