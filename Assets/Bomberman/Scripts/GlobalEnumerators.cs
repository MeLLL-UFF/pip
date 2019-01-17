using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionType
{
    AT_Wait =           0,
    AT_Up =             1,
    AT_Down =           2,
    AT_Left =           3,
    AT_Right =          4,

    //AT_Bomb =         5,

    AT_Hammer_Up =      5,
    AT_Hammer_Down =    6,
    AT_Hammer_Left =    7,
    AT_Hammer_Right =   8,


    AT_Size = 9
}

public class ActionTypeExtension
{
    public static ActionType convert(int actionInt)
    {
        ActionType actionType = (ActionType)actionInt;

        return actionType;
    }
}

[Flags] public enum StateType
{
    ST_Empty    =   0,          // 0        00000000
    ST_Wall     =   1 << 0,     // 1        00000001
    ST_Block    =   1 << 1,     // 2        00000010
    
    ST_Agent1   =   1 << 2,     // 4        00000100
    ST_Target   =   1 << 3,     // 8

    //apenas quando há bomba
    /*ST_Bomb     =   1 << 5,     // 32        
    ST_Fire     =   1 << 6,     // 64
    ST_Danger   =   1 << 7,     // 128   */  
    // -------------------------------


    ST_All = (ST_Wall | ST_Block | ST_Agent1 | ST_Target /*| ST_Bomb | ST_Fire | ST_Danger*/),
    ST_Size = 9
}

public class StateTypeExtension
{
    public static int convertToIntOrdinal(StateType stateType)
    {
        if (stateType == StateType.ST_Empty)
            return 0;

        float log2 = Mathf.Log((int)stateType, 2);

        return ((int)log2 + 1);
    }

    public static bool existsFlag(StateType stateType)
    {
        if ((stateType & StateType.ST_All) != 0)
        {
            return true;
        }

        return false;
    }

    public static string getIntBinaryString(StateType stateType)
    {
        return Convert.ToString((int)stateType, 2).PadLeft((int)StateType.ST_Size - 1, '0');
    }
}

public enum GridType
{
    GT_Binary = 0,
    //GT_OneHot = 1,
    GT_Hybrid = 2
}

public enum GridSentData
{
    GSD_All = 0,     //somente um grid é passado para o espaço de observações
    GSD_Divided = 1  //3 grids específicos são enviados para o espaço de observações
}
