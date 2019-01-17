using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Config {

    //Recompensas

    //public static float REWARD_GOAL = 1f;
    public static float REWARD_GOAL = 1f;

    public static float REWARD_BLOCK_DESTROY = 0.51f;
    public static float REWARD_CLOSEST_DISTANCE = 0.51f; //pode ser visto como subgoals
    public static float REWARD_APPROACHED_DISTANCE = 0.007625f;
    public static float REWARD_FAR_DISTANCE = -0.007625f;
    public static float REWARD_TIME_PENALTY = -0.001f;

    public static float REWARD_STOP_ACTION = -0.03f;

    public static float REWARD_INVALID_WALK_ACTION = -0.02f;
    public static float REWARD_VALID_WALK_POSITION = 0.002f;

    public static float REWARD_INVALID_HAMMER_ACTION = -0.01f;
    public static float REWARD_VALID_HAMMER_ACTION = 0.02f;

    public static float REWARD_MAX_STEP_REACHED = -1.0f;

    public static float REWARD_CORRECT_TEACHER_ACTION = 0.02f;
    public static float REWARD_WRONG_TEACHER_ACTION = -0.02f;

    //Max step per agent
    public static int MAX_STEP_PER_AGENT = 200;
}
