using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Config {

    //Recompensas
    public static float REWARD_DIE = -1f;
    public static float REWARD_KILL_FRIEND = -1f;
    public static float REWARD_GOAL = 1f;
    public static float REWARD_TEAM_GOAL = 1f;
    public static float REWARD_BLOCK_DESTROY = 0.21f;
    //public static float REWARD_BLOCK_DESTROY = 0.51f;
    public static float REWARD_CLOSEST_DISTANCE = 0.31f;
    //public static float REWARD_CLOSEST_DISTANCE = 0.61f;
    public static float REWARD_APPROACHED_DISTANCE = 0.007625f;
    public static float REWARD_FAR_DISTANCE = -0.007625f;
    public static float REWARD_TIME_PENALTY = -0.001f;
    public static float REWARD_INVALID_ACTION = -0.02f;
    public static float REWARD_INVALID_BOMB_ACTION = -0.02f;
    public static float REWARD_MAX_STEP_REACHED = -0.5f;

    //Max step per agent
    public static int MAX_STEP_PER_AGENT = 200;
}
