﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Config {

    //Recompensas
    //public static float REWARD_DIE = -1f;
    public static float REWARD_DIE = -0.5f;

    public static float REWARD_KILL_ENEMY = 1f;

    //public static float REWARD_GOAL = 1f;
    public static float REWARD_GOAL = 1f;
    public static float REWARD_TEAM_GOAL = 1f;
    public static float REWARD_LAST_MAN = 1f;

    public static float REWARD_BLOCK_DESTROY = 0.08f;
    //public static float REWARD_BLOCK_DESTROY = 0.08f;
    public static float REWARD_CLOSEST_DISTANCE = 0.08f; //pode ser visto como subgoals
    //public static float REWARD_CLOSEST_DISTANCE = 0.06f;
    //public static float REWARD_APPROACHED_DISTANCE = 0.001f;
    public static float REWARD_APPROACHED_DISTANCE = 0.002f;
    //public static float REWARD_FAR_DISTANCE = -0.001f;
    public static float REWARD_FAR_DISTANCE = -0.002f;
    //public static float REWARD_TIME_PENALTY = -0.001f;
    public static float REWARD_TIME_PENALTY = -0.01f;

    //public static float REWARD_STOP_ACTION = -0.03f;
    public static float REWARD_STOP_ACTION = 0.0f;

    //public static float REWARD_INVALID_WALK_ACTION = -0.02f;
    public static float REWARD_INVALID_WALK_ACTION = -0.00f;
    //public static float REWARD_VALID_WALK_ACTION = 0.02f;
    public static float REWARD_VALID_WALK_POSITION = 0.002f;

    //public static float REWARD_INVALID_HAMMER_ACTION = -0.02f;
    public static float REWARD_INVALID_HAMMER_ACTION = -0.00f;
    //public static float REWARD_VALID_HAMMER_ACTION = 0.02f;
    public static float REWARD_VALID_HAMMER_ACTION = 0.00f;

    //public static float REWARD_INVALID_BOMB_ACTION = -0.02f;
    public static float REWARD_INVALID_BOMB_ACTION = -0.00f;
    //public static float REWARD_VALID_BOMB_ACTION = 0.02f;
    public static float REWARD_VALID_BOMB_ACTION = 0.00f;

    //public static float REWARD_DANGER_AREA = -0.0002f;
    public static float REWARD_DANGER_AREA = -0.0002f;
    //public static float REWARD_SAFE_AREA = 0.0008f;
    public static float REWARD_SAFE_AREA = 0.0008f;

    public static float REWARD_MAX_STEP_REACHED = -1.0f;

    public static float REWARD_CORRECT_TEACHER_ACTION = 0.6f;
    public static float REWARD_WRONG_TEACHER_ACTION = -0.6f;

    //Max step per agent
    public static int MAX_STEP_PER_AGENT = 300;   // 300 default. Nosso cenário é maior que do ICAART (150), logo esse valor precisa ser maior

    //tempo para a bomba explodir (contínuo)
    //public static float BOMB_TIMER = 3.0f;
    //tempo para a bomba sumir após explodir
    public static float BOMB_TIMER_AFTER_DESTROY = 0.3f;
    //tempo para o fogo da explosão sumir após explosão
    public static float EXPLOSION_TIMER = 0.55f;
    public static float EXPLOSION_TIMER_DISCRETE = 2;
    //tempo para a bomba explodir (discreto). Número de iterações
    public static int BOMB_TIMER_DISCRETE = 6;
}
