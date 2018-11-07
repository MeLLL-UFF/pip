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

    AT_Bomb =         5,

    /*AT_Hammer_Up =      5,
    AT_Hammer_Down =    6,
    AT_Hammer_Left =    7,
    AT_Hammer_Right =   8,*/


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

[Flags] public enum StateType
{
    ST_Empty    =               0,          // 0        00000000
    ST_Wall     =               1 << 0,     // 1        00000001
    ST_Block    =               1 << 1,     // 2        00000010
    
    ST_Agent   =                1 << 2,     // 4        00000100
    ST_EnemyAgent   =           1 << 3,     // 8        00001000
    ST_Target   =               1 << 4,     // 16       00010000

    //apenas quando há bomba
    ST_Bomb     =               1 << 5,     // 32        
    ST_Fire     =               1 << 6,     // 64
    ST_Danger   =               1 << 7,     // 128     
    // -------------------------------

    //usamos apenas essas flags para troca de flag no momento da observação
    ST_Agent1 =                 1 << 8,
    ST_Agent2 =                 1 << 9,
    ST_Agent3 =                 1 << 10,
    ST_Agent4 =                 1 << 11,


    ST_All = (ST_Wall | ST_Block | ST_Agent | ST_EnemyAgent | ST_Target | ST_Bomb | ST_Fire | ST_Danger | ST_Agent1 | ST_Agent2 | ST_Agent3 | ST_Agent4),
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

    public static bool stateTypeHasFlag(StateType stateType, StateType flag)
    {
        if (stateType != StateType.ST_Empty)
        {
            return ((stateType & flag) == flag ? true : false);
        }
        else
        {
            if (stateType == flag)
                return true;
        }
        return false;
    }

    public static float[] convertStateTypeToHybrid(StateType stateType)
    {
        float[] binaryArray = new float[(int)StateType.ST_Size - 1];

        if (stateTypeHasFlag(stateType, StateType.ST_Wall))
        {
            binaryArray[convertToIntOrdinal(StateType.ST_Wall) - 1] = 1;
        }

        if (stateTypeHasFlag(stateType, StateType.ST_Block))
        {
            binaryArray[convertToIntOrdinal(StateType.ST_Block) - 1] = 1;
        }

        if (stateTypeHasFlag(stateType, StateType.ST_Agent))
        {
            binaryArray[convertToIntOrdinal(StateType.ST_Agent) - 1] = 1;
        }

        if (stateTypeHasFlag(stateType, StateType.ST_EnemyAgent))
        {
            binaryArray[convertToIntOrdinal(StateType.ST_EnemyAgent) - 1] = 1;
        }

        if (stateTypeHasFlag(stateType, StateType.ST_Target))
        {
            binaryArray[convertToIntOrdinal(StateType.ST_Target) - 1] = 1;
        }

        if (stateTypeHasFlag(stateType, StateType.ST_Bomb))
        {
            binaryArray[convertToIntOrdinal(StateType.ST_Bomb) - 1] = 1;
        }

        if (stateTypeHasFlag(stateType, StateType.ST_Fire))
        {
            binaryArray[convertToIntOrdinal(StateType.ST_Fire) - 1] = 1;
        }

        if (stateTypeHasFlag(stateType, StateType.ST_Danger))
        {
            binaryArray[convertToIntOrdinal(StateType.ST_Danger) - 1] = 1;
        }

        return binaryArray;
    }

    public static string convertBinaryArrayToString(float[] binaryArray)
    {
        string s = "[" + string.Join("", binaryArray) + "]";
        return s;
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
